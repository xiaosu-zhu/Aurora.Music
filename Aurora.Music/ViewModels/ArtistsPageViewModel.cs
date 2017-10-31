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
        private ObservableCollection<ArtistViewModel> aritstList;

        public ObservableCollection<ArtistViewModel> ArtistList
        {
            get { return aritstList; }
            set { SetProperty(ref aritstList, value); }
        }

        public async Task GetArtists()
        {
            var opr = SQLOperator.Current();
            var artists = await opr.GetArtistsAsync();
            var list = new ObservableCollection<ArtistViewModel>();
            foreach (var item in artists)
            {
                list.Add(new ArtistViewModel
                {
                    SongsCount = item.Count,
                    Name = item.AlbumArtists
                });
            }
            await Task.Delay(160);
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                ArtistList = list;
            });
        }
    }
}
