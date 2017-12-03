using Aurora.Music.Core;
using Aurora.Music.Core.Models;
using Aurora.Music.Core.Storage;
using Aurora.Shared.Extensions;
using Aurora.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
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


        static Player current;
        public static IPlayer Current
        {
            get
            {
                if (current != null && current.mediaPlayer != null)
                {
                    return current;
                }
                else
                {
                    var p = new Player();
                    current = p;
                    return p;
                }
            }
        }

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
            PlayWithRestart();
        }

        public void ChangeVolume(double value)
        {
            mediaPlayer.Volume = value / 100d;
        }

        public event EventHandler<StatusChangedArgs> StatusChanged;
        public event EventHandler<DownloadProgressChangedArgs> DownloadProgressChanged;

        public Player()
        {
            mediaPlayer = new MediaPlayer
            {
                AudioCategory = MediaPlayerAudioCategory.Media,
            };

            currentList = new List<Song>();

            var settings = Settings.Load();
            ChangeAudioEndPoint(settings.OutputDeviceID);
            ChangeVolume(settings.PlayerVolume);

            _autoDevice = mediaPlayer.AudioDevice;
            var type = mediaPlayer.AudioDeviceType;
            var mgr = mediaPlayer.CommandManager;
            mgr.IsEnabled = true;
            mediaPlaybackList = new MediaPlaybackList();
            mediaPlaybackList.CurrentItemChanged += MediaPlaybackList_CurrentItemChanged;
            mediaPlayer.PlaybackSession.PlaybackStateChanged += PlaybackSession_PlaybackStateChanged;
            //mediaPlayer.PlaybackSession.PositionChanged += PlaybackSession_PositionChangedAsync;
            positionUpdateTimer = ThreadPoolTimer.CreatePeriodicTimer(UpdatTimerHandler, TimeSpan.FromMilliseconds(250), UpdateTimerDestoyed);
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
            StatusChanged?.Invoke(this, new StatusChangedArgs
            {
                IsShuffle = mediaPlaybackList.ShuffleEnabled,
                IsLoop = mediaPlaybackList.AutoRepeatEnabled,
                CurrentSong = mediaPlaybackList.CurrentItem?.Source.CustomProperties[Consts.SONG] as Song,
                CurrentIndex = mediaPlaybackList.CurrentItem == null ? -1 : currentList.IndexOf(mediaPlaybackList.CurrentItem?.Source.CustomProperties[Consts.SONG] as Song),
                Items = currentList
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

            StatusChanged?.Invoke(this, new StatusChangedArgs
            {
                IsShuffle = mediaPlaybackList.ShuffleEnabled,
                IsLoop = mediaPlaybackList.AutoRepeatEnabled,
                CurrentSong = mediaPlaybackList.CurrentItem?.Source.CustomProperties[Consts.SONG] as Song,
                CurrentIndex = mediaPlaybackList.CurrentItem == null ? -1 : currentList.IndexOf(mediaPlaybackList.CurrentItem?.Source.CustomProperties[Consts.SONG] as Song),
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
                MediaSource mediaSource;
                string builtin;

                if (item.IsOnline)
                {
                    mediaSource = MediaSource.CreateFromUri(item.OnlineUri);
                    builtin = item.PicturePath;
                }
                else
                {
                    /// **Local files can only create from <see cref="StorageFile"/>**

                    try
                    {
                        StorageFile file = await StorageFile.GetFileFromPathAsync(item.FilePath);

                        builtin = await GetBuiltInArtworkAsync(file.Name, item.FilePath);

                        mediaSource = MediaSource.CreateFromStorageFile(file);
                    }
                    catch (FileNotFoundException)
                    {
                        item.IsEmpty = true;
                        throw;
                    }
                }

                mediaSource.CustomProperties[Consts.ID] = item.ID;
                mediaSource.CustomProperties[Consts.Duration] = mediaSource.Duration ?? default(TimeSpan);
                mediaSource.CustomProperties[Consts.Artwork] = builtin.IsNullorEmpty() ? null : new Uri(builtin);
                item.PicturePath = builtin.IsNullorEmpty() ? item.PicturePath : builtin;
                mediaSource.CustomProperties[Consts.SONG] = item;

                var mediaPlaybackItem = new MediaPlaybackItem(mediaSource);
                var props = mediaPlaybackItem.GetDisplayProperties();

                await WriteProperties(item, props, builtin);

                mediaPlaybackItem.ApplyDisplayProperties(props);
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

                MediaSource mediaSource;
                string builtin;

                if (item.IsOnline)
                {
                    mediaSource = MediaSource.CreateFromUri(item.OnlineUri);
                    builtin = item.PicturePath;
                }
                else
                {
                    /// **Local files can only create from <see cref="StorageFile"/>**
                    StorageFile file = await StorageFile.GetFileFromPathAsync(item.FilePath);

                    builtin = await GetBuiltInArtworkAsync(file.Name, item.FilePath);

                    mediaSource = MediaSource.CreateFromStorageFile(file);
                }

                mediaSource.CustomProperties[Consts.ID] = item.ID;
                mediaSource.CustomProperties[Consts.Duration] = mediaSource.Duration ?? default(TimeSpan);
                mediaSource.CustomProperties[Consts.Artwork] = builtin.IsNullorEmpty() ? null : new Uri(builtin);
                item.PicturePath = builtin.IsNullorEmpty() ? item.PicturePath : builtin;
                mediaSource.CustomProperties[Consts.SONG] = item;

                var mediaPlaybackItem = new MediaPlaybackItem(mediaSource);
                var props = mediaPlaybackItem.GetDisplayProperties();

                await WriteProperties(item, props, builtin);

                mediaPlaybackItem.ApplyDisplayProperties(props);
                mediaPlaybackList.Items.Add(mediaPlaybackItem);
                mediaPlaybackList.StartingItem = mediaPlaybackItem;

                mediaPlayer.Source = mediaPlaybackList;

                newComing = false;

                _addPlayListTask = ThreadPool.RunAsync(async (x) =>
                {
                    await AddtoPlayListFirstAsync(listBefore);
                    await AddtoPlayListAsync(listAfter);
                });
            }

            PlaybackSession_PlaybackStateChanged(null, null);
            //StatusChanged?.Invoke(this, null);

            PlayWithRestart();
        }

        private async Task AddtoPlayListFirstAsync(IEnumerable<Song> items)
        {
            foreach (var item in items.Reverse())
            {
                try
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

                            builtin = await GetBuiltInArtworkAsync(file.Name, item.FilePath);

                            mediaSource = MediaSource.CreateFromStorageFile(file);
                        }

                        catch (FileNotFoundException)
                        {
                            item.IsEmpty = true;
                            throw;
                        }
                    }

                    mediaSource.CustomProperties[Consts.ID] = item.ID;
                    mediaSource.CustomProperties[Consts.Duration] = mediaSource.Duration ?? default(TimeSpan);
                    mediaSource.CustomProperties[Consts.Artwork] = builtin.IsNullorEmpty() ? null : new Uri(builtin);
                    item.PicturePath = builtin.IsNullorEmpty() ? item.PicturePath : builtin;
                    mediaSource.CustomProperties[Consts.SONG] = item;
                    var mediaPlaybackItem = new MediaPlaybackItem(mediaSource);
                    var props = mediaPlaybackItem.GetDisplayProperties();

                    await WriteProperties(item, props, builtin);

                    mediaPlaybackItem.ApplyDisplayProperties(props);

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

                            builtin = await GetBuiltInArtworkAsync(file.Name, item.FilePath);

                            mediaSource = MediaSource.CreateFromStorageFile(file);
                        }

                        catch (FileNotFoundException)
                        {
                            item.IsEmpty = true;
                            throw;
                        }
                    }

                    mediaSource.CustomProperties[Consts.ID] = item.ID;
                    mediaSource.CustomProperties[Consts.Duration] = mediaSource.Duration ?? default(TimeSpan);
                    mediaSource.CustomProperties[Consts.Artwork] = builtin.IsNullorEmpty() ? null : new Uri(builtin);
                    item.PicturePath = builtin.IsNullorEmpty() ? item.PicturePath : builtin;
                    mediaSource.CustomProperties[Consts.SONG] = item;
                    var mediaPlaybackItem = new MediaPlaybackItem(mediaSource);
                    var props = mediaPlaybackItem.GetDisplayProperties();

                    await WriteProperties(item, props, builtin);

                    mediaPlaybackItem.ApplyDisplayProperties(props);

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
            props.MusicProperties.AlbumArtist = item.AlbumArtists.IsNullorEmpty() ? "" : string.Join(", ", item.AlbumArtists);
            props.MusicProperties.AlbumTrackCount = item.TrackCount;
            props.MusicProperties.Artist = item.Performers.IsNullorEmpty() ? "" : string.Join(", ", item.Performers);
            props.MusicProperties.TrackNumber = item.Track;
            if (!item.Genres.IsNullorEmpty())
            {
                foreach (var g in item.Genres)
                {
                    props.MusicProperties.Genres.Add(g);
                }
            }
        }

        private async Task<string> GetBuiltInArtworkAsync(string id, string filePath)
        {
            var options = new Windows.Storage.Search.QueryOptions(Windows.Storage.Search.CommonFileQuery.DefaultQuery, new string[] { ".jpg", ".png", ".bmp" })
            {
                ApplicationSearchFilter = $"System.FileName:{id}.*"
            };

            var query = ApplicationData.Current.TemporaryFolder.CreateFileQueryWithOptions(options);
            var files = await query.GetFilesAsync();
            if (files.Count > 0)
            {
                return files[0].Path;
            }
            else
            {
                using (var tag = TagLib.File.Create(filePath))
                {

                    var pictures = tag.Tag.Pictures;
                    if (!pictures.IsNullorEmpty())
                    {
                        var fileName = $"{id}.{pictures[0].MimeType.Split('/').LastOrDefault().Replace("jpeg", "jpg")}";
                        try
                        {
                            var s = await ApplicationData.Current.TemporaryFolder.GetFileAsync(fileName);
                            if (s == null)
                            {
                                StorageFile cacheImg = await ApplicationData.Current.TemporaryFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
                                await FileIO.WriteBytesAsync(cacheImg, pictures[0].Data.Data);
                                return cacheImg.Path;
                            }
                            else
                            {
                                return s.Path;
                            }
                        }
                        catch (FileNotFoundException)
                        {
                            StorageFile cacheImg = await ApplicationData.Current.TemporaryFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
                            await FileIO.WriteBytesAsync(cacheImg, pictures[0].Data.Data);
                            return cacheImg.Path;
                        }
                    }
                    else
                    {
                        return string.Empty;
                    }
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
            if (b is bool boo)
            {
                if (boo)
                {
                    mediaPlaybackList.SetShuffledItems(mediaPlaybackList.Items);
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
        }

        public void Next()
        {
            if (mediaPlaybackList.CurrentItem == null || mediaPlaybackList.Items.Count < 1)
            {
                return;
            }
            if (mediaPlaybackList.AutoRepeatEnabled)
            {
                mediaPlaybackList.MoveNext();
                return;
            }

            if (mediaPlaybackList.CurrentItemIndex == mediaPlaybackList.Items.Count - 1)
            {
                Stop();
                return;
            }

            mediaPlaybackList.MoveNext();
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
            if (mediaPlaybackList.AutoRepeatEnabled)
            {
                mediaPlaybackList.MovePrevious();
                return;
            }
            if (mediaPlaybackList.CurrentItemIndex == 0)
            {
                Stop();
                return;
            }
            mediaPlaybackList.MovePrevious();
        }

        public void Play()
        {
            if (mediaPlaybackList == null || mediaPlaybackList.Items.IsNullorEmpty())
            {
                return;
            }
            mediaPlayer.Play();
        }

        public async void PlayWithRestart()
        {
            if (mediaPlaybackList == null)
            {
                return;
            }
            int i = 0;
            while (mediaPlaybackList.CurrentItem == null)
            {
                //mediaPlaybackList.MoveNext();
                i++;
                if (i > 10)
                    throw new TimeoutException("Media player received null source");
                await Task.Delay(500);
            }

            mediaPlayer.Source = mediaPlaybackList;
            mediaPlayer.Play();
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

        public void SkiptoItem(uint index)
        {
            if (index >= 0 && index < mediaPlaybackList.Items.Count)
            {
                mediaPlaybackList.MoveTo(index);
            }
        }
    }
}
