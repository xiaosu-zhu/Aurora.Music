using Aurora.Music.Core;
using Aurora.Music.Core.Models;
using Aurora.Music.Core.Storage;
using Aurora.Music.Effects;
using Aurora.Shared.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System.Threading;

namespace Aurora.Music.PlaybackEngine
{
    public sealed class Player : IDisposable, IPlayer
    {
        private MediaPlayer mediaPlayer;
        private MediaPlaybackList mediaPlaybackList;

        private DeviceInformation _autoDevice;

        private object lockable = new object();
        private int _songCountID;
        private IAsyncAction _addPlayListTask;
        private List<Song> currentList;

        private bool? isPlaying;
        private bool newComing;
        private ThreadPoolTimer positionUpdateTimer;

        public bool? IsPlaying { get => isPlaying; }
        private MediaPlaybackState _savedState;
        private TimeSpan _savedPosition;
        private uint _savedIndex;

        public MediaPlayer MediaPlayer { get => mediaPlayer; }

        public double PlaybackRate
        {
            get => mediaPlayer.PlaybackSession.PlaybackRate;
            set => mediaPlayer.PlaybackSession.PlaybackRate = value;
        }
        public bool? IsShuffle { get; private set; }

        public event EventHandler<PositionUpdatedArgs> PositionUpdated;

        public async void ChangeAudioEndPoint(string outputDeviceID)
        {
            if (outputDeviceID.IsNullorEmpty() || outputDeviceID == mediaPlayer.AudioDevice?.Id)
            {
                return;
            }
            var outputDevice = await DeviceInformation.CreateFromIdAsync(outputDeviceID);
            mediaPlayer.AudioDevice = outputDevice;
            Pause();
        }

        public void ChangeVolume(double value)
        {
            mediaPlayer.Volume = value / 100d;
        }

        public event EventHandler<DownloadProgressChangedArgs> DownloadProgressChanged;
        public event EventHandler<PlaybackStatusChangedArgs> PlaybackStatusChanged;
        public event EventHandler<PlayingItemsChangedArgs> ItemsChanged;

        public Player()
        {
            currentList = new List<Song>();

            InitMediaPlayer();

            mediaPlaybackList = new MediaPlaybackList();
            mediaPlaybackList.CurrentItemChanged += MediaPlaybackList_CurrentItemChanged;
            mediaPlayer.PlaybackSession.PlaybackStateChanged += PlaybackSession_PlaybackStateChanged;
            //mediaPlayer.PlaybackSession.PositionChanged += PlaybackSession_PositionChangedAsync;
        }

        private void InitMediaPlayer()
        {
            mediaPlayer = new MediaPlayer
            {
                AudioCategory = MediaPlayerAudioCategory.Media,
            };
            ChangeAudioEndPoint(Settings.Current.OutputDeviceID);
            ChangeVolume(Settings.Current.PlayerVolume);

            _autoDevice = mediaPlayer.AudioDevice;
            var type = mediaPlayer.AudioDeviceType;
            var mgr = mediaPlayer.CommandManager;
            mgr.IsEnabled = true;
            positionUpdateTimer = ThreadPoolTimer.CreatePeriodicTimer(UpdatTimerHandler, TimeSpan.FromMilliseconds(250), UpdateTimerDestoyed);
            if (Settings.Current.AudioGraphEffects.HasFlag(Core.Models.Effects.Equalizer))
                mediaPlayer.AddAudioEffect(typeof(SuperEQ).FullName, false, new PropertySet()
                {
                    ["EqualizerBand"] = new EqualizerBand[]
                    {
                        new EqualizerBand {Bandwidth = 0.8f, Frequency = 30, Gain = Settings.Current.Gain[0]},
                        new EqualizerBand {Bandwidth = 0.8f, Frequency = 75, Gain = Settings.Current.Gain[1]},
                        new EqualizerBand {Bandwidth = 0.8f, Frequency = 150, Gain = Settings.Current.Gain[2]},
                        new EqualizerBand {Bandwidth = 0.8f, Frequency = 30, Gain = Settings.Current.Gain[3]},
                        new EqualizerBand {Bandwidth = 0.8f, Frequency = 600, Gain = Settings.Current.Gain[4]},
                        new EqualizerBand {Bandwidth = 0.8f, Frequency = 1250, Gain = Settings.Current.Gain[5]},
                        new EqualizerBand {Bandwidth = 0.8f, Frequency = 2500, Gain = Settings.Current.Gain[6]},
                        new EqualizerBand {Bandwidth = 0.8f, Frequency = 5000, Gain = Settings.Current.Gain[7]},
                        new EqualizerBand {Bandwidth = 0.8f, Frequency = 10000, Gain = Settings.Current.Gain[8]},
                        new EqualizerBand {Bandwidth = 0.8f, Frequency = 20000, Gain = Settings.Current.Gain[9]},
                    }
                });
        }

