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
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using System.Threading.Tasks;
using Aurora.Music.Pages;
using Windows.System;
using Windows.Storage;

namespace Aurora.Music.ViewModels
{
    class NowPlayingPageViewModel : ViewModelBase, IDisposable
    {
        internal event EventHandler SongChanged;

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

        private double downloadProgress;
        public double DownloadProgress
        {
            get { return downloadProgress; }
            set { SetProperty(ref downloadProgress, value); }
        }

        private SolidColorBrush currentColorBrush;
        public SolidColorBrush CurrentColorBrush
        {
            get { return currentColorBrush; }
            set { SetProperty(ref currentColorBrush, value); }
        }

        public void Init(SongViewModel song)
        {
            Song = song;
            _lastSong = new Song()
            {
                ID = song.ID,
                IsOnline = song.IsOnline,
                OnlineUri = new Uri(song.FilePath),
                FilePath = song.FilePath,
                Duration = song.Song.Duration,
                Album = song.Album,
                OnlineAlbumID = song.Song.OnlineAlbumID,
                OnlineID = song.Song.OnlineID
            };
            CurrentArtwork = song.Artwork;
            lastUriPath = song.Artwork?.AbsolutePath;
            IsPlaying = player.IsPlaying;
            DownloadProgress = MainPageViewModel.Current.DownloadProgress;
            SongChanged?.Invoke(song, EventArgs.Empty);
            CurrentRating = song.Rating;

            var dispa = CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, async () =>
            {
                CurrentColorBrush = new SolidColorBrush(await ImagingHelper.GetMainColor(CurrentArtwork));
                MainPageViewModel.Current.LeftTopColor = AdjustBrightness(CurrentColorBrush, 1);
                IsCurrentFavorite = await _lastSong.GetFavoriteAsync();
            });

            var t = ThreadPool.RunAsync(async x =>
            {
                var ext = MainPageViewModel.Current.LyricExtension;

                var result = await ext.ExecuteAsync(new KeyValuePair<string, object>("q", "lyric"), new KeyValuePair<string, object>("title", Song.Title), new KeyValuePair<string, object>("album", song.Album), new KeyValuePair<string, object>("artist", Song.Song.Performers.IsNullorEmpty() ? null : Song.Song.Performers[0]), new KeyValuePair<string, object>("ID", song.IsOnline ? song.Song.OnlineID : null));
                if (result != null)
                {
                    var l = new Lyric(LrcParser.Parser.Parse((string)result, Song.Song.Duration));
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



        public static void ColorToHSV(System.Drawing.Color color, out double hue, out double saturation, out double value)
        {
            int max = Math.Max(color.R, Math.Max(color.G, color.B));
            int min = Math.Min(color.R, Math.Min(color.G, color.B));

            hue = color.GetHue();
            saturation = (max == 0) ? 0 : 1d - (1d * min / max);
            value = max / 255d;
        }


        public static Color ColorFromHSV(double hue, double saturation, double value)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            value = value * 255;
            var v = Convert.ToByte(value);
            var p = Convert.ToByte(value * (1 - saturation));
            var q = Convert.ToByte(value * (1 - f * saturation));
            var t = Convert.ToByte(value * (1 - (1 - f) * saturation));

            if (hi == 0)
                return Color.FromArgb(255, v, t, p);
            else if (hi == 1)
                return Color.FromArgb(255, q, v, p);
            else if (hi == 2)
                return Color.FromArgb(255, p, v, t);
            else if (hi == 3)
                return Color.FromArgb(255, p, q, v);
            else if (hi == 4)
                return Color.FromArgb(255, t, p, v);
            else
                return Color.FromArgb(255, v, p, q);
        }

        public SolidColorBrush AdjustBrightness(SolidColorBrush b, double d)
        {
            if (b == null)
            {
                return new SolidColorBrush();
            }
            System.Drawing.Color color = System.Drawing.Color.FromArgb(b.Color.R, b.Color.G, b.Color.B);
            ColorToHSV(color, out var h, out var s, out var v);
            v *= d;
            b.Color = ColorFromHSV(h, s, v);
            return new SolidColorBrush(ColorFromHSV(h, s, v));
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

        internal void Unload()
        {
            player.DownloadProgressChanged -= Player_DownloadProgressChanged;
            player.PositionUpdated -= Player_PositionUpdated;
            player.StatusChanged -= Player_StatusChanged;
        }

        internal async Task FindFileAsync()
        {
            if (Song.IsOnline)
            {
                throw new InvalidOperationException("Can't open an online file");
            }
            var file = await StorageFile.GetFileFromPathAsync(Song.Song.FilePath);
            var option = new FolderLauncherOptions();
            option.ItemsToSelect.Add(file);
            await Launcher.LaunchFolderAsync(await file.GetParentAsync(), option);
        }

        public TimeSpan TotalDuration
        {
            get { return currentDuration; }
            set { SetProperty(ref currentDuration, value); }
        }

        internal async Task<AlbumViewModel> GetAlbumAsync()
        {
            if (_lastSong.IsOnline)
            {
                if (_lastSong.OnlineAlbumID.IsNullorEmpty())
                    return null;
                return new AlbumViewModel(await MainPageViewModel.Current.GetOnlineAlbumAsync(_lastSong.OnlineAlbumID));
            }
            return new AlbumViewModel(await SQLOperator.Current().GetAlbumByNameAsync(_lastSong.Album));
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
            player = Player.Current;
            player.StatusChanged += Player_StatusChanged;
            player.PositionUpdated += Player_PositionUpdated;
            player.DownloadProgressChanged += Player_DownloadProgressChanged;
        }

        private async void Player_DownloadProgressChanged(object sender, DownloadProgressChangedArgs e)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, () =>
            {
                DownloadProgress = 100 * e.Progress;
            });
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

        private double currentRating;
        public double CurrentRating
        {
            get
            {
                if (currentRating == 0)
                {
                    return -1;
                }
                return currentRating;
            }
            set
            {
                var rat = value;
                if (rat < 0)
                {
                    rat = 0;
                }
                if (currentRating == (uint)rat)
                    return;
                var t = ThreadPool.RunAsync(async work =>
                {
                    await _lastSong.WriteRatingAsync(rat);
                });
                Song.Rating = (uint)rat;
                SetProperty(ref currentRating, (uint)rat);
            }
        }

        public string FavGlyph(bool b)
        {
            return b ? "\uE00B" : "\uE006";
        }

        public SolidColorBrush FavForeground(bool b)
        {
            return b ? new SolidColorBrush(Color.FromArgb(0xff, 0xef, 0x69, 0x50)) : NowPlayingPage.Current.Resources["SystemControlForegroundBaseLowBrush"] as SolidColorBrush;
        }

        private bool isCurrentFac;
        public bool IsCurrentFavorite
        {
            get { return isCurrentFac; }
            set { SetProperty(ref isCurrentFac, value); }
        }

        public DelegateCommand ToggleFavorite
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    IsCurrentFavorite = !IsCurrentFavorite;
                    _lastSong.WriteFav(IsCurrentFavorite);
                });
            }
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
        private Song _lastSong;

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
                            m.RestoreContext();
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

