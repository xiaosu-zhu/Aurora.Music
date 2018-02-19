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
using Aurora.Shared.Extensions;
using Aurora.Shared.Helpers;
using Aurora.Shared.MVVM;
using Microsoft.Toolkit.Uwp.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.System.Threading;
using Windows.UI.Text;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Aurora.Music.ViewModels
{
    class MainPageViewModel : ViewModelBase, IDisposable
    {
        private Windows.System.Display.DisplayRequest _displayRequest;

        public void ActivateDisplay()
        {
            //create the request instance if needed
            if (_displayRequest == null)
                _displayRequest = new Windows.System.Display.DisplayRequest();

            //make request to put in active state
            _displayRequest.RequestActive();
        }

        public void ReleaseDisplay()
        {
            //must be same instance, so quit if it doesn't exist
            if (_displayRequest == null)
                return;

            //undo the request
            _displayRequest.RequestRelease();
        }


        public static MainPageViewModel Current;

        public List<HamPanelItem> HamList { get; set; } = new List<HamPanelItem>()
        {
            new HamPanelItem
            {
                Title = Consts.Localizer.GetString("HomeText"),
                TargetType = typeof(HomePage),
                Icon="\uE80F",
                IsCurrent = true
            },
            new HamPanelItem
            {
                Title = Consts.Localizer.GetString("LibraryText"),
                Icon="\uE2AC",
                TargetType = typeof(LibraryPage)
            },
            new HamPanelItem
            {
                Title = Consts.Localizer.GetString("DouText"),
                Icon = "\uEFA9",
                TargetType = typeof(DoubanPage)
            },
            new HamPanelItem
            {
                Title = "Explorer Podcasts",
                Icon="\uE774",
                TargetType = typeof(PodcastMarket)
            },
        };

        public ObservableCollection<SongViewModel> NowPlayingList { get; set; } = new ObservableCollection<SongViewModel>();
        private IPlayer player;

        private SolidColorBrush _lastLeftTop;
        private SolidColorBrush leftTopColor;
        public SolidColorBrush LeftTopColor
        {
            get { return leftTopColor; }
            set
            {
                _lastLeftTop = leftTopColor;
                SetProperty(ref leftTopColor, value);
                ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
                titleBar.ButtonForegroundColor = leftTopColor.Color;
                titleBar.ForegroundColor = leftTopColor.Color;
            }
        }

        public LyricExtension LyricExtension { get; private set; }
        public OnlineMusicExtension OnlineMusicExtension { get; private set; }
        public OnlineMetaExtension OnlineMetaExtension { get; private set; }

        private bool needShowPanel = false;
        public bool NeedShowPanel
        {
            get { return needShowPanel; }
            set
            {
                if (MainPage.Current.CanShowPanel)
                    SetProperty(ref needShowPanel, value);
                else
                {
                    SetProperty(ref needShowPanel, false);
                }
            }
        }

        private BitmapImage currentArtwork;
        public BitmapImage CurrentArtwork
        {
            get { return currentArtwork; }
            set { SetProperty(ref currentArtwork, value); }
        }

        private double nowPlayingPosition;
        public double NowPlayingPosition
        {
            get { return nowPlayingPosition; }
            set { SetProperty(ref nowPlayingPosition, value); }
        }

        private bool? isPlaying;
        public bool? IsPlaying
        {
            get { return isPlaying; }
            set { SetProperty(ref isPlaying, value); }
        }

        internal void RestoreLastTitle()
        {
            NeedShowTitle = _lastneedshow;
            Title = _lasttitle;
            LeftTopColor = _lastLeftTop;
        }

        private string placeholderText = Consts.Localizer.GetString("SearchInLibraryText");
        public string PlaceholderText
        {
            get { return placeholderText; }
            set { SetProperty(ref placeholderText, value); }
        }

        private TimeSpan currentPosition;
        public TimeSpan CurrentPosition
        {
            get { return currentPosition; }
            set { SetProperty(ref currentPosition, value); }
        }

        private TimeSpan totalDuration;
        public TimeSpan TotalDuration
        {
            get { return totalDuration; }
            set { SetProperty(ref totalDuration, value); }
        }

        private string currentTitle;
        public string CurrentTitle
        {
            get { return currentTitle.IsNullorEmpty() ? Consts.Localizer.GetString("AppNameText") : currentTitle; }
            set { SetProperty(ref currentTitle, value); }
        }

        private bool isPodcast;
        public bool IsPodcast
        {
            get { return isPodcast; }
            set { SetProperty(ref isPodcast, value); }
        }

        private string currentAlbum;
        private string lastUriPath;

        public string CurrentAlbum
        {
            get { return currentAlbum.IsNullorEmpty() ? Consts.Localizer.GetString("NotPlayingText") : currentAlbum; }
            set { SetProperty(ref currentAlbum, value); }
        }

        private string nowListPreview = "-/-";

        public string NowListPreview
        {
            get { return nowListPreview; }
            set { SetProperty(ref nowListPreview, value); }
        }

        private int currentIndex = -1;
        public int CurrentIndex
        {
            get { return currentIndex; }
            set { SetProperty(ref currentIndex, value); }
        }

        private bool _lastneedshow;
        private bool needHeader;
        public bool NeedShowTitle
        {
            get { return needHeader; }
            set
            {
                _lastneedshow = needHeader;
                SetProperty(ref needHeader, value);
            }
        }

        private string _lasttitle;
        private string title;
        public string Title
        {
            get { return title; }
            set
            {
                _lasttitle = title;
                SetProperty(ref title, value);
            }
        }

        internal async Task<Song> GetOnlineSongAsync(string id)
        {
            var querys = new ValueSet()
            {
                new KeyValuePair<string,object>("q", "online_music"),
                new KeyValuePair<string, object>("action", "song"),
                new KeyValuePair<string, object>("id", id),
                new KeyValuePair<string, object>("bit_rate", Settings.Current.GetPreferredBitRate())
            };
            var songResult = await OnlineMusicExtension.ExecuteAsync(querys);
            if (songResult is Song s)
            {
                return s;
            }
            return null;
        }

        internal async Task<Album> GetOnlineAlbumAsync(string id)
        {
            var querys = new ValueSet()
            {
                new KeyValuePair<string,object>("q", "online_music"),
                new KeyValuePair<string, object>("action", "album"),
                new KeyValuePair<string, object>("id", id),
                new KeyValuePair<string, object>("bit_rate", Settings.Current.GetPreferredBitRate())
            };
            var songResult = await OnlineMusicExtension.ExecuteAsync(querys);
            if (songResult is Album s)
            {
                return s;
            }
            return null;
        }

        internal async Task<AlbumInfo> GetAlbumInfoAsync(string album, string artist)
        {
            var querys = new ValueSet()
            {
                new KeyValuePair<string,object>("album", album),
                new KeyValuePair<string, object>("artist", artist),
                new KeyValuePair<string, object>("action", "album"),
                new KeyValuePair<string, object>("q", "online_meta"),
            };
            var result = await OnlineMetaExtension.ExecuteAsync(querys);
            if (result is AlbumInfo s)
            {
                return s;
            }
            return null;
        }

        internal async Task<Core.Models.Artist> GetArtistInfoAsync(string artist)
        {
            var querys = new ValueSet()
            {
                new KeyValuePair<string, object>("action", "artist"),
                new KeyValuePair<string, object>("artist", artist),
                new KeyValuePair<string, object>("q", "online_meta"),
            };
            var result = await OnlineMetaExtension.ExecuteAsync(querys);
            if (result is Core.Models.Artist s)
            {
                return s;
            }
            return null;
        }

        public DelegateCommand GoPrevious
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    if (IsPodcast)
                    {
                        player?.Backward(TimeSpan.FromSeconds(10));
                    }
                    else
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
                    if (IsPodcast)
                    {
                        player?.Forward(TimeSpan.FromSeconds(30));
                    }
                    else
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

        public DelegateCommand GotoSettings
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    MainPage.Current.GoBackFromNowPlaying();
                    MainPage.Current.Navigate(typeof(SettingsPage));
                });
            }
        }

        public DelegateCommand GotoAbout
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    MainPage.Current.GoBackFromNowPlaying();
                    MainPage.Current.Navigate(typeof(AboutPage));
                });
            }
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
        public bool? IsLoop
        {
            get { return isLoop; }
            set
            {
                SetProperty(ref isLoop, value);

                player?.Loop(value);
            }
        }

        private bool darkAccent;
        public bool IsDarkAccent
        {
            get { return darkAccent; }
            set { SetProperty(ref darkAccent, value); }
        }

        public MainPageViewModel()
        {
            player = PlaybackEngine.PlaybackEngine.Current;
            AttachVisualizerSource();
            Current = this;
            player.DownloadProgressChanged += Player_DownloadProgressChanged;
            player.ItemsChanged += Player_StatusChanged;
            player.PlaybackStatusChanged += Player_PlaybackStatusChanged;
            player.PositionUpdated += Player_PositionUpdated;
            var t = ThreadPool.RunAsync(async x =>
            {
                if (Settings.Current.LastUpdateBuild < SystemInfoHelper.GetPackageVersionNum())
                {
                    var k = CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, async () =>
                    {
                        UpdateInfo i = new UpdateInfo();
                        await i.ShowAsync();
                        Settings.Current.LastUpdateBuild = SystemInfoHelper.GetPackageVersionNum();
                        Settings.Current.Save();
                    });
                }
                await ReloadExtensions();
                await FindFileChanges();
            });
        }

        public void AttachVisualizerSource()
        {
            if (visualizerSource != null)
            {
                visualizerSource.SourceChanged -= VisualizerSource_SourceChanged;
                visualizerSource = null;
            }
            if (player is Player p)
            {
                visualizerSource = new PlaybackSource(p.MediaPlayer);
                visualizerSource.SourceChanged += VisualizerSource_SourceChanged;
                if (visualizerSource.Source != null)
                {
                    visualizerSource.Source.IsSuspended = true;
                }
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
            if (e.PlaybackStatus == Windows.Media.Playback.MediaPlaybackState.Playing && Settings.Current.PreventLockscreen)
            {
                ActivateDisplay();
            }
        }

        public async Task ReloadExtensions()
        {
            LyricExtension = await Extension.Load<LyricExtension>(Settings.Current.LyricExtensionID);
            if (Settings.Current.OnlinePurchase)
            {
                OnlineMusicExtension = await Extension.Load<OnlineMusicExtension>(Settings.Current.OnlineMusicExtensionID);
            }
            else
            {
                OnlineMusicExtension = null;
            }

            OnlineMetaExtension = await Extension.Load<OnlineMetaExtension>(Settings.Current.MetaExtensionID);

            await CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                if (OnlineMusicExtension != null)
                {
                    PlaceholderText = Consts.Localizer.GetString("SearchInWebText");
                }
            });
        }
        private void VisualizerSource_SourceChanged(object sender, IVisualizationSource args)
        {
            args.IsSuspended = !IsVisualizing;
        }

        private double downloadProgress;
        private string _lastQuery;
        private PlaybackSource visualizerSource;
        public PlaybackSource VisualizerSource => visualizerSource;

        public double BufferProgress
        {
            get { return downloadProgress; }
            set { SetProperty(ref downloadProgress, value); }
        }

        private async void Player_DownloadProgressChanged(object sender, DownloadProgressChangedArgs e)
        {
            await CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                BufferProgress = 100 * e.Progress;
            });
        }

        public ObservableCollection<GenericMusicItemViewModel> SearchItems { get; set; } = new ObservableCollection<GenericMusicItemViewModel>();
        private bool sVisualizing;
        public bool IsVisualizing
        {
            get => sVisualizing; set
            {
                sVisualizing = value;
                if (visualizerSource.Source != null) visualizerSource.Source.IsSuspended = !value;
            }
        }

        internal async Task Search(string text)
        {
            var dd = CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                MainPage.Current.ShowAutoSuggestPopup();
            });
            _lastQuery = string.Copy(text);
            var s = string.Copy(_lastQuery);
            List<Task> tasks = new List<Task>();
            if (OnlineMusicExtension != null)
            {
                tasks.Add(Task.Run(async () =>
                {
                    var querys = new ValueSet()
                    {
                        new KeyValuePair<string,object>("q", "online_music"),
                        new KeyValuePair<string, object>("action", "search"),
                        new KeyValuePair<string, object>("keyword", text)
                    };
                    var webResult = await OnlineMusicExtension.ExecuteAsync(querys);
                    if (webResult is IEnumerable<OnlineMusicItem> items)
                    {
                        if (MainPage.Current.CanAdd && s.Equals(_lastQuery, StringComparison.Ordinal))
                            await CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
                            {
                                lock (MainPage.Current.Lockable)
                                {
                                    if (SearchItems.Count > 0 && SearchItems[0].InnerType == MediaType.Placeholder)
                                    {
                                        SearchItems.Clear();
                                    }
                                    foreach (var item in items.Reverse())
                                    {
                                        SearchItems.Insert(0, new GenericMusicItemViewModel(item));
                                    }
                                }
                            });
                    }
                }));
            }
            if (Settings.Current.ShowPodcastsWhenSearch)
                tasks.Add(Task.Run(async () =>
                {
                    await Task.Delay(1000);
                    if (!s.Equals(_lastQuery, StringComparison.Ordinal))
                    {
                        return;
                    }
                    var podcasts = await SearchPodcasts(text);

                    if (MainPage.Current.CanAdd && !podcasts.IsNullorEmpty() && s.Equals(_lastQuery, StringComparison.Ordinal))
                        await CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
                        {
                            lock (MainPage.Current.Lockable)
                            {
                                if (SearchItems.Count > 0 && SearchItems[0].InnerType == MediaType.Placeholder)
                                {
                                    SearchItems.Clear();
                                }
                                podcasts.Reverse();

                                foreach (var item in podcasts)
                                {
                                    SearchItems.Insert(0, new GenericMusicItemViewModel(item));
                                }
                            }
                        });
                }));
            tasks.Add(Task.Run(async () =>
            {
                var result = await FileReader.Search(text);

                if (MainPage.Current.CanAdd && !result.IsNullorEmpty() && s.Equals(_lastQuery, StringComparison.Ordinal))
                    await CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
                    {
                        lock (MainPage.Current.Lockable)
                        {
                            if (SearchItems.Count > 0 && SearchItems[0].InnerType == MediaType.Placeholder)
                            {
                                SearchItems.Clear();
                            }
                            foreach (var item in result)
                            {
                                SearchItems.Add(new GenericMusicItemViewModel(item));
                            }
                        }

                    });
            }));
            await Task.WhenAll(tasks);
            await CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            { MainPage.Current.HideAutoSuggestPopup(); });
        }

        private async Task<List<OnlineMusicItem>> SearchPodcasts(string text)
        {
            return await Podcast.SearchPodcasts(text);
        }

        public async Task FindFileChanges()
        {
            var addedFiles = await FileTracker.FindChanges();
            if (!(addedFiles.Count == 0))
            {
                var reader = new FileReader();
                reader.ProgressUpdated += Reader_ProgressUpdated;
                reader.Completed += Reader_Completed;
                await reader.ReadFileandSave(addedFiles);
            }
        }

        private void Reader_Completed(object sender, EventArgs e)
        {
            var t = CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, async () =>
            {
                MainPage.Current.ProgressUpdate(100);
                MainPage.Current.ProgressUpdate(Consts.Localizer.GetString("FileUpdatingText"), Consts.Localizer.GetString("CompletedText"));
                await Task.Delay(1500);
                MainPage.Current.ProgressUpdate(false);
            });
        }

        private void Reader_ProgressUpdated(object sender, ProgressReport e)
        {
            var t = CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                MainPage.Current.ProgressUpdate();
                MainPage.Current.ProgressUpdate(e.Current * 100.0 / e.Total);
                MainPage.Current.ProgressUpdate(Consts.Localizer.GetString("FileUpdatingText"), e.Description);
            });
        }

        private async void Reader_NewSongsAdded(object sender, SongsAddedEventArgs e)
        {
            await new FileReader().AddToAlbums(e.NewSongs);
        }

        private async void Player_PositionUpdated(object sender, PositionUpdatedArgs e)
        {
            await CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                CurrentPosition = e.Current;
                TotalDuration = e.Total;
            });
        }

        public string GlyphPreOfOnline(bool s)
        {
            return s ? "\uED3C" : "\uE100";
        }

        public string GlyphNextOfOnline(bool s)
        {
            return s ? "\uED3D" : "\uE101";
        }

        private async void Player_StatusChanged(object sender, PlayingItemsChangedArgs e)
        {
            await CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, async () =>
            {
                if (e.CurrentIndex == -1)
                {
                    NowPlayingList.Clear();
                    NowListPreview = "-/-";
                    CurrentTitle = null;
                    CurrentAlbum = null;
                    CurrentArtwork = null;
                    lastUriPath = null;
                    CurrentIndex = -1;
                    NeedShowPanel = false;
                    IsPodcast = false;
                    ReleaseDisplay();
                    return;
                }

                if (e.CurrentSong != null)
                {
                    var p = e.CurrentSong;
                    CurrentTitle = p.Title.IsNullorEmpty() ? p.FilePath.Split('\\').LastOrDefault() : p.Title;
                    IsPodcast = p.IsPodcast;
                    CurrentAlbum = p.Album.IsNullorEmpty() ? (p.Performers.IsNullorEmpty() ? Consts.UnknownAlbum : string.Join(Consts.CommaSeparator, p.Performers)) : p.Album;
                    if (!p.PicturePath.IsNullorEmpty())
                    {
                        if (lastUriPath == p.PicturePath)
                        {

                        }
                        else
                        {
                            var u = new Uri(p.PicturePath);
                            CurrentArtwork = u.IsLoopback ? new BitmapImage(u) : await ImageCache.Instance.GetFromCacheAsync(u);
                            lastUriPath = p.PicturePath;
                        }
                    }
                    else
                    {
                        CurrentArtwork = null; lastUriPath = string.Empty;
                    }
                    var task = Task.Run(() =>
                    {
                        Tile.SendNormal(CurrentTitle, CurrentAlbum, string.Join(Consts.CommaSeparator, p.Performers ?? new string[] { }), p.PicturePath);
                    });
                }
                if (e.Items is IReadOnlyList<Song> l)
                {
                    NowListPreview = $"{e.CurrentIndex + 1}/{l.Count}";
                    NowPlayingList.Clear();
                    for (int i = 0; i < l.Count; i++)
                    {
                        NowPlayingList.Add(new SongViewModel(l[i])
                        {
                            Index = (uint)i,
                        });
                    }
                }
                if (e.CurrentIndex < NowPlayingList.Count)
                {
                    CurrentIndex = e.CurrentIndex;
                }
                if (MainPage.Current.IsCurrentDouban)
                {
                    return;
                }
                else
                {
                    NeedShowPanel = true;
                }
            });
        }

        internal async Task SavePointAsync()
        {
            if (!NowPlayingList.IsNullorEmpty())
            {
                var status = new PlayerStatus(NowPlayingList.Select(s => s.Song), CurrentIndex, CurrentPosition);
                await status.SaveAsync();
            }
        }

        public void Dispose()
        {
            //player.StatusChanged -= Player_StatusChanged;
            //player.PositionUpdated -= Player_PositionUpdated;
        }

        internal async Task InstantPlay(IList<Song> songs, int startIndex = 0)
        {
            await player.NewPlayList(songs, startIndex);
            player.Play();
        }

        internal async Task InstantPlay(List<StorageFile> list)
        {
            await player.NewPlayList(list);
            player.Play();
        }

        internal async Task PlayNext(IList<Song> songs)
        {
            await player.AddtoNextPlay(songs);
            player.Play();
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

        internal void SkiptoItem(SongViewModel songViewModel)
        {
            player.SkiptoIndex(songViewModel.Index);
        }

        public double PositionToValue(TimeSpan t1, TimeSpan total)
        {
            if (total == null || total == default(TimeSpan))
            {
                return 0;
            }
            return 100 * (t1.TotalMilliseconds / total.TotalMilliseconds);
        }

        internal async Task<IList<Song>> ComingNewSongsAsync(List<StorageFile> list)
        {
            return await new FileReader().ReadFileandSendBack(list);
        }
    }


    class HamPanelItem : ViewModelBase
    {
        public string Title { get; set; }

        public Type TargetType { get; set; }

        public string Icon { get; set; }

        public Uri BG { get; set; }

        private bool isCurrent;
        public bool IsCurrent
        {
            get { return isCurrent; }
            set { SetProperty(ref isCurrent, value); }
        }

        public FontWeight ChangeWeight(bool b)
        {
            return b ? FontWeights.Bold : FontWeights.Normal;
        }

        public SolidColorBrush ChangeForeground(bool b)
        {
            return (SolidColorBrush)(b ? MainPage.Current.Resources["AccentForText"] : MainPage.Current.Resources["SystemControlForegroundBaseHighBrush"]);
        }

        public SolidColorBrush ChangeTextForeground(bool b)
        {
            return (SolidColorBrush)(b ? MainPage.Current.Resources["SystemControlForegroundBaseHighBrush"] : MainPage.Current.Resources["ButtonDisabledForegroundThemeBrush"]);
        }

        public double BoolToOpacity(bool b)
        {
            return b ? 1.0 : 0.333333333333;
        }
    }
}
