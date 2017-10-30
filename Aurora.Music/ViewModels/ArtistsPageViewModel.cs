using Aurora.Music.Core.Storage;
using Aurora.Shared.MVVM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;

namespace Aurora.Music.ViewModels
{
    class ArtistsPageViewModel : ViewModelBase
    {
        public ObservableCollection<ArtistViewModel> ArtistList { get; set; } = new ObservableCollection<ArtistViewModel>();

        public async Task GetArtists()
        {
            var opr = SQLOperator.Current();
            var artists = await opr.GetArtistsAsync();
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                foreach (var item in artists)
                {
                    ArtistList.Add(new ArtistViewModel
                    {
                        SongsCount = item.Count,
                        Name = item.AlbumArtists
                    });
                }
            });
        }
    }
}