        public string DownloadOrModify(bool b) => b ? "Download" : "Modify Tag";
        public string DownloadOrModifyIcon(bool b) => b ? "\uE896" : "\uE929";

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
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, async () =>
            {
                IsPlaying = player.IsPlaying;
                if (e.CurrentSong != null)
                {
                    if (!_lastSong.IsIDEqual(e.CurrentSong))
                    {
                        Song = new SongViewModel(e.CurrentSong);

                        CurrentRating = Song.Rating;

                        SongChanged?.Invoke(Song, EventArgs.Empty);

                        if (Song.Artwork != null)
                        {
                            if (lastUriPath == Song.Artwork.AbsolutePath)
                            {

                            }
                            else
                            {
                                CurrentArtwork = Song.Artwork;
                                CurrentColorBrush = new SolidColorBrush(await ImagingHelper.GetMainColor(CurrentArtwork));
                                MainPageViewModel.Current.LeftTopColor = AdjustBrightness(CurrentColorBrush, 1);
                                lastUriPath = Song.Artwork.AbsolutePath;
                            }
                        }
                        else
                        {
                            CurrentArtwork = null;
                            CurrentColorBrush = new SolidColorBrush(new UISettings().GetColorValue(UIColorType.Accent));
                            MainPageViewModel.Current.LeftTopColor = AdjustBrightness(CurrentColorBrush, 1);
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

                        IsCurrentFavorite = await e.CurrentSong.GetFavoriteAsync();
                    }
                }
            });
            if (e.CurrentSong != null)
            {
                if (!_lastSong.IsIDEqual(e.CurrentSong))
                {
                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
                    {
                        Lyric.Clear();
                        LyricHint = "Loading lyrics...";
                    });
                    _lastSong = e.CurrentSong;
                    var ext = MainPageViewModel.Current.LyricExtension;

                    var result = await ext.ExecuteAsync(new KeyValuePair<string, object>("q", "lyric"), new KeyValuePair<string, object>("title", Song.Title), new KeyValuePair<string, object>("album", song.Album), new KeyValuePair<string, object>("artist", Song.Song.Performers.IsNullorEmpty() ? null : Song.Song.Performers[0]), new KeyValuePair<string, object>("ID", song.IsOnline ? song.Song.OnlineID : null));
                    if (result != null)
                    {
                        var l = new Lyric(LrcParser.Parser.Parse((string)result, Song.Song.Duration));
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
