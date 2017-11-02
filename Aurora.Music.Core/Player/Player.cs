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

namespace Aurora.Music.Core.Player
{
    public class Player : IDisposable
    {
        private MediaPlayer mediaPlayer;
        private MediaPlaybackList mediaPlaybackList;

        private object lockable = new object();
        private int _songCountID;

        public event EventHandler<PositionUpdatedArgs> PositionUpdated;
        public event EventHandler<StatusChangedArgs> StatusChanged;

        public Player()
        {
            mediaPlayer = new MediaPlayer
            {
                AudioCategory = MediaPlayerAudioCategory.Media,

            };
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

        private void PlaybackSession_PlaybackStateChanged(MediaPlaybackSession sender, object args)
        {
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

        public async Task NewPlayList(IEnumerable<SONG> items)
        {
            mediaPlayer.Pause();
            mediaPlaybackList.Items.Clear();
            foreach (var item in items)
            {
                StorageFile file = await StorageFile.GetFileFromPathAsync(item.FilePath);
                var mediaSource = MediaSource.CreateFromStorageFile(file);

                mediaSource.CustomProperties[Consts.ID] = item.ID;
                mediaSource.CustomProperties[Consts.Duration] = item.Duration;
                mediaSource.CustomProperties[Consts.Artwork] = new Uri(item.PicturePath.IsNullorEmpty() ? Consts.BlackPlaceholder : item.PicturePath);
                var mediaPlaybackItem = new MediaPlaybackItem(mediaSource);
                var props = mediaPlaybackItem.GetDisplayProperties();

                if (item.PicturePath.IsNullorEmpty())
                {
                    props.Thumbnail = RandomAccessStreamReference.CreateFromUri(new Uri(Consts.WhitePlaceholder));
                }
                else
                {
                    props.Thumbnail = RandomAccessStreamReference.CreateFromFile(await StorageFile.GetFileFromPathAsync(item.PicturePath));
                }

                props.Type = Windows.Media.MediaPlaybackType.Music;
                props.MusicProperties.Title = item.Title.IsNullorEmpty() ? item.FilePath.Split('\\').LastOrDefault() : item.Title;
                props.MusicProperties.AlbumTitle = item.Album.IsNullorEmpty() ? "" : item.Album;
                props.MusicProperties.AlbumArtist = item.AlbumArtists.IsNullorEmpty() ? "" : item.AlbumArtists.Replace("$|$", ", ");
                props.MusicProperties.AlbumTrackCount = item.TrackCount;
                props.MusicProperties.Artist = item.Performers.IsNullorEmpty() ? "" : item.Performers.Replace("$|$", ", ");
                props.MusicProperties.TrackNumber = item.Track;
                if (!item.Genres.IsNullorEmpty())
                {
                    var gen = item.Performers.Split(new string[] { "$|$" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var g in gen)
                    {
                        props.MusicProperties.Genres.Add(g);
                    }
                }


                mediaPlaybackItem.ApplyDisplayProperties(props);

                mediaPlaybackList.Items.Add(mediaPlaybackItem);
            }
            mediaPlayer.Source = mediaPlaybackList;
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
            if (mediaPlaybackList.Items.Count < 1)
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
            if (mediaPlaybackList.Items.Count < 1)
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
            if (mediaPlaybackList.Items.Count < 1)
            {
                return;
            }
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
