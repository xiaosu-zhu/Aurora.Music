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

        private void MediaPlaybackList_CurrentItemChanged(MediaPlaybackList sender, CurrentMediaPlaybackItemChangedEventArgs args)
        {
            //throw new NotImplementedException();
        }

        public void Dispose()
        {
            mediaPlayer.Dispose();
        }
    }
}
