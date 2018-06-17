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
    public sealed class NeonPlayer : IDisposable, IPlayer
    {
        private MediaPlaybackList mediaPlaybackList;

        private DeviceInformation _autoDevice;

        private readonly object lockable = new object();
        private int _songCountID;
        private IAsyncAction _addPlayListTask;
        private List<Song> currentList;
        private bool newComing;
        private ThreadPoolTimer positionUpdateTimer;

        public bool? IsPlaying { get; private set; }
        private MediaPlaybackState _savedState;
        private TimeSpan _savedPosition;
        private uint _savedIndex;

        private bool IsLoop { get; set; }
        private bool IsOneLoop { get; set; }

        public MediaPlayer MediaPlayer { get; private set; }

        public double PlaybackRate
        {
            get => MediaPlayer.PlaybackSession.PlaybackRate;
            set => MediaPlayer.PlaybackSession.PlaybackRate = value;
        }


        private bool isShuffle;
        public bool? IsShuffle
        {
            get => isShuffle;
            set => isShuffle = value == true;
        }

        private List<Song> currentListBackup;
        private int currentIndex;

        public double Volume => MediaPlayer.Volume;

        public event EventHandler<PositionUpdatedArgs> PositionUpdated;

        public async void ChangeAudioEndPoint(string outputDeviceID)
        {
            if (outputDeviceID.IsNullorEmpty() || outputDeviceID == MediaPlayer.AudioDevice?.Id)
            {
                return;
            }
            var outputDevice = await DeviceInformation.CreateFromIdAsync(outputDeviceID);
            MediaPlayer.AudioDevice = outputDevice;
            Pause();
        }

        public void ChangeVolume(double value)
        {
            MediaPlayer.Volume = value / 100d;
        }

        public event EventHandler<DownloadProgressChangedArgs> DownloadProgressChanged;
        public event EventHandler<PlaybackStatusChangedArgs> PlaybackStatusChanged;
        public event EventHandler<PlayingItemsChangedArgs> ItemsChanged;

        public NeonPlayer()
        {
            currentList = new List<Song>();

            InitMediaPlayer();

            mediaPlaybackList = new MediaPlaybackList();
            mediaPlaybackList.CurrentItemChanged += MediaPlaybackList_CurrentItemChanged;
            MediaPlayer.PlaybackSession.PlaybackStateChanged += PlaybackSession_PlaybackStateChanged;
            //mediaPlayer.PlaybackSession.PositionChanged += PlaybackSession_PositionChangedAsync;
        }

        private void InitMediaPlayer()
        {
            MediaPlayer = new MediaPlayer
            {
                AudioCategory = MediaPlayerAudioCategory.Media,
            };
            ChangeAudioEndPoint(Settings.Current.OutputDeviceID);
            ChangeVolume(Settings.Current.PlayerVolume);

            _autoDevice = MediaPlayer.AudioDevice;
            var type = MediaPlayer.AudioDeviceType;
            var mgr = MediaPlayer.CommandManager;
            mgr.IsEnabled = true;
            positionUpdateTimer = ThreadPoolTimer.CreatePeriodicTimer(UpdatTimerHandler, TimeSpan.FromMilliseconds(250), UpdateTimerDestoyed);


            if (Settings.Current.AudioGraphEffects.HasFlag(Core.Models.Effects.ChannelShift))
                MediaPlayer.AddAudioEffect(typeof(ChannelShift).FullName, false, new PropertySet()
                {
                    ["Shift"] = Settings.Current.ChannelShift,
                    ["Mono"] = Settings.Current.StereoToMono
                });
            if (Settings.Current.AudioGraphEffects.HasFlag(Core.Models.Effects.Equalizer))
                MediaPlayer.AddAudioEffect(typeof(SuperEQ).FullName, false, new PropertySet()
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
            if (Settings.Current.AudioGraphEffects.HasFlag(Core.Models.Effects.Limiter))
                MediaPlayer.AddAudioEffect(typeof(Threshold).FullName, false, new PropertySet()
                {
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
            if (MediaPlayer.PlaybackSession.PlaybackState == MediaPlaybackState.Playing)
            {
                PlaybackSession_PositionChangedAsync(MediaPlayer.PlaybackSession, null);
            }
        }

        private void PlaybackSession_PositionChangedAsync(MediaPlaybackSession sender, object args)
        {
            lock (lockable)
            {
                var updatedArgs = new PositionUpdatedArgs
                {
                    Current = sender.Position,
                    Total = MediaPlayer.PlaybackSession.NaturalDuration
                };
                PositionUpdated?.Invoke(this, updatedArgs);
                DownloadProgressChanged?.Invoke(this, new DownloadProgressChangedArgs()
                {
                    Progress = sender.DownloadProgress
                });
                try
                {
                    if (!(mediaPlaybackList.CurrentItem?.Source.CustomProperties[Consts.SONG] is Song song)) return;
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
                                await FileReader.PlayStaticAddAsync(id, 0, 1);
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

            switch (MediaPlayer.PlaybackSession.PlaybackState)
            {
                case MediaPlaybackState.None:
                case MediaPlaybackState.Opening:
                case MediaPlaybackState.Buffering:
                    IsPlaying = null;
                    break;
                case MediaPlaybackState.Playing:
                    IsPlaying = true;
                    break;
                case MediaPlaybackState.Paused:
                    IsPlaying = false;
                    break;
                default:
                    break;
            }

            PlaybackStatusChanged?.Invoke(this, new PlaybackStatusChangedArgs()
            {
                PlaybackStatus = MediaPlayer.PlaybackSession.PlaybackState,
                IsLoop = IsLoop,
                IsOneLoop = IsOneLoop,
                IsShuffle = isShuffle
            });
        }

        private async void MediaPlaybackList_CurrentItemChanged(MediaPlaybackList sender, CurrentMediaPlaybackItemChangedEventArgs args)
        {
            switch (MediaPlayer.PlaybackSession.PlaybackState)
            {
                case MediaPlaybackState.None:
                case MediaPlaybackState.Opening:
                case MediaPlaybackState.Buffering:
                    IsPlaying = null;
                    break;
                case MediaPlaybackState.Playing:
                    IsPlaying = true;
                    break;
                case MediaPlaybackState.Paused:
                    IsPlaying = false;
                    break;
                default:
                    break;
            }

            var currentSong = mediaPlaybackList.CurrentItem?.Source.CustomProperties[Consts.SONG] as Song;

            // TODO: update mediaPlaybackList keep 3 items

            if (currentSong != null)
            {
                currentIndex = currentList.IndexOf(currentSong);
                var localIndex = mediaPlaybackList.CurrentItemIndex;

                switch (localIndex)
                {
                    case 0:
                        mediaPlaybackList.Items.RemoveAt(2);
                        // head
                        var headIndex = currentIndex - 1;
                        if (headIndex < 0)
                        {
                            headIndex = currentList.Count - 1;
                        }
                        var head = await GetMediaPlaybackItemAsync(currentList[headIndex]);
                        mediaPlaybackList.Items.Insert(0, head);
                        break;
                    case 1:
                        break;
                    case 2:
                        mediaPlaybackList.Items.RemoveAt(0);
                        // tail
                        var tailIndex = currentIndex + 1;
                        if (tailIndex >= currentList.Count)
                        {
                            tailIndex = 0;
                        }
                        var tail = await GetMediaPlaybackItemAsync(currentList[tailIndex]);
                        mediaPlaybackList.Items.Add(tail);
                        break;
                    default:
                        break;
                }
            }

            ItemsChanged?.Invoke(this, new PlayingItemsChangedArgs
            {
                IsShuffle = isShuffle,
                IsLoop = IsLoop,
                IsOneLoop = IsOneLoop,
                CurrentSong = currentSong,
                CurrentIndex = currentSong == null ? -1 : currentList.FindIndex(a => a == currentSong),
                Items = currentList,
                Thumnail = mediaPlaybackList.CurrentItem?.GetDisplayProperties().Thumbnail
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
                return;
            }

            newComing = true;
            _addPlayListTask?.Cancel();

            MediaPlayer.Pause();
            mediaPlaybackList.Items.Clear();

            if (isShuffle)
            {
                currentListBackup = items.ToList();
                var item = items[startIndex];
                items.Shuffle();
                var i = items.IndexOf(item);
                var p = items[startIndex];
                items[startIndex] = items[i];
                items[i] = p;
            }

            currentList.Clear();
            currentList.AddRange(items);


            if (startIndex >= currentList.Count)
            {
                startIndex = currentList.Count - 1;
            }

            if (startIndex <= 0)
            {
                startIndex = 0;
            }

            // head
            var headIndex = startIndex - 1;
            if (headIndex < 0)
            {
                headIndex = currentList.Count - 1;
            }
            var head = await GetMediaPlaybackItemAsync(currentList[headIndex]);
            mediaPlaybackList.Items.Add(head);
            // current
            var mid = await GetMediaPlaybackItemAsync(currentList[startIndex]);
            mediaPlaybackList.Items.Add(mid);
            // tail
            var tailIndex = startIndex + 1;
            if (tailIndex >= items.Count)
            {
                tailIndex = 0;
            }
            var tail = await GetMediaPlaybackItemAsync(currentList[tailIndex]);
            mediaPlaybackList.Items.Add(tail);

            mediaPlaybackList.StartingItem = mid;

            MediaPlayer.Source = mediaPlaybackList;

            currentIndex = startIndex;

            PlaybackSession_PlaybackStateChanged(null, null);
        }


        public async Task NewPlayList(IList<StorageFile> items, int startIndex = 0)
        {
            if (items.IsNullorEmpty())
            {
                return;
            }

            _addPlayListTask?.Cancel();

            MediaPlayer.Pause();
            mediaPlaybackList.Items.Clear();

            currentList.Clear();

            if (isShuffle)
            {
                //currentListBackup = items.ToList();
                var item = items[startIndex];
                items.Shuffle();
                var i = items.IndexOf(item);
                var p = items[startIndex];
                items[startIndex] = items[i];
                items[i] = p;
            }

            foreach (var file in items)
            {
                var item = await FileReader.ReadFileAsync(file);
                item.File = file;
                currentList.Add(item);
            }

            if (startIndex > mediaPlaybackList.Items.Count - 1)
            {
                startIndex = mediaPlaybackList.Items.Count - 1;
            }
            if (startIndex < 0)
                startIndex = 0;

            // head
            var headIndex = startIndex - 1;
            if (headIndex < 0)
            {
                headIndex = currentList.Count - 1;
            }
            var head = await GetMediaPlaybackItemAsync(currentList[headIndex]);
            mediaPlaybackList.Items.Add(head);
            // current
            var mid = await GetMediaPlaybackItemAsync(currentList[startIndex]);
            mediaPlaybackList.Items.Add(mid);
            // tail
            var tailIndex = startIndex + 1;
            if (tailIndex >= items.Count)
            {
                tailIndex = 0;
            }
            var tail = await GetMediaPlaybackItemAsync(currentList[tailIndex]);
            mediaPlaybackList.Items.Add(tail);

            mediaPlaybackList.StartingItem = mid;

            mediaPlaybackList.StartingItem = mediaPlaybackList.Items[startIndex];
            MediaPlayer.Source = mediaPlaybackList;

            PlaybackSession_PlaybackStateChanged(null, null);
        }

        private async Task<MediaPlaybackItem> GetMediaPlaybackItemAsync(Song item)
        {
            if (item.IsOnline)
            {
                var mediaSource = MediaSource.CreateFromUri(item.OnlineUri);
                mediaSource.CustomProperties[Consts.SONG] = item;
                var mediaPlaybackItem = new MediaPlaybackItem(mediaSource);
                var props = mediaPlaybackItem.GetDisplayProperties();

                await WritePropertiesAsync(item, props, item.PicturePath);

                mediaPlaybackItem.ApplyDisplayProperties(props);
                return mediaPlaybackItem;
            }
            else
            {
                try
                {
                    /// **Local files can only create from <see cref="StorageFile"/>**

                    StorageFile file;

                    if (item.File != null)
                    {
                        file = item.File;
                    }
                    else
                    {
                        file = await StorageFile.GetFileFromPathAsync(item.FilePath);
                    }

                    var img = await Core.Tools.Helper.UpdateSongAsync(item, file);

                    var mediaSource = MediaSource.CreateFromStorageFile(file);

                    mediaSource.CustomProperties[Consts.SONG] = item;
                    var mediaPlaybackItem = new MediaPlaybackItem(mediaSource);
                    var props = mediaPlaybackItem.GetDisplayProperties();

                    if (img == null)
                    {
                        // Use black placeholder
                        await WritePropertiesAsync(item, props, item.PicturePath);
                    }
                    else
                    {
                        // Use image stream
                        WriteProperties(item, props, img);
                    }
                    mediaPlaybackItem.ApplyDisplayProperties(props);
                    return mediaPlaybackItem;
                }
                catch (FileNotFoundException)
                {
                    item.IsEmpty = true;
                    throw;
                }
            }
        }

        private void WriteProperties(Song item, MediaItemDisplayProperties props, RandomAccessStreamReference img)
        {
            // When to Dispose img?
            props.Thumbnail = img;

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

        private async Task WritePropertiesAsync(Song item, MediaItemDisplayProperties props, string pic)
        {
            if (item.IsOnline)
            {
                if (pic.IsNullorEmpty())
                {
                    props.Thumbnail = null;
                }
                else
                {
                    props.Thumbnail = RandomAccessStreamReference.CreateFromUri(new Uri(pic));
                }
            }
            else
            {
                if (pic.IsNullorEmpty())
                {
                    props.Thumbnail = null;
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
            switch (MediaPlayer.PlaybackSession.PlaybackState)
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
            IsLoop = b ?? false;
        }

        public void Shuffle(bool? b)
        {
            lock (lockable)
            {
                if (IsShuffle == b)
                    return;

                var cure = mediaPlaybackList.CurrentItem.Source.CustomProperties[Consts.SONG] as Song;
                _savedState = MediaPlayer.PlaybackSession.PlaybackState;
                _savedPosition = MediaPlayer.PlaybackSession.Position;

                MediaPlayer.Pause();
                MediaPlayer.Source = null;

                if (b is bool boo && boo)
                {
                    if (boo)
                    {
                        IsShuffle = true;
                        if (currentList.Count > 0)
                        {
                            currentListBackup = currentList.ToList();
                            var l = currentList.ToList();
                            l.Shuffle();
                            var index = l.IndexOf(cure);
                            var p = l[0];
                            l[0] = l[index];
                            l[index] = p;
                            isShuffle = false;
#pragma warning disable CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
                            NewPlayList(l);
#pragma warning restore CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
                            isShuffle = true;
                        }

                    }
                }
                else
                {
                    IsShuffle = false;
                    if (currentListBackup != null)
                    {
                        var index = currentListBackup.IndexOf(cure);
#pragma warning disable CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
                        NewPlayList(currentListBackup.ToList(), index);
#pragma warning restore CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
                    }
                    else if (currentList.Count > 0)
                    {
                        var index = currentList.IndexOf(cure);
#pragma warning disable CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
                        NewPlayList(currentList.ToList(), index);
#pragma warning restore CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
                    }
                }

                if (_savedState == MediaPlaybackState.Playing)
                {
                    MediaPlayer.Play();
                }
                MediaPlayer.PlaybackSession.Position = _savedPosition;
            }

        }

        public void Next()
        {
            if (mediaPlaybackList.CurrentItem == null || mediaPlaybackList.Items.Count < 1)
            {
                return;
            }

            if (currentIndex + 1 == currentList.Count)
            {
                if (IsLoop)
                {
                    mediaPlaybackList.MoveNext();
                    ChangeVolume(Settings.Current.PlayerVolume);
                    return;
                }
                else
                {
                    currentIndex = 0;
                    Stop();
                    return;
                }
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

            mediaPlaybackList.CurrentItemChanged -= MediaPlaybackList_CurrentItemChanged;
            MediaPlayer.PlaybackSession.PlaybackStateChanged -= PlaybackSession_PlaybackStateChanged;

            if (MediaPlayer.PlaybackSession.CanPause)
                MediaPlayer.Pause();
            MediaPlayer.Source = null;
            ItemsChanged?.Invoke(this, new PlayingItemsChangedArgs() { CurrentSong = null, CurrentIndex = -1, Items = null });



            mediaPlaybackList.CurrentItemChanged += MediaPlaybackList_CurrentItemChanged;
            MediaPlayer.PlaybackSession.PlaybackStateChanged += PlaybackSession_PlaybackStateChanged;
        }

        public void Seek(TimeSpan position)
        {
            if (MediaPlayer.PlaybackSession.CanSeek)
            {
                MediaPlayer.PlaybackSession.Position = position;
            }
        }

        public void Previous()
        {
            if (mediaPlaybackList.CurrentItem == null || mediaPlaybackList.Items.Count < 1)
            {
                return;
            }

            if (MediaPlayer.PlaybackSession.Position.TotalSeconds > 3)
            {
                MediaPlayer.PlaybackSession.Position = TimeSpan.Zero;
                return;
            }

            if (currentIndex - 1 < 0)
            {
                if (IsLoop)
                {
                    mediaPlaybackList.MovePrevious();
                    ChangeVolume(Settings.Current.PlayerVolume);
                    return;
                }
                else
                {
                    currentIndex = 0;
                    Stop();
                    return;
                }
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

            if (MediaPlayer.Source == null)
            {
                if (currentList.Count < 1)
                {
                    return;
                }

                currentIndex = 0;
                // stopped, re-add starting item
                mediaPlaybackList.Items.Clear();

                // head
                var headIndex = currentList.Count - 1;
                var head = await GetMediaPlaybackItemAsync(currentList[headIndex]);
                mediaPlaybackList.Items.Add(head);

                // current
                var mid = await GetMediaPlaybackItemAsync(currentList[0]);
                mediaPlaybackList.Items.Add(mid);

                // tail
                var tailIndex = currentIndex + 1;
                if (tailIndex >= currentList.Count)
                {
                    tailIndex = 0;
                }
                var tail = await GetMediaPlaybackItemAsync(currentList[tailIndex]);
                mediaPlaybackList.Items.Add(tail);

                mediaPlaybackList.StartingItem = mid;

                MediaPlayer.Source = mediaPlaybackList;
            }

            int i = 0;

            // wait for mediaPlaybackList to load
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

            MediaPlayer.Play();
            ChangeVolume(Settings.Current.PlayerVolume);
        }

        public void Pause()
        {
            if (MediaPlayer.PlaybackSession.CanPause)
                MediaPlayer.Pause();
        }


        public void Dispose()
        {
            MediaPlayer.Dispose();
        }


        // TODO
        public async void SkiptoIndex(int index)
        {
            if (index == currentIndex)
            {
                return;
            }

            if (index == currentIndex + 1)
            {
                Next();
                return;
            }
            else if (index == currentIndex - 1)
            {
                Previous();
                return;
            }


            mediaPlaybackList.Items.Clear();

            if (index >= currentList.Count)
            {
                index = (currentList.Count - 1);
            }

            if (index <= 0)
            {
                index = 0;
            }

            // head
            var headIndex = index - 1;
            if (headIndex < 0)
            {
                headIndex = currentList.Count - 1;
            }
            var head = await GetMediaPlaybackItemAsync(currentList[headIndex]);
            mediaPlaybackList.Items.Add(head);
            // current
            var mid = await GetMediaPlaybackItemAsync(currentList[index]);
            mediaPlaybackList.Items.Add(mid);
            // tail
            var tailIndex = index + 1;
            if (tailIndex >= currentList.Count)
            {
                tailIndex = 0;
            }
            var tail = await GetMediaPlaybackItemAsync(currentList[tailIndex]);
            mediaPlaybackList.Items.Add(tail);

            mediaPlaybackList.StartingItem = mid;
        }

        // TODO
        public async Task ReloadCurrent()
        {
            var state = MediaPlayer.PlaybackSession.PlaybackState;
            var position = MediaPlayer.PlaybackSession.Position;

            var index = mediaPlaybackList.CurrentItemIndex;

            var cure = mediaPlaybackList.CurrentItem;
            mediaPlaybackList.Items.Remove(mediaPlaybackList.CurrentItem);
            cure.Source.Dispose();
            MediaPlayer.Source = null;

            if (index > currentList.Count)
            {

            }
            else
            {
                var item = currentList[(int)index];
                var mediaPlaybackItem = await GetMediaPlaybackItemAsync(item);
                mediaPlaybackList.Items.Insert((int)index, mediaPlaybackItem);
                mediaPlaybackList.StartingItem = mediaPlaybackItem;
            }
            MediaPlayer.Source = mediaPlaybackList;
            MediaPlayer.PlaybackSession.Position = position;
            if (state == MediaPlaybackState.Playing)
            {
                MediaPlayer.Play();
            }
        }

        // TODO
        public void RemoveCurrentItem()
        {
            var state = MediaPlayer.PlaybackSession.PlaybackState;
            MediaPlayer.Pause();
            var cure = mediaPlaybackList.CurrentItem;
            mediaPlaybackList.MoveNext();
            cure.Source.Dispose();
            mediaPlaybackList.Items.Remove(cure);
            cure = null;
            if (state == MediaPlaybackState.Playing)
            {
                MediaPlayer.Play();
            }
        }

#pragma warning disable 1998
        public async Task DetachCurrentItem()
        {
            _savedState = MediaPlayer.PlaybackSession.PlaybackState;
            _savedPosition = MediaPlayer.PlaybackSession.Position;

            _savedIndex = mediaPlaybackList.CurrentItemIndex;

            var cure = mediaPlaybackList.CurrentItem;
            if (cure == null)
            {

            }
            else
            {
                mediaPlaybackList.Items.Remove(mediaPlaybackList.CurrentItem);
                cure.Source.Dispose();
            }
            MediaPlayer.Source = null;
        }

        public async Task ReAttachCurrentItem()
        {
            if (_savedIndex > currentList.Count)
            {

            }
            else
            {
                var item = currentList[(int)_savedIndex];
                var mediaPlaybackItem = await GetMediaPlaybackItemAsync(item);
                mediaPlaybackList.Items.Insert((int)_savedIndex, mediaPlaybackItem);
                mediaPlaybackList.StartingItem = mediaPlaybackItem;
            }
            MediaPlayer.Source = mediaPlaybackList;
            MediaPlayer.PlaybackSession.Position = _savedPosition;
            if (_savedState == MediaPlaybackState.Playing)
            {
                MediaPlayer.Play();
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


            ItemsChanged?.Invoke(this, new PlayingItemsChangedArgs()
            {
                IsShuffle = isShuffle,
                IsLoop = mediaPlaybackList.AutoRepeatEnabled,
                CurrentSong = currentList[curIdx],
                CurrentIndex = curIdx,
                Items = currentList
            });
        }

        public void ChangeEQ(float[] gain)
        {
            SuperEQ.Current.UpdateEqualizerBand(gain);
        }

        public async void ToggleEffect(Core.Models.Effects audioGraphEffects)
        {
            MediaPlayer.RemoveAllEffects();

            if (Settings.Current.AudioGraphEffects.HasFlag(Core.Models.Effects.ChannelShift))
                MediaPlayer.AddAudioEffect(typeof(ChannelShift).FullName, false, new PropertySet()
                {
                    ["Shift"] = Settings.Current.ChannelShift,
                    ["Mono"] = Settings.Current.StereoToMono
                });
            if (audioGraphEffects.HasFlag(Core.Models.Effects.Equalizer))
            {
                MediaPlayer.AddAudioEffect(typeof(SuperEQ).FullName, false, new PropertySet()
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
            if (Settings.Current.AudioGraphEffects.HasFlag(Core.Models.Effects.Limiter))
                MediaPlayer.AddAudioEffect(typeof(Threshold).FullName, false, new PropertySet()
                {
                });


            if (audioGraphEffects.HasFlag(Core.Models.Effects.Limiter))
            {
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
            var p = MediaPlayer.PlaybackSession.Position - timeSpan;
            if (p.TotalMilliseconds < 0)
            {
                MediaPlayer.PlaybackSession.Position = TimeSpan.Zero;
                Previous();
            }
            else
            {
                MediaPlayer.PlaybackSession.Position -= timeSpan;
            }
        }

        public void Forward(TimeSpan timeSpan)
        {
            var p = MediaPlayer.PlaybackSession.Position + timeSpan;
            if (p > MediaPlayer.PlaybackSession.NaturalDuration)
            {
                Next();
            }
            else
            {
                MediaPlayer.PlaybackSession.Position += timeSpan;
            }
        }

        public void RefreshNowPlayingInfo()
        {
            PlaybackStatusChanged?.Invoke(this, new PlaybackStatusChangedArgs()
            {
                PlaybackStatus = MediaPlayer.PlaybackSession.PlaybackState,
                IsLoop = mediaPlaybackList.AutoRepeatEnabled,
                IsShuffle = isShuffle
            });
            if (mediaPlaybackList.CurrentItem != null)
            {
                var currentSong = mediaPlaybackList.CurrentItem.Source.CustomProperties[Consts.SONG] as Song;
                ItemsChanged?.Invoke(this, new PlayingItemsChangedArgs
                {
                    IsShuffle = isShuffle,
                    IsLoop = mediaPlaybackList.AutoRepeatEnabled,
                    CurrentSong = currentSong,
                    CurrentIndex = currentSong == null ? -1 : currentList.FindIndex(a => a == currentSong),
                    Items = currentList,
                    Thumnail = mediaPlaybackList.CurrentItem?.GetDisplayProperties().Thumbnail
                });
            }
            else
            {
                ItemsChanged?.Invoke(this, new PlayingItemsChangedArgs() { CurrentSong = null, CurrentIndex = -1, Items = null });
            }
        }
    }
}
