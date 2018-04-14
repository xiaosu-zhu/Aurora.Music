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
    class SongsPageViewModel : ViewModelBase
    {

        private ObservableCollection<GroupedItem<SongViewModel>> songsList;
        public ObservableCollection<GroupedItem<SongViewModel>> SongsList
        {
            get { return songsList; }
            set { SetProperty(ref songsList, value); }
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
                    var list = new List<Song>();
                    foreach (var item in SongsList)
                    {
                        list.AddRange(item.Select(a => a.Song));
                    }
                    await MainPageViewModel.Current.InstantPlay(list);
                });
            }
        }

        public int SortIndex { get; internal set; } = 0;

        public async Task GetSongsAsync()
        {
            var songs = await FileReader.GetAllSongAsync();

            IEnumerable<GroupedItem<SongViewModel>> grouped;

            //var grouped = GroupedItem<AlbumViewModel>.CreateGroups(albums.ConvertAll(x => new AlbumViewModel(x)), x => x.GetFormattedArtists());

            //var grouped = GroupedItem<SongViewModel>.CreateGroups(songs.ConvertAll(x => new SongViewModel(x)), x => x.Year, true);

            switch (Settings.Current.SongsSort)
            {
                case SortMode.Alphabet:
                    grouped = GroupedItem<SongViewModel>.CreateGroupsByAlpha(songs.ConvertAll(x => new SongViewModel(x)));
                    SortIndex = 0;
                    break;
                case SortMode.Album:
                    grouped = GroupedItem<SongViewModel>.CreateGroups(songs.ConvertAll(x => new SongViewModel(x)), x => x.FormattedAlbum);
                    SortIndex = 1;
                    break;
                case SortMode.Artist:
                    grouped = GroupedItem<SongViewModel>.CreateGroups(songs.ConvertAll(x => new SongViewModel(x)), x => x.GetFormattedArtists());
                    SortIndex = 2;
                    break;
                case SortMode.Year:
                    grouped = GroupedItem<SongViewModel>.CreateGroups(songs.ConvertAll(x => new SongViewModel(x)), x => x.Song.Year);
                    SortIndex = 3;
                    break;
                default:
                    grouped = GroupedItem<SongViewModel>.CreateGroupsByAlpha(songs.ConvertAll(x => new SongViewModel(x)));
                    SortIndex = 0;
                    break;
            }

            var aCount = await FileReader.GetArtistsCountAsync();

            await CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
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
                SongsCount = SmartFormat.Smart.Format(Consts.Localizer.GetString("SmartSongs"), songs.Count);
                ArtistsCount = SmartFormat.Smart.Format(Consts.Localizer.GetString("SmartArtists"), aCount);
            });

            var b = ThreadPool.RunAsync(async x =>
            {
                var aa = (from s in songs group s by s.Album).ToList();
                aa.Shuffle();
                var list = new List<Uri>();
                for (int j = 0; j < aa.Count && list.Count < 6; j++)
                {
                    if (aa[j].FirstOrDefault().PicturePath.IsNullorEmpty())
                    {
                        continue;
                    }
                    list.Add(new Uri(aa[j].FirstOrDefault().PicturePath));
                }
                list.Shuffle();
                await CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
                {
                    HeroImage = list.ConvertAll(y => (ImageSource)new BitmapImage(y));
                    foreach (var item in SongsList)
                    {
                        foreach (var song in item)
                        {
                            song.RefreshFav();
                        }
                    }
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
            SongsList.Clear();
            var songs = await FileReader.GetAllSongAsync();
            IEnumerable<GroupedItem<SongViewModel>> grouped;

            switch (selectedIndex)
            {
                case 0:
                    grouped = GroupedItem<SongViewModel>.CreateGroupsByAlpha(songs.ConvertAll(x => new SongViewModel(x)));
                    Settings.Current.SongsSort = SortMode.Alphabet;
                    break;
                case 1:
                    grouped = GroupedItem<SongViewModel>.CreateGroups(songs.ConvertAll(x => new SongViewModel(x)), x => x.FormattedAlbum);
                    Settings.Current.SongsSort = SortMode.Album;
                    break;
                case 2:
                    grouped = GroupedItem<SongViewModel>.CreateGroups(songs.ConvertAll(x => new SongViewModel(x)), x => x.GetFormattedArtists());
                    Settings.Current.SongsSort = SortMode.Artist;
                    break;
                case 3:
                    grouped = GroupedItem<SongViewModel>.CreateGroups(songs.ConvertAll(x => new SongViewModel(x)), x => x.Song.Year);
                    Settings.Current.SongsSort = SortMode.Year;
                    break;
                default:
                    grouped = GroupedItem<SongViewModel>.CreateGroupsByAlpha(songs.ConvertAll(x => new SongViewModel(x)));
                    Settings.Current.SongsSort = SortMode.Alphabet;
                    break;
            }
            SortIndex = selectedIndex;
            Settings.Current.Save();

            foreach (var item in grouped)
            {
                item.Aggregate((x, y) =>
                {
                    y.Index = x.Index + 1;
                    return y;
                });
                SongsList.Add(item);
            }
            foreach (var item in SongsList)
            {
                foreach (var song in item)
                {
                    song.RefreshFav();
                }
            }

        }

        internal async Task PlayAt(SongViewModel songViewModel)
        {
            var list = new List<Song>();
            foreach (var item in SongsList)
            {
                list.AddRange(item.Select(a => a.Song));
            }
            await MainPageViewModel.Current.InstantPlay(list, list.FindIndex(x => x.ID == songViewModel.ID));
        }
    }
}
