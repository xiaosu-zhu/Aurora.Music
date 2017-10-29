using Aurora.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Foundation.Collections;
using Aurora.Shared.Extensions;
using Windows.Storage.Streams;

namespace Aurora.Music.Core.Player
{
    public class Player : IDisposable
    {
        private MediaPlayer mediaPlayer;
        private MediaPlaybackList mediaPlaybackList;

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
            mediaPlayer.PlaybackSession.PositionChanged += PlaybackSession_PositionChanged;
        }

        private void PlaybackSession_PositionChanged(MediaPlaybackSession sender, object args)
        {
            PositionUpdated?.Invoke(this, new PositionUpdatedArgs
            {
                Current = sender.Position,
                Total = (TimeSpan)mediaPlaybackList.CurrentItem.Source.CustomProperties["Duration"]
            });
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
                CurrentSong = mediaPlaybackList.CurrentItem
            });
        }

        public async Task NewPlayList(IEnumerable<Storage.Song> items)
        {
            mediaPlayer.Pause();
            mediaPlaybackList.Items.Clear();
            foreach (var item in items)
            {
                StorageFile file = await StorageFile.GetFileFromPathAsync(item.FilePath);
                var mediaSource = MediaSource.CreateFromStorageFile(file);

                mediaSource.CustomProperties["ID"] = item.ID;
                mediaSource.CustomProperties["Duration"] = item.Duration;
                mediaSource.CustomProperties["Artwork"] = new Uri(item.PicturePath);
                var mediaPlaybackItem = new MediaPlaybackItem(mediaSource);

                var props = mediaPlaybackItem.GetDisplayProperties();
                props.Type = Windows.Media.MediaPlaybackType.Music;
                props.MusicProperties.Title = item.Title;
                props.MusicProperties.AlbumTitle = item.Album;
                props.MusicProperties.AlbumArtist = item.AlbumArtists.IsNullorEmpty() ? null : item.AlbumArtists.Replace("$|$", ", ");
                props.MusicProperties.AlbumTrackCount = item.TrackCount;
                props.MusicProperties.Artist = item.Performers.IsNullorEmpty() ? null : item.Performers.Replace("$|$", ", ");
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

        public async Task NewPlayList(IEnumerable<Models.Song> items)
        {
            mediaPlayer.Pause();
            mediaPlaybackList.Items.Clear();
            foreach (var item in items)
            {
                StorageFile file = await StorageFile.GetFileFromPathAsync(item.FilePath);
                var mediaSource = MediaSource.CreateFromStorageFile(file);

                mediaSource.CustomProperties["ID"] = item.ID;
                var mediaPlaybackItem = new MediaPlaybackItem(mediaSource);

                var props = mediaPlaybackItem.GetDisplayProperties();
                props.Type = Windows.Media.MediaPlaybackType.Music;
                props.Thumbnail = RandomAccessStreamReference.CreateFromFile(await StorageFile.GetFileFromPathAsync(item.PicturePath));
                props.MusicProperties.Title = item.Title;
                props.MusicProperties.AlbumTitle = item.Album;
                props.MusicProperties.AlbumArtist = item.AlbumArtists.IsNullorEmpty() ? null : string.Join(", ", item.AlbumArtists);
                props.MusicProperties.AlbumTrackCount = item.TrackCount;
                props.MusicProperties.Artist = item.Performers.IsNullorEmpty() ? null : string.Join(", ", item.Performers);
                props.MusicProperties.TrackNumber = item.Track;
                if (!item.Genres.IsNullorEmpty())
                    foreach (var g in item.Genres)
                    {
                        props.MusicProperties.Genres.Add(g);
                    }

                mediaPlaybackItem.ApplyDisplayProperties(props);

                mediaPlaybackList.Items.Add(mediaPlaybackItem);
            }
            mediaPlayer.Source = mediaPlaybackList;
        }

        public void ToggleLoop()
        {
            mediaPlaybackList.AutoRepeatEnabled = true;
        }

        public void ToggleShuffle()
        {
            mediaPlaybackList.SetShuffledItems(mediaPlaybackList.Items);
            mediaPlaybackList.ShuffleEnabled = true;
        }

        public void GoNext()
        {
            if (mediaPlaybackList.Items.Count < 1)
            {
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
            if (mediaPlaybackList.CurrentItemIndex == mediaPlaybackList.Items.Count - 1)
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
    }

    public class PositionUpdatedArgs
    {
        public TimeSpan Current { get; set; }
        public TimeSpan Total { get; set; }
    }
}