        private void UpdateTimerDestoyed(ThreadPoolTimer timer)
        {
            timer.Cancel();
            timer = null;
            positionUpdateTimer?.Cancel();
            positionUpdateTimer = null;
            positionUpdateTimer = ThreadPoolTimer.CreatePeriodicTimer(UpdatTimerHandler, TimeSpan.FromMilliseconds(250), UpdateTimerDestoyed);
        }

        private void UpdatTimerHandler(ThreadPoolTimer timer)
        {
            if (mediaPlayer.PlaybackSession.PlaybackState == MediaPlaybackState.Playing)
            {
                PlaybackSession_PositionChangedAsync(mediaPlayer.PlaybackSession, null);
            }
        }

        private void PlaybackSession_PositionChangedAsync(MediaPlaybackSession sender, object args)
        {
            // TODO: online music can't fire
            lock (lockable)
            {
                var updatedArgs = new PositionUpdatedArgs
                {
                    Current = sender.Position,
                    Total = mediaPlayer.PlaybackSession.NaturalDuration
                };
                PositionUpdated?.Invoke(this, updatedArgs);
                DownloadProgressChanged?.Invoke(this, new DownloadProgressChangedArgs()
                {
                    Progress = sender.DownloadProgress
                });
                try
                {
                    var song = mediaPlaybackList.CurrentItem?.Source.CustomProperties[Consts.SONG] as Song;
                    if (song == null) return;
                    if (song.IsOnline)
                    {
                        // TODO: Online Music Statistic
                        //throw new NotImplementedException("Play Statistic online");
                    }
                    else
                    {
                        var id = song.ID;
                        if (id != default(int) && _songCountID != id && updatedArgs.Current.TotalSeconds / updatedArgs.Total.TotalSeconds > 0.5)
                        {
                            _songCountID = id;
                            var t = ThreadPool.RunAsync(async (x) =>
                            {
                                var opr = SQLOperator.Current();
                                await FileReader.PlayStaticAdd(id, 0, 1);
                            });
                        }
                    }
                }
                catch (Exception)
                {

                }
            }
        }



        private void PlaybackSession_PlaybackStateChanged(MediaPlaybackSession sender, object args)
        {
            // TODO: When error, restore

            switch (mediaPlayer.PlaybackSession.PlaybackState)
            {
                case MediaPlaybackState.None:
                case MediaPlaybackState.Opening:
                case MediaPlaybackState.Buffering:
                    isPlaying = null;
                    break;
                case MediaPlaybackState.Playing:
                    isPlaying = true;
                    break;
                case MediaPlaybackState.Paused:
                    isPlaying = false;
                    break;
                default:
                    break;
            }

            PlaybackStatusChanged?.Invoke(this, new PlaybackStatusChangedArgs()
            {
                PlaybackStatus = mediaPlayer.PlaybackSession.PlaybackState,
                IsLoop = mediaPlaybackList.AutoRepeatEnabled,
                IsShuffle = mediaPlaybackList.ShuffleEnabled
            });
        }

