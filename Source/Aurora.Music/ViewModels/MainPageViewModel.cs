using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aurora.Music.Core.Storage;
using Aurora.Shared.Helpers;
using Aurora.Shared.MVVM;
using Windows.Storage;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;
using Aurora.Music.Pages;
using Windows.UI.Text;
using Windows.ApplicationModel.Core;
using Windows.UI.Xaml.Media;
using Windows.Media.Playback;
using System.Collections.ObjectModel;
using Aurora.Music.Core;
using Windows.System.Threading;
using Aurora.Shared.Extensions;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls;
using Aurora.Music.Core.Models;
using Aurora.Music.PlaybackEngine;

namespace Aurora.Music.ViewModels
{
    class MainPageViewModel : ViewModelBase, IDisposable
    {
        public static MainPageViewModel Current;

        public List<HamPanelItem> HamList { get; set; } = new List<HamPanelItem>()
        {
            new HamPanelItem
            {
                Title = "Home",
                TargetType = typeof(HomePage),
                Icon="\uE80F",
                IsCurrent = true,
                BG = new Uri("ms-appx:///Assets/Images/albums.png")
            },
            new HamPanelItem
            {
                Title = "Library",
                Icon="\uE2AC",
                TargetType = typeof(LibraryPage),
                BG = new Uri("ms-appx:///Assets/Images/artists.png")
            },
            new HamPanelItem
            {
                Title = "Playlist",
                Icon="\uE955",
                TargetType = typeof(HomePage),
                BG = new Uri("ms-appx:///Assets/Images/songs.png")
            },
        };

        public ObservableCollection<SongViewModel> NowPlayingList { get; set; } = new ObservableCollection<SongViewModel>();

        private Settings settings;
        private IPlayer player;

        private bool _lastLeftTop;
        private bool isLeftTopDark;
        public bool IsLeftTopForeWhite
        {
            get { return isLeftTopDark; }
            set
            {
                _lastLeftTop = isLeftTopDark;
                SetProperty(ref isLeftTopDark, value);
                ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
                titleBar.ForegroundColor = value ? Colors.Black : Colors.White;
            }
        }

        public Extension LyricExtension;
        public Extension OnlineMusicExtension;

        private bool needShowPanel = true;
        public bool NeedShowPanel
        {
            get { return needShowPanel; }
            set { SetProperty(ref needShowPanel, value); }
        }

        private Uri currentArtwork;
        public Uri CurrentArtwork
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
            IsLeftTopForeWhite = _lastLeftTop;
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
            get { return currentTitle.IsNullorEmpty() ? "Aurora Music" : currentTitle; }
            set { SetProperty(ref currentTitle, value); }
        }

        private string currentAlbum;
        private string lastUriPath;

        public string CurrentAlbum
        {
            get { return currentAlbum.IsNullorEmpty() ? "Not playing" : currentAlbum; }
            set { SetProperty(ref currentAlbum, value); }
        }

        private string nowListPreview = "-/-";

