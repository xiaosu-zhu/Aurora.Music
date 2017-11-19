using Aurora.Music.Core;
using Aurora.Music.Core.Models;
using Aurora.Music.Core.Storage;
using Aurora.Music.PlaybackEngine;
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
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
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

        private IPlayer player;

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
            lastUriPath = song.PicturePath.IsNullorEmpty() ? null : song.PicturePath;
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

        private Uri placeHolder = new Uri(Consts.NowPlaceholder);
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
            player.Seek(timeSpan);
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
                    player?.Previous();
                });
            }
        }

        public DelegateCommand GoNext
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    player?.Next();
                });
            }
        }

        public DelegateCommand PlayPause
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    if (IsPlaying is bool b)
                    {
                        if (b)
                        {
                            player?.Pause();
                        }
                        else
                        {
                            player?.Play();
                        }
                    }
                    else
                    {
                        player?.Play();
                    }
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

                player?.Shuffle(value);
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

                player?.Loop(value);
            }
        }

        public DelegateCommand CompactOverlay
        {
            get
            {
                return new DelegateCommand(async () =>
                {
                    await MainPage.Current.GotoComapctOverlay();
                });
            }
        }

        public DelegateCommand ShowLyricWindow
        {
            get
            {
                return new DelegateCommand(async () =>
                {
                    await MainPage.Current.ShowLyricWindow();
                });
            }
        }
        public DelegateCommand ReturnNormal
        {
            get
            {
                return new DelegateCommand(async () =>
                {
                    if (await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.Default))
                    {
                        (Window.Current.Content as Frame).Content = null;

                        if (MainPage.Current is MainPage m)
                        {
                            (Window.Current.Content as Frame).Content = m;
                        }
                        else if (MainPageViewModel.Current is MainPageViewModel v)
                        {
                            v.Dispose();
                            v = null;
                            (Window.Current.Content as Frame).Navigate(typeof(MainPage));
                        }
                    }
                });
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
                IsPlaying = player.IsPlaying;
                if (e.CurrentSong != null)
                {
                    Song = new SongViewModel(e.CurrentSong);
                    if (!Song.PicturePath.IsNullorEmpty())
                    {
                        if (lastUriPath == Song.PicturePath)
                        {

                        }
                        else
                        {
                            CurrentArtwork = Song.PicturePath == Consts.BlackPlaceholder ? null : new Uri(Song.PicturePath);
                            lastUriPath = Song.PicturePath == Consts.BlackPlaceholder ? null : Song.PicturePath;
                        }
                    }
                    else
                    {
                        CurrentArtwork = null;
                        lastUriPath = null;
                    }
                    if (e.Items is IList<Song> l)
                    {
                        NowListPreview = $"{e.CurrentIndex + 1}/{l.Count}";
                        uint i = 1;
                        NowPlayingList.Clear();
                        foreach (var item in l)
                        {
                            NowPlayingList.Add(new SongViewModel(item)
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
                if (_lastSong != e.CurrentSong.ID)
                {
                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
                    {
                        Lyric.Clear();
                        LyricHint = "Loading lyrics...";
                    });
                    _lastSong = e.CurrentSong.ID;
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
