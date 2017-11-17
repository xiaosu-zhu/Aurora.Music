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
    class SongsPageViewModel : ViewModelBase
    {

        private ObservableCollection<GroupedItem<SongViewModel>> albumList;
        public ObservableCollection<GroupedItem<SongViewModel>> SongsList
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

        public SongsPageViewModel()
        {
            SongsList = new ObservableCollection<GroupedItem<SongViewModel>>();
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

        public async Task GetSongsAsync()
        {
            var songs = await FileReader.GetAllSongAsync();

            var grouped = GroupedItem<SongViewModel>.CreateGroupsByAlpha(songs.ConvertAll(x => new SongViewModel(x)));

            //var grouped = GroupedItem<AlbumViewModel>.CreateGroups(albums.ConvertAll(x => new AlbumViewModel(x)), x => x.GetFormattedArtists());

            //var grouped = GroupedItem<SongViewModel>.CreateGroups(songs.ConvertAll(x => new SongViewModel(x)), x => x.Year, true);

            var aCount = await FileReader.GetArtistsCountAsync();

            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                SongsList.Clear();
                foreach (var item in grouped)
                {
                    item.Aggregate((x, y) =>
                    {
                        y.Index = x.Index + 1;
                        return y;
                    });
                    SongsList.Add(item);
                }
                SongsCount = songs.Count == 1 ? "1 Song" : $"{songs.Count} Songs";
                ArtistsCount = aCount == 1 ? "1 Artist" : $"{aCount} Artists";
            });

            var b = ThreadPool.RunAsync(async x =>
            {
                var aa = songs.ToList();
                aa.Shuffle();
                var list = new List<Uri>();
                for (int j = 0; j < songs.Count && j < 9; j++)
                {
                    if (songs[j].PicturePath.IsNullorEmpty()) continue;
                    list.Add(new Uri(songs[j].PicturePath));
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
            SongsList.Clear();
            var t = ThreadPool.RunAsync(async tow =>
            {
                var songs = await FileReader.GetAllSongAsync();
                IEnumerable<GroupedItem<SongViewModel>> grouped;

                switch (selectedIndex)
                {
                    case 0:
                        grouped = GroupedItem<SongViewModel>.CreateGroupsByAlpha(songs.ConvertAll(x => new SongViewModel(x)));
                        break;
                    case 1:
                        grouped = GroupedItem<SongViewModel>.CreateGroups(songs.ConvertAll(x => new SongViewModel(x)), x => x.FormattedAlbum);
                        break;
                    case 2:
                        grouped = GroupedItem<SongViewModel>.CreateGroups(songs.ConvertAll(x => new SongViewModel(x)), x => x.GetFormattedArtists());
                        break;
                    default:
                        grouped = GroupedItem<SongViewModel>.CreateGroups(songs.ConvertAll(x => new SongViewModel(x)), x => x.Year, true);
                        break;
                }
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
                {
                    foreach (var item in grouped)
                    {
                        item.Aggregate((x, y) =>
                        {
                            y.Index = x.Index + 1;
                            return y;
                        });
                        SongsList.Add(item);
                    }
                });
            });

        }

        internal async Task PlayAt(SongViewModel songViewModel)
        {
            var list = await FileReader.GetAllSongAsync();
            await MainPageViewModel.Current.InstantPlay(list, list.FindIndex(x => x.ID == songViewModel.ID));
        }
    }
}
