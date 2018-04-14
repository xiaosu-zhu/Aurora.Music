// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Aurora.Music.Core;
using Aurora.Music.Core.Models;
using Aurora.Music.Core.Storage;
using Aurora.Shared.Extensions;
using Aurora.Shared.MVVM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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

        public int SortIndex { get; internal set; } = 0;

        public async Task GetAlbums()
        {
            var albums = await FileReader.GetAllAlbumsAsync();

            //var grouped = GroupedItem<AlbumViewModel>.CreateGroupsByAlpha(albums.ConvertAll(x => new AlbumViewModel(x)));

            //var grouped = GroupedItem<AlbumViewModel>.CreateGroups(albums.ConvertAll(x => new AlbumViewModel(x)), x => x.GetFormattedArtists());
            IEnumerable<GroupedItem<AlbumViewModel>> grouped;

            switch (Settings.Current.AlbumsSort)
            {
                case SortMode.Alphabet:
                    grouped = GroupedItem<AlbumViewModel>.CreateGroupsByAlpha(albums.ConvertAll(x => new AlbumViewModel(x)));
                    SortIndex = 1;
                    break;
                case SortMode.Year:
                    grouped = GroupedItem<AlbumViewModel>.CreateGroups(albums.ConvertAll(x => new AlbumViewModel(x)), x => x.Year, true);
                    SortIndex = 0;
                    break;
                case SortMode.Artist:
                    grouped = GroupedItem<AlbumViewModel>.CreateGroups(albums.ConvertAll(x => new AlbumViewModel(x)), x => x.GetFormattedArtists());
                    SortIndex = 2;
                    break;
                default:
                    grouped = GroupedItem<AlbumViewModel>.CreateGroups(albums.ConvertAll(x => new AlbumViewModel(x)), x => x.Year, true);
                    SortIndex = 0;
                    break;
            }

            var aCount = await FileReader.GetArtistsCountAsync();

            await CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                AlbumList.Clear();
                foreach (var item in grouped)
                {
                    AlbumList.Add(item);
                }
                SongsCount = SmartFormat.Smart.Format(Consts.Localizer.GetString("SmartAlbums"), albums.Count);
                ArtistsCount = SmartFormat.Smart.Format(Consts.Localizer.GetString("SmartArtists"), aCount);
            });

            var b = ThreadPool.RunAsync(async x =>
            {
                var aa = albums.ToList();
                aa.Shuffle();
                var list = new List<Uri>();
                for (int j = 0; j < aa.Count && list.Count < 6; j++)
                {
                    if (aa[j].PicturePath.IsNullorEmpty()) continue;
                    list.Add(new Uri(aa[j].PicturePath));
                }
                list.Shuffle();
                await CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
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

        internal async void ChangeSort(int selectedIndex)
        {
            AlbumList.Clear();
            var albums = await FileReader.GetAllAlbumsAsync();
            IEnumerable<GroupedItem<AlbumViewModel>> grouped;
            switch (selectedIndex)
            {
                case 0:
                    grouped = GroupedItem<AlbumViewModel>.CreateGroups(albums.ConvertAll(x => new AlbumViewModel(x)), x => x.Year, true);
                    Settings.Current.AlbumsSort = SortMode.Year;
                    break;
                case 1:
                    grouped = GroupedItem<AlbumViewModel>.CreateGroupsByAlpha(albums.ConvertAll(x => new AlbumViewModel(x)));
                    Settings.Current.AlbumsSort = SortMode.Alphabet;
                    break;
                case 2:
                    grouped = GroupedItem<AlbumViewModel>.CreateGroups(albums.ConvertAll(x => new AlbumViewModel(x)), x => x.GetFormattedArtists());
                    Settings.Current.AlbumsSort = SortMode.Artist;
                    break;
                default:
                    grouped = GroupedItem<AlbumViewModel>.CreateGroups(albums.ConvertAll(x => new AlbumViewModel(x)), x => x.Year, true);
                    Settings.Current.AlbumsSort = SortMode.Year;
                    break;
            }
            SortIndex = selectedIndex;
            Settings.Current.Save();
            foreach (var item in grouped)
            {
                AlbumList.Add(item);
            }

        }
    }
}
