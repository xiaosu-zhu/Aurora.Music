using Aurora.Music.Core.Storage;
using Aurora.Shared.MVVM;
using System;
using System.Collections.ObjectModel;
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

        private string artistsCount;
        public string ArtistsCount
        {
            get { return artistsCount; }
            set { SetProperty(ref artistsCount, value); }
        }

        private string songsCount;
        public string SongsCount
        {
            get { return songsCount; }
            set { SetProperty(ref songsCount, value); }
        }

        public async Task GetArtists()
        {
            var opr = SQLOperator.Current();
            var artists = await opr.GetArtistsAsync();
            var list = new ObservableCollection<ArtistViewModel>();
            long sum = 0;
            foreach (var item in artists)
            {
                list.Add(new ArtistViewModel
                {
                    SongsCount = item.Count,
                    Name = item.AlbumArtists
                });
                sum += item.Count;
            }
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                ArtistList = list;
                ArtistsCount = list.Count == 1 ? "1 Artists" : $"{list.Count} Artists";
                SongsCount = sum == 1 ? "1 Song" : $"{sum} Songs";
            });
        }
    }
}
