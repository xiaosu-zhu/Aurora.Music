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

namespace Aurora.Music.ViewModels
{
    class AlbumsPageViewModel : ViewModelBase
    {
        public ObservableCollection<AlbumViewModel> AlbumList { get; set; } = new ObservableCollection<AlbumViewModel>();

        private BitmapImage heroImage = null;
        public BitmapImage HeroImage
        {
            get { return heroImage; }
            set { SetProperty(ref heroImage, value); }
        }

        public AlbumsPageViewModel()
        {

        }

        public async Task GetAlbums()
        {
            var albums = await FileReader.GetAlbumsAsync();
            foreach (var item in albums)
            {
                AlbumList.Add(new AlbumViewModel(item));
            }
        }
    }
}
