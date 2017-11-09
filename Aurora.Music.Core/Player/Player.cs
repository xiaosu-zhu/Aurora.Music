using Aurora.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Aurora.Music.Core;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Foundation.Collections;
using Aurora.Shared.Extensions;
using Windows.Storage.Streams;
using Aurora.Music.Core.Storage;
using Windows.System.Threading;
using System.IO;
using Aurora.Music.Core.Models;
using Windows.Devices.Enumeration;
using Windows.Foundation;

namespace Aurora.Music.Core.Player
{
    public class Player : IDisposable
    {
        private MediaPlayer mediaPlayer;
        private MediaPlaybackList mediaPlaybackList;

        private DeviceInformation _autoDevice;

        private object lockable = new object();
        private int _songCountID;
        private IAsyncAction _addPlayListTask;

        public bool? IsPlaying { get; set; }

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

        public Player()
        {
            mediaPlayer = new MediaPlayer
            {
                AudioCategory = MediaPlayerAudioCategory.Media,

            };

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
            mediaPlayer.PlaybackSession.PositionChanged += PlaybackSession_PositionChangedAsync;
        }

        private void PlaybackSession_PositionChangedAsync(MediaPlaybackSession sender, object args)
        {
            lock (lockable)
            {
                if (mediaPlaybackList.CurrentItem == null)
                {
                    PositionUpdated?.Invoke(this, new PositionUpdatedArgs
                    {
                        Current = default(TimeSpan),
                        Total = default(TimeSpan)
                    });
                }
                else
                {
                    try
                    {
                        var id = (int)mediaPlaybackList.CurrentItem.Source.CustomProperties[Consts.ID];
                        var updatedArgs = new PositionUpdatedArgs
                        {
                            Current = sender.Position,
                            Total = (TimeSpan)mediaPlaybackList.CurrentItem.Source.CustomProperties[Consts.Duration]
                        };
                        PositionUpdated?.Invoke(this, updatedArgs);
                        if (_songCountID != id && updatedArgs.Current.TotalSeconds / updatedArgs.Total.TotalSeconds > 0.5)
                        {
                            _songCountID = id;
                            var opr = SQLOperator.Current();
                            AsyncHelper.RunSync(() => opr.SongCountAddAsync(id, 1));
                        }
                    }
                    catch (NullReferenceException)
                    {
                        PositionUpdated?.Invoke(this, new PositionUpdatedArgs
                        {
                            Current = sender.Position,
                            Total = default(TimeSpan)
                        });
                    }
                }
            }
        }

        public void GotoPosition(TimeSpan timeSpan)
        {
            mediaPlayer.PlaybackSession.Position = timeSpan;
        }

        private void PlaybackSession_PlaybackStateChanged(MediaPlaybackSession sender, object args)
        {
            switch (mediaPlayer.PlaybackSession.PlaybackState)
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
            StatusChanged?.Invoke(this, new StatusChangedArgs
            {
                State = mediaPlayer.PlaybackSession.PlaybackState,
                IsShuffle = mediaPlaybackList.ShuffleEnabled,
                IsLoop = mediaPlaybackList.AutoRepeatEnabled,
                CurrentSong = mediaPlaybackList.CurrentItem
            });
        }

        private void MediaPlaybackList_CurrentItemChanged(MediaPlaybackList sender, CurrentMediaPlaybackItemChangedEventArgs args)
        {
            switch (mediaPlayer.PlaybackSession.PlaybackState)
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
            StatusChanged?.Invoke(this, new StatusChangedArgs
            {
                State = mediaPlayer.PlaybackSession.PlaybackState,
                IsShuffle = mediaPlaybackList.ShuffleEnabled,
                IsLoop = mediaPlaybackList.AutoRepeatEnabled,
                CurrentSong = mediaPlaybackList.CurrentItem,
                CurrentIndex = mediaPlaybackList.CurrentItemIndex,
                Items = mediaPlaybackList.Items
            });
        }

        public async Task InstantPlay(IList<Song> items, int startIndex = 0)
        {
            if (items.IsNullorEmpty())
            {
                throw new ArgumentNullException("Items empty");
            }
            _addPlayListTask?.Cancel();


            mediaPlayer.Pause();
            mediaPlaybackList.Items.Clear();

            if (startIndex <= 0)
            {
                var item = items[0];
                var builtin = await GetBuiltInArtworkAsync(item.ID, item.FilePath);

                /// **Local files can only create from <see cref="StorageFile"/>**
                StorageFile file = await StorageFile.GetFileFromPathAsync(item.FilePath);

                var mediaSource = MediaSource.CreateFromStorageFile(file);
                mediaSource.CustomProperties[Consts.ID] = item.ID;
                mediaSource.CustomProperties[Consts.Duration] = item.Duration;
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

                var builtin = await GetBuiltInArtworkAsync(item.ID, item.FilePath);
                StorageFile file = await StorageFile.GetFileFromPathAsync(item.FilePath);

                var mediaSource = MediaSource.CreateFromStorageFile(file);
                mediaSource.CustomProperties[Consts.ID] = item.ID;
                mediaSource.CustomProperties[Consts.Duration] = item.Duration;
                mediaSource.CustomProperties[Consts.Artwork] = builtin.IsNullorEmpty() ? null : new Uri(builtin);
                item.PicturePath = builtin.IsNullorEmpty() ? item.PicturePath : builtin;
                mediaSource.CustomProperties[Consts.SONG] = item;

                var mediaPlaybackItem = new MediaPlaybackItem(mediaSource);
                var props = mediaPlaybackItem.GetDisplayProperties();

                await WriteProperties(item, props, builtin);

                mediaPlaybackItem.ApplyDisplayProperties(props);
                mediaPlaybackList.Items.Add(mediaPlaybackItem);
                mediaPlaybackList.StartingItem = mediaPlaybackItem;

                _addPlayListTask = ThreadPool.RunAsync(async (x) =>
                {
                    await AddtoPlayListFirstAsync(listBefore);
                    await AddtoPlayListAsync(listAfter);
                });
            }



            PlayWithRestart();
        }

