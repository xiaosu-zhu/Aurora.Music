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
using Aurora.Music.Core.Player;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;
using Aurora.Music.Pages;
using Windows.UI.Text;

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
                IsCurrent = true
            },
            new HamPanelItem
            {
                Title = "Library",
                TargetType = typeof(LibraryPage)
            },
            new HamPanelItem
            {
                Title = "Playlist",
                TargetType = typeof(HomePage)
            },
        };

        private SQLOperator opr;
        private Player player;

        private bool needShowPanel;
        public bool NeedShowPanel
        {
            get { return needShowPanel; }
            set { SetProperty(ref needShowPanel, value); }
        }

        private BitmapImage currentArtwork;
        public BitmapImage CurrentArtwork
        {
            get { return currentArtwork; }
            set { SetProperty(ref currentArtwork, value); }
        }

        public MainPageViewModel()
        {
            opr = SQLOperator.Current();
            player = new Player();
            opr.NewSongsAdded += Opr_NewSongsAdded;
            Current = this;
        }

        public void Dispose()
        {
            ((IDisposable)player).Dispose();
        }

        public async Task Go()
        {
            StorageFolder folder = KnownFolders.MusicLibrary;
            await FileReader.Read(folder);
        }

        private async void Opr_NewSongsAdded(object sender, SongsAddedEventArgs e)
        {
            await FileReader.AddToAlbums(e.NewSongs);
        }

        internal async Task NewPlayList(IEnumerable<Song> songs)
        {
            await player.NewPlayList(songs);
        }

        public void Play()
        {
            player.Play();
        }
    }


    class HamPanelItem : ViewModelBase
    {
        public string Title { get; set; }

        public Type TargetType { get; set; }

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

        public Visibility ChangeVisibility(bool b)
        {
            return b ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