        private void MediaPlaybackList_CurrentItemChanged(MediaPlaybackList sender, CurrentMediaPlaybackItemChangedEventArgs args)
        {
            switch (mediaPlayer.PlaybackSession.PlaybackState)
            {
                case MediaPlaybackState.None:
                case MediaPlaybackState.Opening:
                case MediaPlaybackState.Buffering:
                    isPlaying = null;
                    break;
                case MediaPlaybackState.Playing:
                    isPlaying = true;
                    break;
                case MediaPlaybackState.Paused:
                    isPlaying = false;
                    break;
                default:
                    break;
            }

            var currentSong = mediaPlaybackList.CurrentItem?.Source.CustomProperties[Consts.SONG] as Song;


            ItemsChanged?.Invoke(this, new PlayingItemsChangedArgs
            {
                IsShuffle = mediaPlaybackList.ShuffleEnabled,
                IsLoop = mediaPlaybackList.AutoRepeatEnabled,
                CurrentSong = currentSong,
                CurrentIndex = currentSong == null ? -1 : currentList.FindIndex(a => a == currentSong),
                Items = currentList
            });

            // TODO: restore when error
            if (args.Reason == MediaPlaybackItemChangedReason.Error)
            {
                throw new IOException("Play back error.");
            }
        }

        public async Task NewPlayList(IList<Song> items, int startIndex = 0)
        {
            if (items.IsNullorEmpty())
            {
                throw new ArgumentNullException("Items empty");
            }

            newComing = true;
            _addPlayListTask?.Cancel();

            mediaPlayer.Pause();
            mediaPlaybackList.Items.Clear();

            currentList.Clear();
            currentList.AddRange(items);

            if (startIndex <= 0)
            {
                var item = items[0];
                MediaPlaybackItem mediaPlaybackItem = await GetMediaPlaybackItem(item);

                mediaPlaybackList.Items.Add(mediaPlaybackItem);
                mediaPlaybackList.StartingItem = mediaPlaybackItem;

                mediaPlayer.Source = mediaPlaybackList;

                newComing = false;

                _addPlayListTask = ThreadPool.RunAsync(async (x) =>
                {
                    await AddtoPlayListAsync(items.TakeLast(items.Count - 1));
                });
            }
            else
            {
                if (startIndex >= items.Count)
                {
                    startIndex = items.Count - 1;
                }
                var item = items[startIndex];

                var listBefore = items.Take(startIndex);
                var listAfter = items.TakeLast(items.Count - 1 - startIndex);

                MediaPlaybackItem mediaPlaybackItem = await GetMediaPlaybackItem(item);

                mediaPlaybackList.Items.Add(mediaPlaybackItem);
                mediaPlaybackList.StartingItem = mediaPlaybackItem;

                mediaPlayer.Source = mediaPlaybackList;

                newComing = false;

                _addPlayListTask = ThreadPool.RunAsync(async (x) =>
                {
                    var a = AddtoPlayListFirstAsync(listBefore);
                    var s = AddtoPlayListAsync(listAfter);
                    await Task.WhenAll(new Task[] { a, s });
                });
            }

            PlaybackSession_PlaybackStateChanged(null, null);
        }
        public async Task NewPlayList(IList<StorageFile> items)
        {
            if (items.IsNullorEmpty())
            {
                throw new ArgumentNullException("Items empty");
            }

            newComing = true;
            _addPlayListTask?.Cancel();

            mediaPlayer.Pause();
            mediaPlaybackList.Items.Clear();

            currentList.Clear();

            foreach (var file in items)
            {
                MediaSource mediaSource;

                var item = await FileReader.ReadFileAsync(file);

                mediaSource = MediaSource.CreateFromStorageFile(file);
                mediaSource.CustomProperties[Consts.SONG] = item;

                var mediaPlaybackItem = new MediaPlaybackItem(mediaSource);
                var props = mediaPlaybackItem.GetDisplayProperties();

                await WriteProperties(item, props, item.PicturePath);

                mediaPlaybackItem.ApplyDisplayProperties(props);
                mediaPlaybackList.Items.Add(mediaPlaybackItem);
                currentList.Add(item);
            }
            mediaPlaybackList.StartingItem = mediaPlaybackList.Items.First();
            mediaPlayer.Source = mediaPlaybackList;
        }

        private async Task AddtoPlayListFirstAsync(IEnumerable<Song> items)
        {
            foreach (var item in items.Reverse())
            {
                try
                {
                    MediaPlaybackItem mediaPlaybackItem = await GetMediaPlaybackItem(item);

                    if (newComing)
                        return;

                    mediaPlaybackList.Items.Insert(0, mediaPlaybackItem);
                }
                catch (FileNotFoundException)
                {
                    continue;
                }
            }
        }