        public async Task AddtoPlayListFirstAsync(IEnumerable<Song> items)
        {
            int i = 0;
            foreach (var item in items)
            {
                try
                {
                    var builtin = await GetBuiltInArtworkAsync(item.ID, item.FilePath);
                    StorageFile file = await StorageFile.GetFileFromPathAsync(item.FilePath);

                    var mediaSource = MediaSource.CreateFromStorageFile(file);

                    mediaSource.CustomProperties[Consts.ID] = item.ID;
                    mediaSource.CustomProperties[Consts.Duration] = item.Duration;
                    mediaSource.CustomProperties[Consts.Artwork] = builtin.IsNullorEmpty() ? null : new Uri(builtin);
                    item.PicturePath = builtin.IsNullorEmpty() ? item.PicturePath : builtin;
                    mediaSource.CustomProperties[Consts.SONG] = item;
                    var mediaPlaybackItem = new MediaPlaybackItem(mediaSource);
                    var props = mediaPlaybackItem.GetDisplayProperties();

                    await WriteProperties(item, props, builtin);

                    mediaPlaybackItem.ApplyDisplayProperties(props);

                    mediaPlaybackList.Items.Insert(i++, mediaPlaybackItem);
                }
                catch (FileNotFoundException)
                {
                    continue;
                }
                MediaPlaybackList_CurrentItemChanged(null, null);
            }
        }

        public async Task AddtoPlayListAsync(IEnumerable<Song> items)
        {
            foreach (var item in items)
            {
                try
                {
                    var builtin = await GetBuiltInArtworkAsync(item.ID, item.FilePath);
                    StorageFile file = await StorageFile.GetFileFromPathAsync(item.FilePath);

                    var mediaSource = MediaSource.CreateFromStorageFile(file);

                    mediaSource.CustomProperties[Consts.ID] = item.ID;
                    mediaSource.CustomProperties[Consts.Duration] = item.Duration;
                    mediaSource.CustomProperties[Consts.Artwork] = builtin.IsNullorEmpty() ? null : new Uri(builtin);
                    item.PicturePath = builtin.IsNullorEmpty() ? item.PicturePath : builtin;
                    mediaSource.CustomProperties[Consts.SONG] = item;
                    var mediaPlaybackItem = new MediaPlaybackItem(mediaSource);
                    var props = mediaPlaybackItem.GetDisplayProperties();

                    await WriteProperties(item, props, builtin);

                    mediaPlaybackItem.ApplyDisplayProperties(props);
                    mediaPlaybackList.Items.Add(mediaPlaybackItem);
                }
                catch (FileNotFoundException)
                {
                    continue;
                }
                MediaPlaybackList_CurrentItemChanged(null, null);
            }
        }

        private async Task WriteProperties(Song item, MediaItemDisplayProperties props, string pic)
        {
            if (pic == string.Empty)
            {
                props.Thumbnail = RandomAccessStreamReference.CreateFromUri(new Uri(Consts.BlackPlaceholder));
            }
            else
            {
                props.Thumbnail = RandomAccessStreamReference.CreateFromFile(await StorageFile.GetFileFromPathAsync(pic));
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

        private async Task<string> GetBuiltInArtworkAsync(int id, string filePath)
        {
            var options = new Windows.Storage.Search.QueryOptions(Windows.Storage.Search.CommonFileQuery.DefaultQuery, new string[] { ".jpg", ".png", ".bmp" })
            {
                ApplicationSearchFilter = $"filename:{id}"
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

        public void ToggleLoop(bool? b)
        {
            mediaPlaybackList.AutoRepeatEnabled = b ?? false;
        }

        public void ToggleShuffle(bool? b)
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

        public void GoNext()
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

        public void Seek(TimeSpan offset)
        {
            if (mediaPlayer.PlaybackSession.CanSeek)
            {
                mediaPlayer.PlaybackSession.Position = offset;
            }
        }

        public void GoPrevious()
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

        public void PlayWithRestart()
        {
            if (mediaPlaybackList == null || mediaPlaybackList.Items.IsNullorEmpty())
            {
                return;
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
    }

    public class StatusChangedArgs
    {
        public MediaPlaybackItem CurrentSong { get; set; }
        public MediaPlaybackState State { get; set; }
        public bool IsLoop { get; set; }
        public bool IsShuffle { get; set; }

        public uint CurrentIndex { get; set; }
        public IObservableVector<MediaPlaybackItem> Items { get; internal set; }
    }

    public class PositionUpdatedArgs
    {
        public TimeSpan Current { get; set; }
        public TimeSpan Total { get; set; }
    }
}
