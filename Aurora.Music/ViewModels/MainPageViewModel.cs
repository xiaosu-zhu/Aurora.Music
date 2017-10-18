using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aurora.Music.Core.Storage;
using Aurora.Shared.Helpers;
using Aurora.Shared.MVVM;
using Windows.Storage;

namespace Aurora.Music.ViewModels
{
    class MainPageViewModel : ViewModelBase
    {
        private SQLOperator opr = AsyncHelper.RunSync(async () => await SQLOperator.CurrentAsync());
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
    }
}
