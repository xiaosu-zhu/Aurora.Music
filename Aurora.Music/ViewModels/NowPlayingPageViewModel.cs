using Aurora.Music.Core;
using Aurora.Music.Core.Player;
using Aurora.Music.Core.Storage;
using Aurora.Shared.MVVM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.ApplicationModel.Core;
using Windows.Media.Playback;
using Windows.UI.Xaml.Media.Imaging;

namespace Aurora.Music.ViewModels
{
    class NowPlayingPageViewModel : ViewModelBase
    {
        private BitmapImage artwork;
        public BitmapImage CurrentArtwork
        {
            get { return artwork; }
            set { SetProperty(ref artwork, value); }
        }

        private Player player;

        private string currentTitle;
        public string CurrentTitle
        {
            get { return currentTitle; }
            set { SetProperty(ref currentTitle, value); }
        }

        private SongViewModel song;
        public SongViewModel Song
        {
            get { return song; }
            set { SetProperty(ref song, value); }
        }

        public void Init(SongViewModel song)
        {
            Song = song;
            CurrentArtwork = new BitmapImage(new Uri(song.PicturePath));
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

        private string lastUriPath;

        private bool? isPlaying;
        public bool? IsPlaying
        {
            get { return isPlaying; }
            set { SetProperty(ref isPlaying, value); }
        }

        private string currentAlbum;
        public string CurrentAlbum
        {
            get { return currentAlbum; }
            set { SetProperty(ref currentAlbum, value); }
        }

        private int currentIndex;
        public int CurrentIndex
        {
            get { return currentIndex; }
            set { SetProperty(ref currentIndex, value); }
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
                    Song = new SongViewModel(e.CurrentSong.Source.CustomProperties[Consts.SONG] as SONG);
                    if (e.CurrentSong.Source.CustomProperties["Artwork"] is Uri u)
                    {
                        if (lastUriPath == u.AbsolutePath)
                        {

                        }
                        else
                        {
                            CurrentArtwork = new BitmapImage(u);
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
                            NowPlayingList.Add(new SongViewModel(item.Source.CustomProperties[Consts.SONG] as SONG)
                            {
                                Index = i++
                            });
                        }
                        CurrentIndex = Convert.ToInt32(e.CurrentIndex);
                    }
                }
            });
        }
    }
}
