using Aurora.Shared.MVVM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace Aurora.Music.ViewModels
{
    class AlbumDetailViewModel : ViewModelBase
    {
        private ObservableCollection<SongViewModel> songList;
        public ObservableCollection<SongViewModel> SongList
        {
            get { return songList; }
            set { SetProperty(ref songList, value); }
        }

        private AlbumViewModel album;
        public AlbumViewModel Album
        {
            get { return album; }
            set { SetProperty(ref album, value); }
        }

        private BitmapImage heroImage;
        public BitmapImage HeroImage
        {
            get { return heroImage; }
            set { SetProperty(ref heroImage, value); }
        }
    }
}
