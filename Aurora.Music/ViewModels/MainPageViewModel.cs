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

namespace Aurora.Music.ViewModels
{
    class MainPageViewModel : ViewModelBase, IDisposable
    {
        public static MainPageViewModel Current;

        private SQLOperator opr;
        private Player player;

        private bool needShowPanel;
        public bool NeedShowPanel
        {
            get { return needShowPanel; }
            set { SetProperty(ref needShowPanel, value); }
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

        internal async Task NewPlayList(IEnumerable<Core.Storage.Song> songs)
        {
            await player.NewPlayList(songs);
        }

        public void Play()
        {
            player.Play();
        }
    }
}