        private async Task AddtoPlayListAsync(IEnumerable<Song> items)
        {
            foreach (var item in items)
            {
                try
                {
                    MediaPlaybackItem mediaPlaybackItem = await GetMediaPlaybackItem(item);

                    if (newComing)
                        return;

                    mediaPlaybackList.Items.Add(mediaPlaybackItem);
                }
                catch (FileNotFoundException)
                {
                    continue;
                }
            }
        }

        private async Task<MediaPlaybackItem> GetMediaPlaybackItem(Song item)
        {
            MediaSource mediaSource;
            string builtin;

            if (item.IsOnline)
            {
                mediaSource = MediaSource.CreateFromUri(item.OnlineUri);
                builtin = item.PicturePath;
            }
            else
            {
                try
                {
                    /// **Local files can only create from <see cref="StorageFile"/>**
                    StorageFile file = await StorageFile.GetFileFromPathAsync(item.FilePath);

                    builtin = await Core.Tools.Helper.GetBuiltInArtworkAsync(item.ID.ToString(), item.Title, file);

                    mediaSource = MediaSource.CreateFromStorageFile(file);
                }

                catch (FileNotFoundException)
                {
                    item.IsEmpty = true;
                    throw;
                }
            }

            item.PicturePath = builtin.IsNullorEmpty() ? item.PicturePath : builtin;
            mediaSource.CustomProperties[Consts.SONG] = item;
            var mediaPlaybackItem = new MediaPlaybackItem(mediaSource);
            var props = mediaPlaybackItem.GetDisplayProperties();

            await WriteProperties(item, props, builtin);

            mediaPlaybackItem.ApplyDisplayProperties(props);
            return mediaPlaybackItem;
        }

        private async Task WriteProperties(Song item, MediaItemDisplayProperties props, string pic)
        {
            if (item.IsOnline)
            {
                if (pic == string.Empty)
                {
                    props.Thumbnail = RandomAccessStreamReference.CreateFromUri(new Uri(Consts.BlackPlaceholder));
                }
                else
                {
                    props.Thumbnail = RandomAccessStreamReference.CreateFromUri(new Uri(pic));
                }
            }
            else
            {
                if (pic == string.Empty)
                {
                    props.Thumbnail = RandomAccessStreamReference.CreateFromUri(new Uri(Consts.BlackPlaceholder));
                }
                else
                {
                    props.Thumbnail = RandomAccessStreamReference.CreateFromFile(await StorageFile.GetFileFromPathAsync(pic));
                }
            }

            props.Type = Windows.Media.MediaPlaybackType.Music;
            props.MusicProperties.Title = item.Title.IsNullorEmpty() ? item.FilePath.Split('\\').LastOrDefault() : item.Title;
            props.MusicProperties.AlbumTitle = item.Album.IsNullorEmpty() ? "" : item.Album;
            props.MusicProperties.AlbumArtist = item.AlbumArtists.IsNullorEmpty() ? "" : string.Join(Consts.CommaSeparator, item.AlbumArtists);
            props.MusicProperties.AlbumTrackCount = item.TrackCount;
            props.MusicProperties.Artist = item.Performers.IsNullorEmpty() ? "" : string.Join(Consts.CommaSeparator, item.Performers);
            props.MusicProperties.TrackNumber = item.Track;
            if (!item.Genres.IsNullorEmpty())
            {
                foreach (var g in item.Genres)
                {
                    props.MusicProperties.Genres.Add(g);
                }
            }
        }

        public void PlayPause()
        {
            switch (mediaPlayer.PlaybackSession.PlaybackState)
            {
                case MediaPlaybackState.Playing:
                    Pause();
                    break;
                case MediaPlaybackState.None:
                case MediaPlaybackState.Opening:
                case MediaPlaybackState.Buffering:
                case MediaPlaybackState.Paused:
                    Play();
                    break;
                default:
                    Play();
                    break;
            }
        }

        public void Loop(bool? b)
        {
            mediaPlaybackList.AutoRepeatEnabled = b ?? false;
        }

