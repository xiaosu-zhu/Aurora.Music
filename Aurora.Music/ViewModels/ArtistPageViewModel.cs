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
        public ObservableCollection<AlbumViewModel> AlbumList { get; set; } = new ObservableCollection<AlbumViewModel>();

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
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                var list = new List<ImageSource>();
                int i = 0;
                foreach (var item in a)
                {
                    AlbumList.Add(new AlbumViewModel(item));
                    if (i < 9)
                    {
                        if (item.PicturePath.IsNullorEmpty()) continue;
                        list.Add(new BitmapImage(new Uri(item.PicturePath)));
                        i++;
                    }
                }
                HeroImage = list;
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
