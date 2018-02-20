// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using AudioVisualizer;
using Aurora.Music.Controls;
using Aurora.Music.Core;
using Aurora.Music.Core.Models;
using Aurora.Music.Core.Storage;
using Aurora.Music.Pages;
using Aurora.Music.PlaybackEngine;
using Aurora.Shared;
using Aurora.Shared.Extensions;
using Aurora.Shared.Helpers;
using Aurora.Shared.MVVM;
using Microsoft.Toolkit.Uwp.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using TagLib;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Media.Casting;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System;
using Windows.System.Threading;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Aurora.Music.ViewModels
{
    class NowPlayingPageViewModel : ViewModelBase, IDisposable
    {
        internal event EventHandler SongChanged;
        private Song _lastSong;
        private DataTransferManager dataTransferManager;
        private CastingDevicePicker castingPicker;

        private BitmapImage artwork;
        public BitmapImage CurrentArtwork
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

        private string lyricHint = Consts.Localizer.GetString("LoadingLyricsText");
        public string LyricHint
        {
            get { return lyricHint; }
            set { SetProperty(ref lyricHint, value); }
        }

        private double downloadProgress;
        public double BufferProgress
        {
            get { return downloadProgress; }
            set { SetProperty(ref downloadProgress, value); }
        }

        public Color[] CurrentColor = new Color[2];

        private SolidColorBrush currentColorBrush = new SolidColorBrush();
        public SolidColorBrush CurrentColorBrush
        {
            get { return currentColorBrush; }
            set
            {
                SetProperty(ref currentColorBrush, value);
                var c = currentColorBrush.Color;
                c.ColorToHSV(out var h, out var s, out var v);
                CurrentColor[0] = ImagingHelper.ColorFromHSV(h, s, 0.8);
                CurrentColor[1] = ImagingHelper.ColorFromHSV(h, s, 0.6);
            }
        }

        private double playbackRate = PlaybackEngine.PlaybackEngine.Current.PlaybackRate;
        public double PlaybackRate
        {
            get { return playbackRate; }
            set
            {
                SetProperty(ref playbackRate, value);
                PlaybackEngine.PlaybackEngine.Current.PlaybackRate = value;
            }
        }

        private double olume = Settings.Current.PlayerVolume;
        public double Volume
        {
            get { return olume; }
            set
            {
                SetProperty(ref olume, value);
                Settings.Current.PlayerVolume = value;
                player.ChangeVolume(value);
            }
        }

        public async void Init(SongViewModel song)
        {
            //Initialize our picker object
            castingPicker = new CastingDevicePicker();

            //Set the picker to filter to video capable casting devices
            castingPicker.Filter.SupportsAudio = true;

            //Hook up device selected event
            castingPicker.CastingDeviceSelected += CastingPicker_CastingDeviceSelected;

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

            if (Song.Artwork != null)
            {
                CurrentArtwork = song.Artwork.IsLoopback ? new BitmapImage(song.Artwork) : await ImageCache.Instance.GetFromCacheAsync(song.Artwork);
                CurrentColorBrush = new SolidColorBrush(await ImagingHelper.GetMainColor(Song.Artwork));
                MainPageViewModel.Current.LeftTopColor = AdjustColorbyTheme(CurrentColorBrush);
                lastUriPath = Song.Artwork.AbsolutePath;
            }
            else
            {
                CurrentArtwork = null;
                CurrentColorBrush = new SolidColorBrush(new UISettings().GetColorValue(UIColorType.Accent));
                MainPageViewModel.Current.LeftTopColor = AdjustColorbyTheme(CurrentColorBrush);
                lastUriPath = null;
            }

            IsPlaying = player.IsPlaying;
            BufferProgress = MainPageViewModel.Current.BufferProgress;
            SongChanged?.Invoke(song, EventArgs.Empty);
            CurrentRating = song.Rating;
            CurrentIndex = MainPageViewModel.Current.CurrentIndex;
            var task = ThreadPool.RunAsync(async x =>
            {
                var favor = await _lastSong.GetFavoriteAsync();
                await CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
                {
                    IsCurrentFavorite = favor;
                });

            });

            var t = ThreadPool.RunAsync(async x =>
            {
                var ext = MainPageViewModel.Current.LyricExtension;
                if (song.IsPodcast)
                {
                    var l = new Lyric(LrcParser.Parser.Parse(song.Album, Song.Song.Duration));
                    await CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, () =>
                    {
                        Lyric.New(l);
                    });
                }
                else if (ext != null)
                {
                    var result = await ext.GetLyricAsync(song.Song, MainPageViewModel.Current.OnlineMusicExtension?.ServiceName);
                    if (result != null)
                    {
                        var l = new Lyric(LrcParser.Parser.Parse(result, Song.Song.Duration));
                        await CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, () =>
                        {
                            Lyric.New(l);
                        });
                    }
                    else
                    {
                        await CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, () =>
                        {
                            Lyric.Clear();
                            LyricHint = Consts.Localizer.GetString("NoLyricText");
                        });
                    }
                }
                else
                {
                    await CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, () =>
                    {
                        Lyric.Clear();
                        LyricHint = Consts.Localizer.GetString("NoLyricText");
                    });
                }
            });
        }

        public void ShowCastingUI(Rect rect)
        {
            castingPicker.Show(rect, Windows.UI.Popups.Placement.Above);
        }

        private async void CastingPicker_CastingDeviceSelected(CastingDevicePicker sender, CastingDeviceSelectedEventArgs args)
        {
            //Casting must occur from the UI thread.  This dispatches the casting calls to the UI thread.
            await CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, async () =>
            {
                //Create a casting conneciton from our selected casting device
                CastingConnection connection = args.SelectedCastingDevice.CreateCastingConnection();

                //Hook up the casting events
                connection.ErrorOccurred += Connection_ErrorOccurred;
                connection.StateChanged += Connection_StateChanged;

                //Cast the content loaded in the media element to the selected casting device
                await connection.RequestStartCastingAsync((player as Player).MediaPlayer.GetAsCastingSource());
            });

        }

        private bool sVisualizing = false;
        public bool IsVisualizing
        {
            get { return sVisualizing; }
            set
            {
                SetProperty(ref sVisualizing, value);
                MainPageViewModel.Current.IsVisualizing = value;
            }
        }

        public DelegateCommand ToggleVisualizing
        {
            get => new DelegateCommand(() =>
            {
                IsVisualizing = !IsVisualizing;
            });
        }

        private void Connection_StateChanged(CastingConnection sender, object args)
        {
        }

        private void Connection_ErrorOccurred(CastingConnection sender, CastingConnectionErrorOccurredEventArgs args)
        {
            MainPage.Current.PopMessage($"Casting Error: {args.ErrorStatus.ToString()}\r\n{args.Message}");
        }

        internal Task<AlbumViewModel> GetAlbumAsync()
        {
            return Song.GetAlbumAsync();
        }

        public SolidColorBrush AdjustColorbyTheme(SolidColorBrush b)
        {
            if (b == null)
            {
                return new SolidColorBrush();
            }

            if (NowPlayingPage.Current.IsDarkTheme())
            {
                return AdjustBrightness(b, 1);
            }
            else
            {
                return AdjustBrightness(b, 0.3333);
            }
        }

        public string GlyphOfOnline(SongViewModel s)
        {
            return s.IsPodcast ? "\uE95A" : "\uE753";
        }

        public string GlyphPreOfOnline(SongViewModel s)
        {
            return s.IsPodcast ? "\uED3C" : "\uE100";
        }

        public string VolumeToSymbol(double d)
        {
            if (d.AlmostEqualTo(0))
            {
                return "\uE992";
            }
            if (d < 33.333333)
            {
                return "\uE993";
            }
            if (d < 66.66666667)
            {
                return "\uE994";
            }
            return "\uE995";
        }

        public string VolumeToString(double d)
        {
            return d.ToString("0");
        }

        public string PlaybackRateToString(double d)
        {
            return $"{d.ToString("0.##")}\u00D7";
        }

        public SolidColorBrush AdjustBrightness(SolidColorBrush b, double d)
        {
            if (b == null)
            {
                return new SolidColorBrush();
            }
            b.Color.ColorToHSV(out var h, out var s, out var v);
            v = d;
            return new SolidColorBrush(ImagingHelper.ColorFromHSV(h, s, v));
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
            player.ItemsChanged -= Player_StatusChanged;
            player.PlaybackStatusChanged -= Player_PlaybackStatusChanged;

            dataTransferManager.DataRequested -= DataTransferManager_DataRequested;
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

        internal async Task WriteRatingValue(double value)
        {
            if (Song.Song.IsOnline)
            {
                throw new NotImplementedException("WriteRatingAsync on online");
            }
            else
            {
                await player.DetachCurrentItem();
                await Song.Song.WriteRatingAsync(value);
                await player.ReAttachCurrentItem();
            }
            CurrentRating = value;
        }

        internal async Task DeleteCurrentAsync(StorageDeleteOption op)
        {
            if (Song.IsOnline)
            {
                throw new InvalidOperationException("Online item can't delete");
            }
            var s = Song.Song.FilePath;
            player.RemoveCurrentItem();

            var file = await StorageFile.GetFileFromPathAsync(s);
            await file.DeleteAsync(op);
            MainPage.Current.PopMessage("Deleted");
        }

        internal void ShareCurrentAsync()
        {
            DataTransferManager.ShowShareUI();
        }

        internal async Task DowmloadOrModifyAsync()
        {
            if (Song.IsOnline)
            {
                StorageFolder folder;
                try
                {
                    if (!Settings.Current.DownloadPathToken.IsNullorEmpty())
                    {
                        folder = await Windows.Storage.AccessCache.StorageApplicationPermissions.
                            FutureAccessList.GetFolderAsync(Settings.Current.DownloadPathToken);
                    }
                    else
                    {
                        var lib = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Music);
                        folder = await lib.SaveFolder.CreateFolderAsync("Download", CreationCollisionOption.OpenIfExists);
                    }
                }
                catch (Exception)
                {
                    var lib = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Music);
                    folder = await lib.SaveFolder.CreateFolderAsync("Download", CreationCollisionOption.OpenIfExists);
                }
                MainPage.Current.PopMessage("Preparing to Download");
                downloadSong = song.Song;
                await Task.Run(async () =>
                {
                    try
                    {
                        var resultFile = await FileTracker.DownloadMusic(Song.Song, folder);
                        MainPage.Current.PopMessage(Consts.Localizer.GetString("DownloadCompletedText"));
                        MainPage.Current.ProgressUpdate(Consts.Localizer.GetString("CompletedText"), Consts.Localizer.GetString("DownloadCompletedText"));
                        await FileTracker.AddTags(resultFile, downloadSong);
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                });
            }
            else
            {
                var dialog = new TagDialog(Song);
                await dialog.ShowAsync();
            }
        }

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

        private int currentIndex = -1;
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
            player = PlaybackEngine.PlaybackEngine.Current;
            player.ItemsChanged += Player_StatusChanged;
            player.PlaybackStatusChanged += Player_PlaybackStatusChanged;
            player.PositionUpdated += Player_PositionUpdated;
            player.DownloadProgressChanged += Player_DownloadProgressChanged;

            dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += DataTransferManager_DataRequested;



            NowListPreview = MainPageViewModel.Current.NowListPreview;
            foreach (var item in MainPageViewModel.Current.NowPlayingList)
            {
                NowPlayingList.Add(item);
            }
        }

        private async void Player_PlaybackStatusChanged(object sender, PlaybackStatusChangedArgs e)
        {
            await CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                IsPlaying = e.PlaybackStatus == Windows.Media.Playback.MediaPlaybackState.Playing;
                IsLoop = e.IsLoop;
                IsShuffle = e.IsShuffle;
            });
        }

        private async void DataTransferManager_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            var d = args.Request.GetDeferral();
            var request = args.Request;
            try
            {
                if (Song.IsOnline)
                {
                    request.Data.SetWebLink(Song.Song.OnlineUri);
                    request.Data.SetText($"I'm listening {song.Title}, you can listen from {Song.Song.OnlineUri.OriginalString}");
                    if (Song.Song.PicturePath.IsNullorEmpty())
                    {
                        request.Data.Properties.Thumbnail = RandomAccessStreamReference.CreateFromUri(new Uri(Consts.BlackPlaceholder));
                    }
                    else
                    {
                        request.Data.Properties.Thumbnail = RandomAccessStreamReference.CreateFromUri(new Uri(Song.Song.PicturePath));
                    }
                    request.Data.Properties.Title = $"Share \"{Song.Title}\"";
                    request.Data.Properties.Description = $"Share link of what you're playing now";
                }
                else
                {
                    var files = new List<StorageFile>
                    {
                        await StorageFile.GetFileFromPathAsync(song.Song.FilePath)
                    };

                    request.Data.SetStorageItems(files);
                    if (Song.Song.PicturePath.IsNullorEmpty())
                    {
                        request.Data.Properties.Thumbnail = RandomAccessStreamReference.CreateFromUri(new Uri(Consts.BlackPlaceholder));
                    }
                    else
                    {
                        request.Data.Properties.Thumbnail = RandomAccessStreamReference.CreateFromFile(await StorageFile.GetFileFromPathAsync(Song.Song.PicturePath));
                    }

                    request.Data.Properties.Title = $"Share \"{Song.Title}\"";
                    request.Data.Properties.Description = $"Share music files to others.";
                }
                request.Data.Properties.ContentSourceApplicationLink = new Uri("as-music:");
            }
            catch (Exception e)
            {
                request.FailWithDisplayText(e.Message);
            }
            d.Complete();
        }

        private async void Player_DownloadProgressChanged(object sender, DownloadProgressChangedArgs e)
        {
            await CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, () =>
            {
                BufferProgress = 100 * e.Progress;
            });
        }

        public string TimeSpanFormat(TimeSpan t)
        {
            return t.ToString($@"m\{CultureInfoHelper.CurrentCulture.DateTimeFormat.TimeSeparator}ss", CultureInfoHelper.CurrentCulture);
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
                return bb ? Consts.Localizer.GetString("PauseText") : Consts.Localizer.GetString("PlayText");
            }
            return Consts.Localizer.GetString("PlayText");
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
                if (currentRating < 0.5)
                {
                    return -1;
                }
                return currentRating;
            }
            set
            {
                var rat = value;
                if (rat < 0.5)
                {
                    rat = 0;
                }

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
            set
            {
                SetProperty(ref isCurrentFac, value);
                _lastSong.WriteFav(value);
            }
        }

        public DelegateCommand GoPrevious
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    if (song.IsPodcast)
                    {
                        player?.Backward(TimeSpan.FromSeconds(10));
                    }
                    else
                    {
                        player?.Previous();
                    }
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

        public DelegateCommand Stop
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    player?.Stop();
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
                return string.Join(Consts.CommaSeparator, artists);
            }
            return Consts.UnknownArtists;
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
        public bool IsShuffleBool
        {
            get
            {
                return (isShuffle is bool a && a);
            }
            set
            {
                SetProperty(ref isShuffle, value);
                RaisePropertyChanged("IsShuffle");
                player?.Shuffle(value);
            }
        }

        private bool? isLoop = false;
        public bool? IsLoop
        {
            get { return isLoop; }
            set
            {
                SetProperty(ref isLoop, value);

                player?.Loop(value);
            }
        }
        public bool IsLoopBool
        {
            get
            {
                return (isLoop is bool a && a);
            }
            set
            {
                SetProperty(ref isLoop, value);
                RaisePropertyChanged("IsLoop");
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
        public DelegateCommand ShowSleepTimer
        {
            get
            {
                return new DelegateCommand(async () =>
                {
                    SleepTimer s = new SleepTimer();
                    await s.ShowAsync();
                });
            }
        }

        public DelegateCommand ShowEqualizer
        {
            get
            {
                return new DelegateCommand(async () =>
                {
                    EqualizerSettings s = new EqualizerSettings();
                    await s.ShowAsync();
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

        public bool CanDraw { get; private set; }
        private CustomVisualizer isualizer;
        private Song downloadSong;

        public CustomVisualizer Visualizer
        {
            get => isualizer;
            set
            {
                isualizer = value;
                MainPageViewModel.Current.VisualizerSource.SourceChanged += VisualizerSource_SourceChanged;
                if (MainPageViewModel.Current.VisualizerSource.Source != null)
                {
                    isualizer.Source = MainPageViewModel.Current.VisualizerSource.Source;
                }
            }
        }

        private void VisualizerSource_SourceChanged(object sender, IVisualizationSource args)
        {
            if (isualizer != null)
                isualizer.Source = MainPageViewModel.Current.VisualizerSource.Source;
        }

        public string DownloadOrModify(bool b) => b ? Consts.Localizer.GetString("DownloadText") : Consts.Localizer.GetString("ModifyTagsText");
        public string DownloadOrModifyIcon(bool b) => b ? "\uE896" : "\uE929";

        private async void Player_PositionUpdated(object sender, PositionUpdatedArgs e)
        {
            await CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
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

        private async void Player_StatusChanged(object sender, PlayingItemsChangedArgs e)
        {
            await CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, async () =>
            {
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
                                CurrentArtwork = song.Artwork.IsLoopback ? new BitmapImage(song.Artwork) : await ImageCache.Instance.GetFromCacheAsync(song.Artwork);
                                CurrentColorBrush = new SolidColorBrush(await ImagingHelper.GetMainColor(Song.Artwork));
                                MainPageViewModel.Current.LeftTopColor = AdjustColorbyTheme(CurrentColorBrush);
                                lastUriPath = Song.Artwork.AbsolutePath;
                            }
                        }
                        else
                        {
                            CurrentArtwork = null;
                            CurrentColorBrush = new SolidColorBrush(new UISettings().GetColorValue(UIColorType.Accent));
                            MainPageViewModel.Current.LeftTopColor = AdjustColorbyTheme(CurrentColorBrush);
                            lastUriPath = null;
                        }
                        if (e.Items is IReadOnlyList<Song> l)
                        {
                            NowListPreview = $"{e.CurrentIndex + 1}/{l.Count}";
                            NowPlayingList.Clear();
                            for (int i = 0; i < l.Count; i++)
                            {
                                NowPlayingList.Add(new SongViewModel(l[i])
                                {
                                    Index = (uint)i
                                });
                            }
                            if (e.CurrentIndex < NowPlayingList.Count)
                            {
                                CurrentIndex = e.CurrentIndex;
                            }
                        }
                        IsCurrentFavorite = await e.CurrentSong.GetFavoriteAsync();
                    }
                }
            });
            if (e.CurrentSong != null)
            {
                if (!_lastSong.IsIDEqual(e.CurrentSong))
                {
                    await CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
                    {
                        Lyric.Clear();
                        LyricHint = Consts.Localizer.GetString("LoadingLyricsText");
                    });
                    _lastSong = e.CurrentSong;
                    var ext = MainPageViewModel.Current.LyricExtension;
                    if (song.IsPodcast)
                    {
                        var l = new Lyric(LrcParser.Parser.Parse(song.Album, Song.Song.Duration));
                        await CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, () =>
                        {
                            Lyric.New(l);
                        });
                    }
                    else if (ext != null)
                    {
                        var result = await ext.GetLyricAsync(song.Song, MainPageViewModel.Current.OnlineMusicExtension?.ServiceName);
                        if (result != null)
                        {
                            var l = new Lyric(LrcParser.Parser.Parse(result, Song.Song.Duration));
                            await CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
                            {
                                Lyric.New(l);
                            });
                        }
                        else
                        {
                            await CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
                            {
                                Lyric.Clear();
                                LyricHint = Consts.Localizer.GetString("NoLyricText");
                            });
                        }
                    }
                    else
                    {
                        await CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
                        {
                            Lyric.Clear();
                            LyricHint = Consts.Localizer.GetString("NoLyricText");
                        });
                    }
                }
            }
        }

        public void Dispose()
        {
            player.PositionUpdated -= Player_PositionUpdated;
            player.ItemsChanged -= Player_StatusChanged;
            player.PlaybackStatusChanged -= Player_PlaybackStatusChanged;
            dataTransferManager.DataRequested -= DataTransferManager_DataRequested;
            castingPicker.CastingDeviceSelected -= CastingPicker_CastingDeviceSelected;
            castingPicker = null;
        }
    }
}
