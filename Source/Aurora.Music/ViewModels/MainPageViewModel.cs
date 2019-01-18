// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

using AudioVisualizer;

using Aurora.Music.Controls;
using Aurora.Music.Core;
using Aurora.Music.Core.Models;
using Aurora.Music.Core.Storage;
using Aurora.Music.Core.Tools;
using Aurora.Music.Pages;
using Aurora.Music.PlaybackEngine;
using Aurora.Shared.Extensions;
using Aurora.Shared.Helpers;
using Aurora.Shared.MVVM;

using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.UserActivities;
using Windows.Foundation.Collections;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI.Shell;
using Windows.UI.StartScreen;
using Windows.UI.Text;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Aurora.Music.ViewModels
{
    class MainPageViewModel : ViewModelBase, IDisposable
    {
        private Windows.System.Display.DisplayRequest _displayRequest;
        private int _displayRequestCount = 0;

        public void ActivateDisplay()
        {
            //create the request instance if needed
            if (_displayRequest == null)
                _displayRequest = new Windows.System.Display.DisplayRequest();

            //make request to put in active state
            _displayRequest.RequestActive();
            _displayRequestCount++;
        }

        public void ReleaseDisplay()
        {
            //must be same instance, so quit if it doesn't exist
            if (_displayRequest == null)
                return;
            if (_displayRequestCount > 0)
            {
                //undo the request
                _displayRequest.RequestRelease();
                _displayRequestCount--;
            }
        }

        public string VolumeToSymbol(double d)
        {
            if (d.AlmostEqualTo(0))
            {
                return "\uE198";
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

        internal void PositionChange(TimeSpan timeSpan)
        {
            if (Math.Abs((timeSpan - CurrentPosition).TotalSeconds) < 1)
            {
                return;
            }
            player.Seek(timeSpan);
        }

        private double volume = Settings.Current.PlayerVolume;
        public double Volume
        {
            get { return volume; }
            set
            {
                SetProperty(ref volume, value);
                Settings.Current.PlayerVolume = value;
                player.ChangeVolume(value);
            }
        }

        public static MainPageViewModel Current;

        public List<HamPanelItem> HamList { get; set; } = new List<HamPanelItem>()
        {
            new HamPanelItem
            {
                Title = Consts.Localizer.GetString("HomeText"),
                TargetType = typeof(HomePage),
                Icon="\uE80F",
                Index = VirtualKey.Number1,
                IndexNum = "1"
            },
            new HamPanelItem
            {
                Title = Consts.Localizer.GetString("LibraryText"),
                Icon="\uE2AC",
                TargetType = typeof(LibraryPage),
                Index = VirtualKey.Number2,
                IndexNum = "2"
            },
            new HamPanelItem
            {
                Title = Consts.Localizer.GetString("DouText"),
                Icon = "\uEFA9",
                TargetType = typeof(DoubanPage),
                Index = VirtualKey.Number3,
                IndexNum = "3"
            },
            new HamPanelItem
            {
                Title = Consts.Localizer.GetString("PodcastMarket"),
                Icon="\uE774",
                TargetType = typeof(PodcastMarket),
                Index = VirtualKey.Number4,
                IndexNum = "4"
            },
        };

        public ObservableCollection<SongViewModel> NowPlayingList { get; set; } = new ObservableCollection<SongViewModel>();
        private IPlayer player;

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

        public bool NightModeEnabled { get; set; } = Settings.Current.NightMode;

        private BitmapImage currentArtwork = new BitmapImage(new Uri(Consts.BlackPlaceholder));
        public BitmapImage CurrentArtwork
        {
            get { return currentArtwork; }
            set { SetProperty(ref currentArtwork, value); }
        }

        private bool needShowBack;
        public bool NeedShowBack
        {
            get { return needShowBack; }
            set { SetProperty(ref needShowBack, value); }
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
            if (_lasttitle == null)
            {
                return;
            }
            NeedShowTitle = _lastneedshow;
            Title = _lasttitle;
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

        public string CurrentPlayingDesc()
        {
            if (currentTitle.IsNullorEmpty())
            {
                return Consts.Localizer.GetString("AppNameText");
            }
            else
            {
                return $"{currentTitle} - {CurrentAlbum} - {CurrentArtist}";
            }
        }

        private bool isPodcast;
        public bool IsPodcast
        {
            get { return isPodcast; }
            set { SetProperty(ref isPodcast, value); }
        }

        private string currentAlbum;
        public string CurrentAlbum
        {
            get { return currentAlbum.IsNullorEmpty() ? Consts.Localizer.GetString("NotPlayingText") : currentAlbum; }
            set { SetProperty(ref currentAlbum, value); }
        }

        private string currentArtist;
        public string CurrentArtist
        {
            get { return currentArtist; }
            set { SetProperty(ref currentArtist, value); }
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

        public Visibility TitlebarVisibility(bool b)
        {
            return (b && CoreApplication.GetCurrentView().TitleBar.IsVisible) ? Visibility.Visible : Visibility.Collapsed;
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
                new KeyValuePair<string, object>("id", id)
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
                new KeyValuePair<string, object>("id", id)
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
            if (!Settings.Current.DataPlayEnabled && Microsoft.Toolkit.Uwp.Connectivity.NetworkHelper.Instance.ConnectionInformation.IsInternetOnMeteredConnection)
            {
                return new AlbumInfo()
                {
                    AltArtwork = null,
                    Description = "Disabled fetching according to setting",
                    Artist = artist,
                    Name = album
                };
            }
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
            if (!Settings.Current.DataPlayEnabled && Microsoft.Toolkit.Uwp.Connectivity.NetworkHelper.Instance.ConnectionInformation.IsInternetOnMeteredConnection)
            {
                return new Core.Models.Artist()
                {
                    AvatarUri = null,
                    Description = "Disabled fetching according to setting",
                    Name = artist
                };
            }
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
                    _lasttitle = null;
                    MainPage.Current.GoBackFromNowPlaying();
                    MainPage.Current.Navigate(typeof(SettingsPage));
                });
            }
        }

        public DelegateCommand GotoDownload
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    _lasttitle = null;
                    MainPage.Current.GoBackFromNowPlaying();
                    MainPage.Current.Navigate(typeof(DownloadPage));
                });
            }
        }

        public DelegateCommand GotoAbout
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    _lasttitle = null;
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
                if (isShuffle == value) return;
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
            Current = this;
            player = PlaybackEngine.PlaybackEngine.Current;
            if (Settings.Current.LastUpdateBuild < SystemInfoHelper.GetPackageVersionNum())
            {
                if (Consts.UpdateNote == null)
                {
                    Settings.Current.LastUpdateBuild = SystemInfoHelper.GetPackageVersionNum();
                    Settings.Current.Save();
                }
                else
                {
                    var i = new UpdateInfo();
#pragma warning disable CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
                    i.ShowAsync();
#pragma warning restore CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
                    Settings.Current.LastUpdateBuild = SystemInfoHelper.GetPackageVersionNum();
                    Settings.Current.Save();
                }
            }
            Task.Run(() =>
            {
                AttachVisualizerSource();
            });
            Task.Run(() =>
            {
                player.DownloadProgressChanged += Player_DownloadProgressChanged;
                player.ItemsChanged += Player_StatusChanged;
                player.PlaybackStatusChanged += Player_PlaybackStatusChanged;
                player.PositionUpdated += Player_PositionUpdated;
                Downloader.Current.ProgressChanged += Current_ProgressChanged;
                Downloader.Current.ProgressCancelled += Current_ProgressChanged;
                Downloader.Current.ItemCompleted += Current_ProgressChanged;
                FileReader.ProgressUpdated += Reader_ProgressUpdated;
                FileReader.Completed += Reader_Completed;
            });
            Task.Run(async () =>
            {
                await ReloadExtensionsAsync();
            });
            Task.Run(async () =>
            {
                await FindFileChangesAsync();
            });
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.Shell.AdaptiveCardBuilder"))
            {
                Task.Run(async () =>
                {
                    await CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, async () =>
                    {
                        activity = UserActivityChannel.GetDefault();

                        var id = Guid.NewGuid().ToString();

                        act = await activity.GetOrCreateUserActivityAsync(id);

                        if (act.State == UserActivityState.Published)
                        {
                            await activity.DeleteActivityAsync(id);
                            act = await activity.GetOrCreateUserActivityAsync(id);
                        }
                    });
                });
            }
            if (JumpList.IsSupported())
            {
                Task.Run(async () =>
                {
                    var jumpList = await JumpListHelper.LoadJumpListAsync();
                    await jumpList.SaveAsync();
                });
            }
        }

        private int changing = 0;
        private async void FileTracker_FilesChanged(Windows.Storage.Search.IStorageQueryResultBase sender, object e)
        {
            changing++;
            var capture = changing;
            await Task.Delay(15000);
            if (capture != changing)
            {
                return;
            }
            await FilesChangedAsync();
        }

        private bool isdownloading;
        public bool IsDownloading
        {
            get { return isdownloading; }
            set { SetProperty(ref isdownloading, value); }
        }

        private double downloadPer;
        public double DownloadPer
        {
            get { return downloadPer; }
            set { SetProperty(ref downloadPer, value); }
        }

        private string downloadDes = Consts.Localizer.GetString("DownloadText");
        public string DownloadDes
        {
            get { return downloadDes; }
            set { SetProperty(ref downloadDes, value); }
        }

        private async void Current_ProgressChanged(object sender, (Windows.Networking.BackgroundTransfer.DownloadOperation, DownloadDesc) e)
        {
            int i = 0, all = 0;
            double down = 0, total = 0;
            foreach (var item in (sender as Downloader).GetAll())
            {
                if (item.Item1.Progress.Status == Windows.Networking.BackgroundTransfer.BackgroundTransferStatus.Running)
                {
                    i++;
                }
                down += item.Item1.Progress.BytesReceived;
                total += item.Item1.Progress.TotalBytesToReceive;
                all++;
            }
            await CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                if (i > 0)
                {
                    IsDownloading = true;
                    DownloadDes = SmartFormat.Smart.Format(Consts.Localizer.GetString("DownloadDesc"), i, all);
                }
                else
                {
                    IsDownloading = false;
                    DownloadDes = Consts.Localizer.GetString("DownloadText");
                }
                DownloadPer = total == 0 ? 0 : 100 * down / total;
            });
        }

        public void AttachVisualizerSource()
        {
            if (visualizerSource != null)
            {
                visualizerSource.SourceChanged -= VisualizerSource_SourceChanged;
                visualizerSource = null;
            }
            if (player is IPlayer p)
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
                IsPlaying = e.PlaybackStatus == MediaPlaybackState.Playing;
                isLoop = e.IsLoop;
                isShuffle = e.IsShuffle;
                RaisePropertyChanged("IsLoop");
                RaisePropertyChanged("IsShuffle");

                if (e.PlaybackStatus == MediaPlaybackState.Playing && Settings.Current.PreventLockscreen)
                {
                    ActivateDisplay();
                }
            });
        }

        public async Task ReloadExtensionsAsync()
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
        private object _currentActivity;
        private UserActivityChannel activity;
        private UserActivity act;
        private bool showed;

        public bool IsVisualizing
        {
            get => sVisualizing; set
            {
                sVisualizing = value;
                if (visualizerSource?.Source != null) visualizerSource.Source.IsSuspended = !value;
            }
        }

        public List<FileTracker> Trackers { get; private set; } = new List<FileTracker>();
        public Song CurrentSong { get; private set; }

        internal async Task SearchAsync(string text, AutoSuggestBoxTextChangedEventArgs args)
        {
            var dd = CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                MainPage.Current.ShowAutoSuggestPopup();
            });
            var tasks = new List<Task>();
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
                        await CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
                        {
                            if (args.CheckCurrent())
                                lock (MainPage.Current.Lockable)
                                {
                                    if (_lastQuery != text)
                                    {
                                        _lastQuery = text;
                                        SearchItems.Clear();
                                    }
                                    if (SearchItems.Count > 0 && SearchItems[0].InnerType == MediaType.Placeholder)
                                    {
                                        SearchItems.Clear();
                                    }
                                    foreach (var item in items)
                                    {
                                        SearchItems.Add(new GenericMusicItemViewModel(item)
                                        {
                                            IsSearch = true
                                        });
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
                    var notChanged = false;
                    await args.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
                    {
                        notChanged = args.CheckCurrent();
                    });
                    if (!notChanged)
                        return;

                    var podcasts = await SearchPodcastsAsync(text);

                    await CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
                    {
                        if (args.CheckCurrent() && !podcasts.IsNullorEmpty())
                            lock (MainPage.Current.Lockable)
                            {
                                if (_lastQuery != text)
                                {
                                    _lastQuery = text;
                                    SearchItems.Clear();
                                }
                                if (SearchItems.Count > 0 && SearchItems[0].InnerType == MediaType.Placeholder)
                                {
                                    SearchItems.Clear();
                                }
                                podcasts.Reverse();

                                foreach (var item in podcasts)
                                {
                                    SearchItems.Add(new GenericMusicItemViewModel(item)
                                    {
                                        IsSearch = true
                                    });
                                }
                            }
                    });
                }));
            tasks.Add(Task.Run(async () =>
            {
                var result = await FileReader.SearchAsync(text);

                await CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
                {
                    if (args.CheckCurrent() && !result.IsNullorEmpty())
                        lock (MainPage.Current.Lockable)
                        {
                            if (_lastQuery != text)
                            {
                                _lastQuery = text;
                                SearchItems.Clear();
                            }
                            if (SearchItems.Count > 0 && SearchItems[0].InnerType == MediaType.Placeholder)
                            {
                                SearchItems.Clear();
                            }
                            foreach (var item in result)
                            {
                                SearchItems.Add(new GenericMusicItemViewModel(item)
                                {
                                    IsSearch = true
                                });
                            }
                        }

                });
            }));
            await Task.WhenAll(tasks);
            await CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            { MainPage.Current.HideAutoSuggestPopup(); });
        }

        private async Task<List<OnlineMusicItem>> SearchPodcastsAsync(string text)
        {
            return await Podcast.SearchPodcasts(text);
        }

        public async Task FilesChangedAsync()
        {
            var foldersDB = await SQLOperator.Current().GetAllAsync<FOLDER>();
            var filtered = new List<string>();
            var folders = FileReader.InitFolderList();
            foreach (var fo in foldersDB)
            {
                var folder = await fo.GetFolderAsync();
                if (folders.Exists(a => a.Path == folder.Path))
                {
                    continue;
                }
                if (fo.IsFiltered)
                {
                    filtered.Add(folder.DisplayName);
                }
                else
                {
                    folders.Add(folder);
                }
            }
            try
            {
                folders.Remove(folders.Find(a => a.Path == ApplicationData.Current.LocalFolder.Path));
            }
            catch (Exception)
            {
            }

            Trackers.Clear();
            foreach (var item in folders)
            {
                Trackers.Add(new FileTracker(item, filtered));
            }

            var files = new List<StorageFile>();

            foreach (var item in Trackers)
            {
                files.AddRange(await item.SearchFolder());
            }

            var addedFiles = await FileTracker.FindChanges(files);

            if (!(addedFiles.Count == 0))
            {
                await FileReader.ReadFileandSaveAsync(addedFiles);
            }
        }

        private async Task FindFileChangesAsync()
        {
            var foldersDB = await SQLOperator.Current().GetAllAsync<FOLDER>();
            var filtered = new List<string>();
            var folders = FileReader.InitFolderList();
            foreach (var fo in foldersDB)
            {
                var folder = await fo.GetFolderAsync();
                if (folders.Exists(a => a.Path == folder.Path))
                {
                    continue;
                }
                if (fo.IsFiltered)
                {
                    filtered.Add(folder.DisplayName);
                }
                else
                {
                    folders.Add(folder);
                }
            }
            try
            {
                folders.Remove(folders.Find(a => a.Path == ApplicationData.Current.LocalFolder.Path));
            }
            catch (Exception)
            {
            }

            foreach (var item in folders)
            {
                Trackers.Add(new FileTracker(item, filtered));
            }

            var files = new List<StorageFile>();

            foreach (var item in Trackers)
            {
                files.AddRange(await item.SearchFolder());
            }

            var addedFiles = await FileTracker.FindChanges(files);

            if (!(addedFiles.Count == 0))
            {
                await FileReader.ReadFileandSaveAsync(addedFiles);
            }
        }

        internal void RestoreFromCompactOverlay()
        {
            player.RefreshNowPlayingInfo();
        }

        private void Reader_Completed(object sender, EventArgs e)
        {
            if (!showed)
            {
                return;
            }
            showed = false;
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
            if (!showed && e.CanHide)
                return;
            showed = true;
            var t = CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                MainPage.Current.ProgressUpdate();
                MainPage.Current.ProgressUpdate(e.Current * 100.0 / e.Total);
                MainPage.Current.ProgressUpdate(Consts.Localizer.GetString("FileUpdatingText"), e.Description);
            });
        }

        private async void Reader_NewSongsAdded(object sender, SongsAddedEventArgs e)
        {
            await FileReader.SortAlbumsAsync();
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
                    CurrentSong = null;
                    NowPlayingList.Clear();
                    NowListPreview = "-/-";
                    CurrentTitle = null;
                    CurrentAlbum = null;
                    CurrentArtist = null;
                    await CurrentArtwork.SetSourceAsync(await RandomAccessStreamReference.CreateFromUri(new Uri(Consts.BlackPlaceholder)).OpenReadAsync());
                    CurrentIndex = -1;
                    NeedShowPanel = false;
                    IsPodcast = false;
                    ReleaseDisplay();
                    return;
                }

                if (e.CurrentSong != null)
                {
                    CurrentSong = e.CurrentSong;
                    var p = e.CurrentSong;
                    CurrentTitle = p.Title.IsNullorEmpty() ? p.FilePath.Split('\\').LastOrDefault() : p.Title;
                    IsPodcast = p.IsPodcast;
                    CurrentAlbum = p.Album.IsNullorEmpty() ? Consts.UnknownAlbum : p.Album;
                    CurrentArtist = p.Performers == null ? (p.AlbumArtists == null ? Consts.UnknownArtists : string.Join(Consts.CommaSeparator, p.AlbumArtists)) : string.Join(Consts.CommaSeparator, p.Performers);

                    if (e.Thumnail != null)
                    {
                        await CurrentArtwork.SetSourceAsync(await e.Thumnail.OpenReadAsync());
                    }
                    else
                    {
                        var thumb = RandomAccessStreamReference.CreateFromUri(new Uri(Consts.BlackPlaceholder));
                        await CurrentArtwork.SetSourceAsync(await thumb.OpenReadAsync());
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
                ApplicationView.GetForCurrentView().Title = CurrentPlayingDesc();

                if (e.CurrentSong != null && Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.Shell.AdaptiveCardBuilder"))
                {
                    var last = NowPlayingList.Count - 1 <= currentIndex;

                    string img0, img1;
                    img1 = null;

                    if (!NowPlayingList[currentIndex].IsOnline)
                    {
                        //if (NowPlayingList[currentIndex].Song.PicturePath.IsNullorEmpty())
                        //{
                        //    img0 = Consts.BlackPlaceholder;
                        //}
                        //else
                        //{
                        //    img0 = $"ms-appdata:///temp/{NowPlayingList[currentIndex].Artwork.AbsoluteUri.Split('/').Last()}";
                        //}
                        img0 = null;
                    }
                    else
                    {
                        img0 = NowPlayingList[currentIndex].Artwork?.AbsoluteUri;
                    }

                    var otherArtwork = NowPlayingList.Where(a => a.Artwork?.AbsoluteUri != img0);

                    foreach (var item in otherArtwork)
                    {
                        if (!item.IsOnline)
                        {
                            img1 = null;
                        }
                        else
                        {
                            img1 = item.Artwork.AbsoluteUri;
                        }
                        break;
                    }

                    var json = await TimelineCard.AuthorAsync(currentTitle, currentAlbum, currentArtist, img0, img1, NowPlayingList.Count);

                    act.ActivationUri = new Uri("as-music:///?action=timeline-restore");

                    act.VisualElements.Content = AdaptiveCardBuilder.CreateAdaptiveCardFromJson(json);
                    act.VisualElements.DisplayText = Consts.Localizer.GetString("AppNameText");
                    act.VisualElements.Description = Consts.Localizer.GetString("TimelineTitle");
                    await act.SaveAsync();

                    var songs = NowPlayingList.Where(s => s.IsOnedrive || s.IsOnline).Select(s => s.Song).ToList();
#pragma warning disable CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
                    Task.Run(async () =>
                    {
                        if (songs.Count > 0)
                        {
                            var status = new PlayerStatus(songs, currentIndex, currentPosition);
                            await status.RoamingSaveAsync();
                        }
                        else
                        {
                            await PlayerStatus.ClearRoamingAsync();
                        }
                    });
#pragma warning restore CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法

                    await CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
                    {
                        try
                        {
                            //Dispose of any current UserActivitySession, and create a new one.
                            (_currentActivity as UserActivitySession)?.Dispose();
                            _currentActivity = act.CreateSession();
                        }
                        catch (Exception)
                        {
                        }
                    });
                }
            });
        }

        internal async Task SavePointAsync()
        {
            if (!NowPlayingList.IsNullorEmpty())
            {
                var songs = NowPlayingList.Select(s => s.Song).ToList();
                if (NowPlayingList.Count > currentIndex)
                {
                    var status = new PlayerStatus(songs, currentIndex, currentPosition);
                    await status.SaveAsync();
                }
            }
        }

        public void Dispose()
        {
            //player.StatusChanged -= Player_StatusChanged;
            //player.PositionUpdated -= Player_PositionUpdated;
        }

        internal async Task InstantPlayAsync(IList<Song> songs, int startIndex = 0)
        {
            if (songs.Any(a => a.IsOnline) && !Settings.Current.DataPlayEnabled && Microsoft.Toolkit.Uwp.Connectivity.NetworkHelper.Instance.ConnectionInformation.IsInternetOnMeteredConnection)
            {
                MainPage.Current.PopMessage("Filtered online songs according to setting");
                songs = songs.Where(a => !a.IsOnline).ToList();
            }
            Task.Run(async () =>
            {
                await player.NewPlayList(songs, startIndex);
                player.Play();
            });
        }

        internal async Task InstantPlayAsync(List<StorageFile> list, int startIndex = 0)
        {
            Task.Run(async () =>
            {
                await player.NewPlayList(list, startIndex);
                player.Play();
            });
        }

        internal async Task PlayNextAsync(IList<Song> songs)
        {
            if (songs.Any(a => a.IsOnline) && !Settings.Current.DataPlayEnabled && Microsoft.Toolkit.Uwp.Connectivity.NetworkHelper.Instance.ConnectionInformation.IsInternetOnMeteredConnection)
            {
                MainPage.Current.PopMessage("Filtered online songs according to setting");
                songs = songs.Where(a => !a.IsOnline).ToList();
            }
            Task.Run(async () =>
            {
                await player.AddtoNextPlay(songs);
                player.Play();
            });
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
            player.SkiptoIndex((int)songViewModel.Index);
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
            return await FileReader.ReadFileandSendBackAsync(list);
        }
    }


    class HamPanelItem : ViewModelBase
    {
        public VirtualKey Index { get; set; }
        public string IndexNum { get; set; }

        public string Title { get; set; }

        public Type TargetType { get; set; }

        public string Icon { get; set; }

        public Uri BG { get; set; }

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

        public double PaneLength(bool a)
        {
            return a ? 320d : 48d;
        }
    }
}
