using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aurora.Music.Core.Storage;
using Aurora.Shared.Helpers;
using Aurora.Shared.MVVM;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Media;
using Windows.ApplicationModel.Core;
using Aurora.Shared.Extensions;

namespace Aurora.Music.ViewModels
{
    class ArtistPageViewModel : ViewModelBase
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

        private string artist;
        public string Artist
        {
            get { return artist; }
            set { SetProperty(ref artist, value); }
        }

        public ArtistPageViewModel()
        {

        }

        public async Task GetAlbums(string artist)
        {
            var albums = await FileReader.GetAlbumsAsync("AlbumArtists", artist);
            var a = albums.OrderByDescending(x => x.Year);

            var list = new List<Uri>();
            var aList = new ObservableCollection<AlbumViewModel>();
            int i = 0;
            foreach (var item in a)
            {
                aList.Add(new AlbumViewModel(item));
                if (i < 9)
                {
                    if (item.PicturePath.IsNullorEmpty()) continue;
                    list.Add(new Uri(item.PicturePath));
                    i++;
                }
            }
            list.Shuffle();
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                Artist = artist;
                AlbumList = aList;
                HeroImage = list.ConvertAll(x => (ImageSource)new BitmapImage(x));
            });
        }

        internal async Task PlayAlbum(AlbumViewModel album)
        {
            var songs = await album.FindSongs();
            await MainPageViewModel.Current.NewPlayList(songs);
            MainPageViewModel.Current.PlayPause.Execute();
        }
    }
}
