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

namespace Aurora.Music.ViewModels
{
    class MainPageViewModel : ViewModelBase
    {
        private SQLOperator opr = SQLOperator.Current();
        public async Task Go()
        {
            opr.NewSongsAdded += Opr_NewSongsAdded;
            StorageFolder folder = KnownFolders.MusicLibrary;
            await FileReader.Read(folder);
        }

        private async void Opr_NewSongsAdded(object sender, SongsAddedEventArgs e)
        {
            await FileReader.AddToAlbums(e.NewSongs);
        }

        internal async void DebugPrint()
        {
            var songs = await FileReader.GetSongsAsync();
            var i = 0;
            foreach (var item in songs)
            {
                if (i % 6 == 0)
                {
                    Debug.WriteLine(item.Title);
                }
                i++;
            }
            var albums = await FileReader.GetAlbumsAsync();
            foreach (var item in albums)
            {
                Debug.WriteLine(item.Name);
            }
        }
    }
}
