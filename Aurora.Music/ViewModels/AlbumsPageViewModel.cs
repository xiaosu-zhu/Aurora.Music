using Aurora.Music.Core.Storage;
using Aurora.Shared.Extensions;
using Aurora.Shared.MVVM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.System.Threading;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Aurora.Music.ViewModels
{
    class AlbumsPageViewModel : ViewModelBase
    {
        private ObservableCollection<GroupedItem<AlbumViewModel>> albumList;
        public ObservableCollection<GroupedItem<AlbumViewModel>> AlbumList
        {
            get { return albumList; }
            set { SetProperty(ref albumList, value); }
        }

        private List<ImageSource> heroImage = null;
        public List<ImageSource> HeroImage
        {
            get { return heroImage; }
            set { SetProperty(ref heroImage, value); }
        }

        private string genres;
        public string ArtistsCount
        {
            get { return genres; }
            set { SetProperty(ref genres, value); }
        }

        private string songsCount;
        public string SongsCount
        {
            get { return songsCount; }
            set { SetProperty(ref songsCount, value); }
        }

        public AlbumsPageViewModel()
        {
            AlbumList = new ObservableCollection<GroupedItem<AlbumViewModel>>();
        }

        public DelegateCommand PlayAll
        {
            get
            {
                return new DelegateCommand(async () =>
                {
                    await MainPageViewModel.Current.InstantPlay(await FileReader.GetAllSongAsync());
                });
            }
        }

        public async Task GetAlbums()
        {
            var albums = await FileReader.GetAllAlbumsAsync();

            //var grouped = GroupedItem<AlbumViewModel>.CreateGroupsByAlpha(albums.ConvertAll(x => new AlbumViewModel(x)));

            //var grouped = GroupedItem<AlbumViewModel>.CreateGroups(albums.ConvertAll(x => new AlbumViewModel(x)), x => x.GetFormattedArtists());

            var grouped = GroupedItem<AlbumViewModel>.CreateGroups(albums.ConvertAll(x => new AlbumViewModel(x)), x => x.Year, true);

            var aCount = await FileReader.GetArtistsCountAsync();

            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                AlbumList.Clear();
                foreach (var item in grouped)
                {
                    AlbumList.Add(item);
                }
                SongsCount = albums.Count == 1 ? "1 Album" : $"{albums.Count} Albums";
                ArtistsCount = aCount == 1 ? "1 Artist" : $"{aCount} Artists";
            });

            var b = ThreadPool.RunAsync(async x =>
            {
                var aa = albums.ToList();
                aa.Shuffle();
                var list = new List<Uri>();
                for (int j = 0; j < aa.Count && list.Count < 9; j++)
                {
                    if (aa[j].PicturePath.IsNullorEmpty()) continue;
                    list.Add(new Uri(aa[j].PicturePath));
                }
                list.Shuffle();
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
                {
                    HeroImage = list.ConvertAll(y => (ImageSource)new BitmapImage(y));
                });
            });
        }

        internal async Task PlayAlbumAsync(AlbumViewModel album)
        {
            var songs = await album.GetSongsAsync();
            await MainPageViewModel.Current.InstantPlay(songs);
        }

        internal void ChangeSort(int selectedIndex)
        {
            AlbumList.Clear();
            var t = ThreadPool.RunAsync(async y =>
           {
               var albums = await FileReader.GetAllAlbumsAsync();
               IEnumerable<GroupedItem<AlbumViewModel>> grouped;

               switch (selectedIndex)
               {
                   case 0:
                       grouped = GroupedItem<AlbumViewModel>.CreateGroups(albums.ConvertAll(x => new AlbumViewModel(x)), x => x.Year, true);
                       break;
                   case 1:
                       grouped = GroupedItem<AlbumViewModel>.CreateGroupsByAlpha(albums.ConvertAll(x => new AlbumViewModel(x)));
                       break;
                   case 2:
                       grouped = GroupedItem<AlbumViewModel>.CreateGroups(albums.ConvertAll(x => new AlbumViewModel(x)), x => x.GetFormattedArtists());
                       break;
                   default:
                       grouped = GroupedItem<AlbumViewModel>.CreateGroups(albums.ConvertAll(x => new AlbumViewModel(x)), x => x.Year, true);
                       break;
               }
               await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
               {
                   foreach (var item in grouped)
                   {
                       AlbumList.Add(item);
                   }
               });
           });

        }
    }
}