        public void Shuffle(bool? b)
        {
            lock (lockable)
            {
                if (IsShuffle == b)
                    return;
                var cure = mediaPlaybackList.CurrentItem;
                _savedState = mediaPlayer.PlaybackSession.PlaybackState;
                _savedPosition = mediaPlayer.PlaybackSession.Position;

                mediaPlayer.Pause();
                mediaPlayer.Source = null;

                if (b is bool boo)
                {
                    if (boo)
                    {
                        mediaPlaybackList.ShuffleEnabled = true;
                    }
                    else
                    {
                        mediaPlaybackList.ShuffleEnabled = false;
                    }
                }
                else
                {
                    mediaPlaybackList.ShuffleEnabled = false;
                }

                //var index = mediaPlaybackList.ShuffleEnabled ? mediaPlaybackList.ShuffledItems.ToList().IndexOf(cure) : mediaPlaybackList.Items.IndexOf(cure);
                mediaPlayer.Source = mediaPlaybackList;
                //if (index >= 0)
                //    mediaPlaybackList.MoveTo((uint)index);
                //mediaPlayer.PlaybackSession.Position = _savedPosition;
                if (_savedState == MediaPlaybackState.Playing)
                {
                    mediaPlayer.Play();
                }
            }

        }

        public void Next()
        {
            if (mediaPlaybackList.CurrentItem == null || mediaPlaybackList.Items.Count < 1)
            {
                return;
            }

            if (mediaPlaybackList.ShuffleEnabled)
            {
                uint i = mediaPlaybackList.CurrentItemIndex + 1;
                bool b = false;
                foreach (var item in mediaPlaybackList.ShuffledItems)
                {
                    if (b)
                    {
                        i = (uint)mediaPlaybackList.Items.IndexOf(item);
                        b = false;
                        break;
                    }
                    // in next loop, find the next shuffled item
                    if (item == mediaPlaybackList.CurrentItem)
                    {
                        b = true;
                    }
                }

                // played to the last of ShuffledItems
                if (b)
                {
                    if (mediaPlaybackList.AutoRepeatEnabled)
                    {
                        mediaPlaybackList.MoveTo(0);
                        ChangeVolume(Settings.Current.PlayerVolume);

                    }
                    else
                    {
                        Stop();
                    }
                    return;
                }

                mediaPlaybackList.MoveTo(i);

                ChangeVolume(Settings.Current.PlayerVolume);
                return;
            }



            if (mediaPlaybackList.AutoRepeatEnabled)
            {
                mediaPlaybackList.MoveNext();

                ChangeVolume(Settings.Current.PlayerVolume);
                return;
            }


            if (mediaPlaybackList.CurrentItemIndex == mediaPlaybackList.Items.Count - 1)
            {
                Stop();
                return;
            }

            mediaPlaybackList.MoveNext();
            ChangeVolume(Settings.Current.PlayerVolume);
        }

        public void Stop()
        {
            if (mediaPlaybackList.Items.Count < 1)
            {
                return;
            }
            if (mediaPlayer.PlaybackSession.CanPause)
                mediaPlayer.Pause();
            mediaPlayer.Source = null;
            ItemsChanged?.Invoke(this, new PlayingItemsChangedArgs() { CurrentSong = null, CurrentIndex = -1, Items = null });
        }

        public void Seek(TimeSpan position)
        {
            if (mediaPlayer.PlaybackSession.CanSeek)
            {
                mediaPlayer.PlaybackSession.Position = position;
            }
        }

