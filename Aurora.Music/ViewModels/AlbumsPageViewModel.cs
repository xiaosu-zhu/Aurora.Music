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
        private ObservableCollection<AlbumViewModel> albumList;
        public ObservableCollection<AlbumViewModel> AlbumList
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
            AlbumList = new ObservableCollection<AlbumViewModel>();
        }

        public async Task GetAlbums()
        {
            var albums = await FileReader.GetAllAlbumsAsync();

            //var grouped = GroupedItem<AlbumViewModel>.CreateGroupsByAlpha(list);

            var b = ThreadPool.RunAsync(async x =>
            {
                var aa = albums.ToList();
                aa.Shuffle();
                var list = new List<Uri>();
                for (int j = 0; j < albums.Count && j < 9; j++)
                {
                    if (albums[j].PicturePath.IsNullorEmpty()) continue;
                    list.Add(new Uri(albums[j].PicturePath));
                }
                list.Shuffle();
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
                {
                    HeroImage = list.ConvertAll(y => (ImageSource)new BitmapImage(y));
                });
            });

            var a = albums.OrderByDescending(x => x.Year);
            var aCount = await FileReader.GetArtistsCountAsync();
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                AlbumList.Clear();
                foreach (var item in a)
                {
                    AlbumList.Add(new AlbumViewModel(item));
                }
                SongsCount = AlbumList.Count == 1 ? "1 Album" : $"{AlbumList.Count} Albums";
                ArtistsCount = aCount == 1 ? "1 Artist" : $"{aCount} Artists";
            });
        }

        internal async Task PlayAlbumAsync(AlbumViewModel album)
        {
            var songs = await album.GetSongsAsync();
            await MainPageViewModel.Current.InstantPlay(songs);
        }
    }
}