        internal void FastForward(bool v)
        {
            throw new NotImplementedException();
        }

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
            var querys = new List<KeyValuePair<string, object>>()
                {
                    new KeyValuePair<string,object>("q", "online_music"),
                    new KeyValuePair<string, object>("action", "song"),
                    new KeyValuePair<string, object>("id", id),
                    new KeyValuePair<string, object>("bit_rate", settings.GetPreferredBitRate())
                };
            var songResult = await OnlineMusicExtension.ExecuteAsync(querys.ToArray());
            if (songResult is Song s)
            {
                return s;
            }
            return null;
        }

        internal async Task<Album> GetOnlineAlbumAsync(string id)
        {
            var querys = new List<KeyValuePair<string, object>>()
                {
                    new KeyValuePair<string,object>("q", "online_music"),
                    new KeyValuePair<string, object>("action", "album"),
                    new KeyValuePair<string, object>("id", id),
                    new KeyValuePair<string, object>("bit_rate", settings.GetPreferredBitRate())
                };
            var songResult = await OnlineMusicExtension.ExecuteAsync(querys.ToArray());
            if (songResult is Album s)
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

        public DelegateCommand GotoSettings
        {
            get
            {
                return new DelegateCommand(() =>
                {
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
            settings = Settings.Load();
            player = Player.Current;
            Current = this;
            player.DownloadProgressChanged += Player_DownloadProgressChanged;
            player.StatusChanged += Player_StatusChanged;
            player.PositionUpdated += Player_PositionUpdated;
            var t = ThreadPool.RunAsync(async x =>
            {
                var exts = await Extension.Load(Settings.Load().LyricExtensionID);
                foreach (var ext in exts)
                {
                    if (ext is LyricExtension)
                    {
                        LyricExtension = ext;
                        break;
                    }
                }
                exts = await Extension.Load(Settings.Load().OnlineMusicExtensionID);
                foreach (var ext in exts)
                {
                    if (ext is OnlineMusicExtension)
                    {
                        OnlineMusicExtension = ext;
                        break;
                    }
                }
                await FindFileChanges();
            });
        }

        private double downloadProgress;
        private string _lastQuery;

        public double DownloadProgress
        {
            get { return downloadProgress; }
            set { SetProperty(ref downloadProgress, value); }
        }

        private async void Player_DownloadProgressChanged(object sender, DownloadProgressChangedArgs e)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                DownloadProgress = 100 * e.Progress;
            });
        }

        public ObservableCollection<GenericMusicItemViewModel> SearchItems { get; set; } = new ObservableCollection<GenericMusicItemViewModel>();

        internal async Task Search(string text)
        {
            if (OnlineMusicExtension != null)
            {
                _lastQuery = string.Copy(text);
                var s = string.Copy(_lastQuery);

                var job = ThreadPool.RunAsync(async work =>
                {
                    var querys = new List<KeyValuePair<string, object>>()
                    {
                        new KeyValuePair<string,object>("q", "online_music"),
                        new KeyValuePair<string, object>("action", "search"),
                        new KeyValuePair<string, object>("keyword", text)
                    };
                    var dd = CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, async () =>
                    {
                        await Task.Delay(200);
                        MainPage.Current.ShowAutoSuggestPopup();
                    });
                    var webResult = await OnlineMusicExtension.ExecuteAsync(querys.ToArray());
                    if (webResult is IEnumerable<OnlineMusicItem> items)
                    {
                        if (MainPage.Current.CanAdd && s == _lastQuery)
                            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
                            {
                                lock (MainPage.Current.Lockable)
                                {
                                    if (SearchItems.Count > 0 && SearchItems[0].Title.IsNullorEmpty())
                                    {
                                        SearchItems.Clear();
                                    }
                                    foreach (var item in items.Reverse())
                                    {
                                        SearchItems.Insert(0, new GenericMusicItemViewModel(item));
                                    }
                                    MainPage.Current.HideAutoSuggestPopup();
                                }
                            });
                    }
                });
            }

            var result = await FileReader.Search(text);

            if (MainPage.Current.CanAdd && !result.IsNullorEmpty())
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
                {
                    lock (MainPage.Current.Lockable)
                    {
                        if (SearchItems.Count > 0 && SearchItems[0].Title.IsNullorEmpty())
                        {
                            SearchItems.Clear();
                        }
                        foreach (var item in result)
                        {
                            SearchItems.Add(new GenericMusicItemViewModel(item));
                        }
                    }
                    MainPage.Current.HideAutoSuggestPopup();
                });
        }

        private async Task FindFileChanges()
        {
            var addedFiles = await FileTracker.FindChanges();
            if (!addedFiles.IsNullorEmpty())
            {
                var reader = new FileReader();
                await reader.ReadFileandSave(addedFiles);
            }
        }

        private async void Reader_NewSongsAdded(object sender, SongsAddedEventArgs e)
        {
            await new FileReader().AddToAlbums(e.NewSongs);
        }

        private async void Player_PositionUpdated(object sender, PositionUpdatedArgs e)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                CurrentPosition = e.Current;
                TotalDuration = e.Total;
            });
        }

        private async void Player_StatusChanged(object sender, StatusChangedArgs e)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                IsPlaying = player.IsPlaying;
                if (e.CurrentSong != null)
                {
                    var p = e.CurrentSong;
                    CurrentTitle = p.Title.IsNullorEmpty() ? p.FilePath.Split('\\').LastOrDefault() : p.Title;
                    CurrentAlbum = p.Album.IsNullorEmpty() ? (p.Performers.IsNullorEmpty() ? "Unknown Album" : string.Join(", ", p.Performers)) : p.Album;
                    if (!p.PicturePath.IsNullorEmpty())
                    {
                        if (lastUriPath == p.PicturePath)
                        {

                        }
                        else
                        {
                            CurrentArtwork = new Uri(p.PicturePath);
                            lastUriPath = p.PicturePath;
                        }
                    }
                    else
                    {
                        CurrentArtwork = null;
                    }
                }
                if (e.Items is IList<Song> l)
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
            });
        }

        public void Dispose()
        {
            //player.StatusChanged -= Player_StatusChanged;
            //player.PositionUpdated -= Player_PositionUpdated;
        }

        internal async Task InstantPlay(IList<Song> songs, int startIndex = 0)
        {
            await player.NewPlayList(songs, startIndex);
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

        internal void SkiptoItem(SongViewModel songViewModel)
        {
            player.SkiptoItem(songViewModel.Index);
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
            return (SolidColorBrush)(b ? MainPage.Current.Resources["SystemControlBackgroundAccentBrush"] : MainPage.Current.Resources["SystemControlForegroundBaseHighBrush"]);
        }
    }
}