        public void Previous()
        {
            if (mediaPlaybackList.CurrentItem == null || mediaPlaybackList.Items.Count < 1)
            {
                return;
            }

            if (mediaPlayer.PlaybackSession.Position.TotalSeconds > 3)
            {
                mediaPlayer.PlaybackSession.Position = TimeSpan.Zero;
                return;
            }

            if (mediaPlaybackList.ShuffleEnabled)
            {
                uint i = mediaPlaybackList.CurrentItemIndex - 1;
                MediaPlaybackItem previous = null;

                foreach (var item in mediaPlaybackList.ShuffledItems)
                {
                    // find the current shuffled item and find the previous index
                    if (item == mediaPlaybackList.CurrentItem)
                    {
                        // the first of shuffle list
                        if (previous == null)
                        {
                            if (mediaPlaybackList.AutoRepeatEnabled)
                            {
                                mediaPlaybackList.MoveTo(0);
                                ChangeVolume(Settings.Current.PlayerVolume);

                            }
                            else
                            {
                                Stop();
                            }
                            return;
                        }

                        i = (uint)mediaPlaybackList.Items.IndexOf(previous);
                    }
                    
                    previous = item;
                }

                mediaPlaybackList.MoveTo(i);

                ChangeVolume(Settings.Current.PlayerVolume);
                return;
            }


            if (mediaPlaybackList.AutoRepeatEnabled)
            {
                mediaPlaybackList.MovePrevious();
                ChangeVolume(Settings.Current.PlayerVolume);
                return;
            }
            if (mediaPlaybackList.CurrentItemIndex == 0)
            {
                Stop();
                return;
            }
            mediaPlaybackList.MovePrevious();
            ChangeVolume(Settings.Current.PlayerVolume);
        }

        public async void Play()
        {
            if (mediaPlaybackList == null || mediaPlaybackList.Items.IsNullorEmpty())
            {
                return;
            }

            if (mediaPlayer.Source == null)
            {
                mediaPlayer.Source = mediaPlaybackList;
            }

            int i = 0;
            while (mediaPlaybackList.CurrentItem == null)
            {
                //mediaPlaybackList.MoveNext();
                i++;
                if (i > 10)
                {
                    mediaPlaybackList.StartingItem = mediaPlaybackList.Items.First();
                    break;
                }
                await Task.Delay(500);
            }

            mediaPlayer.Play();
            ChangeVolume(Settings.Current.PlayerVolume);
        }

        public void Pause()
        {
            if (mediaPlayer.PlaybackSession.CanPause)
                mediaPlayer.Pause();
        }


        public void Dispose()
        {
            mediaPlayer.Dispose();
        }

        public void SkiptoIndex(uint index)
        {
            if (index >= 0 && index < mediaPlaybackList.Items.Count)
            {
                mediaPlaybackList.MoveTo(index);
            }
        }

        public async Task ReloadCurrent()
        {
            var state = mediaPlayer.PlaybackSession.PlaybackState;
            var position = mediaPlayer.PlaybackSession.Position;

            var index = mediaPlaybackList.CurrentItemIndex;

            var cure = mediaPlaybackList.CurrentItem;
            mediaPlaybackList.Items.Remove(mediaPlaybackList.CurrentItem);
            cure.Source.Dispose();
            mediaPlayer.Source = null;

            if (index > currentList.Count)
            {

            }
            else
            {
                var item = currentList[(int)index];
                MediaPlaybackItem mediaPlaybackItem = await GetMediaPlaybackItem(item);
                mediaPlaybackList.Items.Insert((int)index, mediaPlaybackItem);
                mediaPlaybackList.StartingItem = mediaPlaybackItem;
            }
            mediaPlayer.Source = mediaPlaybackList;
            mediaPlayer.PlaybackSession.Position = position;
            if (state == MediaPlaybackState.Playing)
            {
                mediaPlayer.Play();
            }
        }

        public void RemoveCurrentItem()
        {
            var state = mediaPlayer.PlaybackSession.PlaybackState;
            mediaPlayer.Pause();
            var cure = mediaPlaybackList.CurrentItem;
            mediaPlaybackList.MoveNext();
            cure.Source.Dispose();
            mediaPlaybackList.Items.Remove(cure);
            cure = null;
            if (state == MediaPlaybackState.Playing)
            {
                mediaPlayer.Play();
            }
        }

#pragma warning disable 1998
        public async Task DetachCurrentItem()
        {
            _savedState = mediaPlayer.PlaybackSession.PlaybackState;
            _savedPosition = mediaPlayer.PlaybackSession.Position;

            _savedIndex = mediaPlaybackList.CurrentItemIndex;

            var cure = mediaPlaybackList.CurrentItem;
            mediaPlaybackList.Items.Remove(mediaPlaybackList.CurrentItem);
            cure.Source.Dispose();
            mediaPlayer.Source = null;
        }

