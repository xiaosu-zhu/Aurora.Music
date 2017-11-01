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

        private string genres;
        public string Genres
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
            var genres = (from alb in a
                          where !alb.Genres.IsNullorEmpty()
                          group alb by alb.Genres into grp
                          orderby grp.Count() descending
                          select grp.Key).First();
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                AlbumList = aList;
                SongsCount = aList.Count == 1 ? "1 Album" : $"{aList.Count} Albums";
                Genres = genres.Length > 0 ? string.Join(", ", genres) : "Various Genres";
                HeroImage = list.ConvertAll(x => (ImageSource)new BitmapImage(x));
            });
        }

        internal void GetArtwork()
        {
            var t = CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, () =>
            {
                foreach (var item in AlbumList)
                {
                    item.ToImage();
                };
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
