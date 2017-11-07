using Aurora.Music.Core;
using Aurora.Music.Core.Models;
using Aurora.Music.Core.Player;
using Aurora.Music.Core.Storage;
using Aurora.Shared.Extensions;
using Aurora.Shared.Helpers;
using Aurora.Shared.MVVM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.ApplicationModel.Core;
using Windows.Media.Playback;
using Windows.System.Threading;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace Aurora.Music.ViewModels
{
    class NowPlayingPageViewModel : ViewModelBase, IDisposable
    {
        private Uri artwork;
        public Uri CurrentArtwork
        {
            get { return artwork; }
            set { SetProperty(ref artwork, value); }
        }

        private Player player;

        private SongViewModel song;
        public SongViewModel Song
        {
            get { return song; }
            set { SetProperty(ref song, value); }
        }

        private string lyricHint = "Loading lyrics...";
        public string LyricHint
        {
            get { return lyricHint; }
            set { SetProperty(ref lyricHint, value); }
        }

        public void Init(SongViewModel song)
        {
            Song = song;
            CurrentArtwork = song.PicturePath.IsNullorEmpty() ? null : new Uri(song.PicturePath);
            lastUriPath = song.PicturePath;
            IsPlaying = player.IsPlaying;
            var t = ThreadPool.RunAsync(async x =>
            {
                _lastSong = song.ID;
                var substis = await WebRequester.GetSongLrcListAsync(Song.Title, Song.Performers.IsNullorEmpty() ? null : Song.Performers[0]);
                if (!substis.IsNullorEmpty())
                {
                    var l = new Lyric(LrcParser.Parser.Parse(await ApiRequestHelper.HttpGet(substis.First().Value), Song.Duration));
                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, () =>
                    {
                        Lyric.New(l);
                    });
                }
                else
                {
                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, () =>
                    {
                        Lyric.Clear();
                        LyricHint = "Can't find lyrics.";
                    });
                }
            });
        }

        private Uri placeHolder = new Uri(Consts.BlackPlaceholder);
        public Uri PlaceHolder
        {
            get { return placeHolder; }
            set { SetProperty(ref placeHolder, value); }
        }

        private TimeSpan currentPosition;
        public TimeSpan CurrentPosition
        {
            get { return currentPosition; }
            set { SetProperty(ref currentPosition, value); }
        }

        private TimeSpan currentDuration;
        public TimeSpan TotalDuration
        {
            get { return currentDuration; }
            set { SetProperty(ref currentDuration, value); }
        }

        private LyricViewModel lyric = new LyricViewModel();
        public LyricViewModel Lyric
        {
            get { return lyric; }
            set { SetProperty(ref lyric, value); }
        }

        internal void PositionChange(TimeSpan timeSpan)
        {
            if (Math.Abs((timeSpan - CurrentPosition).TotalSeconds) < 1)
            {
                return;
            }
            player.GotoPosition(timeSpan);
        }

        private string lastUriPath;

        private bool? isPlaying;
        public bool? IsPlaying
        {
            get { return isPlaying; }
            set { SetProperty(ref isPlaying, value); }
        }

        private int currentIndex;
        public int CurrentIndex
        {
            get { return currentIndex; }
            set { SetProperty(ref currentIndex, value); }
        }

        private double positionValue;
        public double PositionValue
        {
            get
            {
                return positionValue;
            }
            set
            {
                SetProperty(ref positionValue, value);
            }
        }

        public ObservableCollection<SongViewModel> NowPlayingList { get; set; } = new ObservableCollection<SongViewModel>();

        private string cuurentListPreview;
        public string NowListPreview
        {
            get { return cuurentListPreview; }
            set { SetProperty(ref cuurentListPreview, value); }
        }

        public NowPlayingPageViewModel()
        {
            player = MainPageViewModel.Current.GetPlayer();
            player.StatusChanged += Player_StatusChanged;
            player.PositionUpdated += Player_PositionUpdated;
        }

        public string TimeSpanFormat(TimeSpan t)
        {
            return t.ToString(@"m\:ss");
        }

        public Symbol NullableBoolToSymbol(bool? b)
        {
            if (b is bool bb)
            {
                return bb ? Symbol.Pause : Symbol.Play;
            }
            return Symbol.Play;
        }

        public string NullableBoolToString(bool? b)
        {
            if (b is bool bb)
            {
                return bb ? "Pause" : "Play";
            }
            return "Play";
        }

        public double PositionToValue(TimeSpan t1, TimeSpan total)
        {
            if (total == null || total == default(TimeSpan))
            {
                return 0;
            }
            return 100 * (t1.TotalMilliseconds / total.TotalMilliseconds);
        }

        public DelegateCommand GoPrevious
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    player?.GoPrevious();
                });
            }
        }

        public DelegateCommand GoNext
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    player?.GoNext();
                });
            }
        }

        public DelegateCommand PlayPause
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    player?.PlayPause();
                });
            }
        }

        public string ArtistsToString(string[] artists)
        {
            if (!artists.IsNullorEmpty())
            {
                return string.Join(", ", artists);
            }
            return "Unknown Artist";
        }

        private bool? isShuffle = false;
        public bool? IsShuffle
        {
            get { return isShuffle; }
            set
            {
                SetProperty(ref isShuffle, value);

                player?.ToggleShuffle(value);
            }
        }

        private bool? isLoop = false;
        private int _lastSong;

        public bool? IsLoop
        {
            get { return isLoop; }
            set
            {
                SetProperty(ref isLoop, value);

                player?.ToggleLoop(value);
            }
        }

        private async void Player_PositionUpdated(object sender, PositionUpdatedArgs e)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                CurrentPosition = e.Current;
                TotalDuration = e.Total;
                if (Lyric != null && CurrentPosition != default(TimeSpan))
                {
                    Lyric.Update(CurrentPosition);
                }
                PositionValue = PositionToValue(CurrentPosition, TotalDuration);
            });
        }

        private async void Player_StatusChanged(object sender, StatusChangedArgs e)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                switch (e.State)
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
                if (e.CurrentSong != null)
                {
                    Song = new SongViewModel(e.CurrentSong.Source.CustomProperties[Consts.SONG] as Song);
                    if (e.CurrentSong.Source.CustomProperties["Artwork"] is Uri u)
                    {
                        if (lastUriPath == u.AbsolutePath)
                        {

                        }
                        else
                        {
                            CurrentArtwork = u;
                            lastUriPath = u.AbsolutePath;
                        }
                    }
                    else
                    {
                        CurrentArtwork = null;
                    }
                    if (e.Items is IList<MediaPlaybackItem> l)
                    {
                        NowListPreview = $"{e.CurrentIndex + 1}/{l.Count}";
                        uint i = 1;
                        NowPlayingList.Clear();
                        foreach (var item in l)
                        {
                            var prop = item.GetDisplayProperties();
                            NowPlayingList.Add(new SongViewModel(item.Source.CustomProperties[Consts.SONG] as Song)
                            {
                                Index = i++
                            });
                        }
                        CurrentIndex = Convert.ToInt32(e.CurrentIndex);
                    }
                }
            });
            if (e.CurrentSong != null)
            {
                if (_lastSong != (int)e.CurrentSong.Source.CustomProperties[Consts.ID])
                {
                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
                    {
                        Lyric.Clear();
                        LyricHint = "Loading lyrics...";
                    });
                    _lastSong = (int)e.CurrentSong.Source.CustomProperties[Consts.ID];
                    var substis = await WebRequester.GetSongLrcListAsync(Song.Title, Song.Performers.IsNullorEmpty() ? null : Song.Performers[0]);
                    if (!substis.IsNullorEmpty())
                    {
                        var l = new Lyric(LrcParser.Parser.Parse(await ApiRequestHelper.HttpGet(substis.First().Value), Song.Duration));
                        await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
                        {
                            Lyric.New(l);
                        });
                    }
                    else
                    {
                        await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
                        {
                            Lyric.Clear();
                            LyricHint = "Can't find lyrics.";
                        });
                    }
                }
            }
        }

        public void Dispose()
        {
            player.PositionUpdated -= Player_PositionUpdated;
            player.StatusChanged -= Player_StatusChanged;

        }
    }
}