        public async Task ReAttachCurrentItem()
        {
            if (_savedIndex > currentList.Count)
            {

            }
            else
            {
                var item = currentList[(int)_savedIndex];
                MediaPlaybackItem mediaPlaybackItem = await GetMediaPlaybackItem(item);
                mediaPlaybackList.Items.Insert((int)_savedIndex, mediaPlaybackItem);
                mediaPlaybackList.StartingItem = mediaPlaybackItem;
            }
            mediaPlayer.Source = mediaPlaybackList;
            mediaPlayer.PlaybackSession.Position = _savedPosition;
            if (_savedState == MediaPlaybackState.Playing)
            {
                mediaPlayer.Play();
            }
        }

        public async Task AddtoNextPlay(IList<Song> items)
        {
            if (mediaPlaybackList.CurrentItem == null)
            {
                await NewPlayList(items);
                return;
            }

            var curIdx = (int)mediaPlaybackList.CurrentItemIndex;

            // NOTE: add to current song list
            currentList.InsertRange(curIdx + 1, items);

            for (int i = 0; i < items.Count; i++)
            {
                try
                {
                    var item = items[i];

                    MediaPlaybackItem mediaPlaybackItem = await GetMediaPlaybackItem(item);

                    if (newComing)
                        return;


                    mediaPlaybackList.Items.Insert(curIdx + 1 + i, mediaPlaybackItem);

                }
                catch (FileNotFoundException)
                {
                    continue;
                }
            }
        }

        public void ChangeEQ(float[] gain)
        {
            SuperEQ.Current.UpdateEqualizerBand(gain);
        }

        public async void ToggleEffect(Core.Models.Effects audioGraphEffects)
        {
            mediaPlayer.RemoveAllEffects();
            if (audioGraphEffects.HasFlag(Core.Models.Effects.Equalizer))
            {
                mediaPlayer.AddAudioEffect(typeof(SuperEQ).FullName, false, new PropertySet()
                {
                    ["EqualizerBand"] = new EqualizerBand[]
                    {
                        new EqualizerBand {Bandwidth = 0.8f, Frequency = 30, Gain = Settings.Current.Gain[0]},
                        new EqualizerBand {Bandwidth = 0.8f, Frequency = 75, Gain = Settings.Current.Gain[1]},
                        new EqualizerBand {Bandwidth = 0.8f, Frequency = 150, Gain = Settings.Current.Gain[2]},
                        new EqualizerBand {Bandwidth = 0.8f, Frequency = 30, Gain = Settings.Current.Gain[3]},
                        new EqualizerBand {Bandwidth = 0.8f, Frequency = 600, Gain = Settings.Current.Gain[4]},
                        new EqualizerBand {Bandwidth = 0.8f, Frequency = 1250, Gain = Settings.Current.Gain[5]},
                        new EqualizerBand {Bandwidth = 0.8f, Frequency = 2500, Gain = Settings.Current.Gain[6]},
                        new EqualizerBand {Bandwidth = 0.8f, Frequency = 5000, Gain = Settings.Current.Gain[7]},
                        new EqualizerBand {Bandwidth = 0.8f, Frequency = 10000, Gain = Settings.Current.Gain[8]},
                        new EqualizerBand {Bandwidth = 0.8f, Frequency = 20000, Gain = Settings.Current.Gain[9]},
                    }
                });
            }
            if (mediaPlaybackList.CurrentItem == null)
            {
                return;
            }
            await DetachCurrentItem();
            await ReAttachCurrentItem();
        }

        public void Backward(TimeSpan timeSpan)
        {
            var p = mediaPlayer.PlaybackSession.Position - timeSpan;
            if (p.TotalMilliseconds < 0)
            {
                mediaPlayer.PlaybackSession.Position = TimeSpan.Zero;
                Previous();
            }
            else
            {
                mediaPlayer.PlaybackSession.Position -= timeSpan;
            }
        }

        public void Forward(TimeSpan timeSpan)
        {
            var p = mediaPlayer.PlaybackSession.Position + timeSpan;
            if (p > mediaPlayer.PlaybackSession.NaturalDuration)
            {
                Next();
            }
            else
            {
                mediaPlayer.PlaybackSession.Position += timeSpan;
            }
        }
    }
}
