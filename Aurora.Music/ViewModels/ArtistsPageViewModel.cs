using Aurora.Music.Core.Storage;
using Aurora.Shared.MVVM;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;

namespace Aurora.Music.ViewModels
{
    class ArtistsPageViewModel : ViewModelBase
    {
        private ObservableCollection<GroupedItem<ArtistViewModel>> aritstList;

        public ObservableCollection<GroupedItem<ArtistViewModel>> ArtistList
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

        public ArtistsPageViewModel()
        {
            ArtistList = new ObservableCollection<GroupedItem<ArtistViewModel>>();
        }

        public async Task GetArtists()
        {
            var opr = SQLOperator.Current();
            var artists = await opr.GetArtistsAsync();
            var grouped = GroupedItem<Artist>.CreateGroupsByAlpha(artists);

            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                ArtistList.Clear();

                long sum = 0;
                foreach (var item in grouped)
                {
                    ArtistList.Add(new GroupedItem<ArtistViewModel>(item.Key, item.Select(i => new ArtistViewModel
                    {
                        Name = i.AlbumArtists,
                        SongsCount = i.Count
                    })));
                    sum += item.Select(x => x.Count).Sum();
                }
                ArtistsCount = ArtistList.Count == 1 ? "1 Artists" : $"{ArtistList.Count} Artists";
                SongsCount = sum == 1 ? "1 Song" : $"{sum} Songs";
            });
        }
    }
}
