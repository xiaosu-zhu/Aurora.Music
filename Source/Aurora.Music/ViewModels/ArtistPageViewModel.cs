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
    class ArtistPageViewModel : ViewModelBase, IKey
    {
        private ObservableCollection<AlbumViewModel> albumList;
        public ObservableCollection<AlbumViewModel> AlbumList
        {
            get { return albumList; }
            set { SetProperty(ref albumList, value); }
        }

        private ObservableCollection<GroupedItem<SongViewModel>> songsList;
        public ObservableCollection<GroupedItem<SongViewModel>> SongsList
        {
            get { return songsList; }
            set { SetProperty(ref songsList, value); }
        }

        public bool NightModeEnabled { get; set; } = Settings.Current.NightMode;

        private ArtistViewModel artist;
        public ArtistViewModel Artist
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

        public string Key => Artist.Name;

        public ArtistPageViewModel()
        {
            AlbumList = new ObservableCollection<AlbumViewModel>();
            SongsList = new ObservableCollection<GroupedItem<SongViewModel>>();
        }

        public DelegateCommand PlayAll
        {
            get
            {
                return new DelegateCommand(async () =>
                {
                    await MainPageViewModel.Current.InstantPlayAsync(SongsList.SelectMany(a => a.Select(s => s.Song)).ToList());
                });
            }
        }

        internal async Task PlayAt(SongViewModel songViewModel)
        {
            var list = new List<Song>();
            foreach (var item in SongsList)
            {
                list.AddRange(item.Select(a => a.Song));
            }
            await MainPageViewModel.Current.InstantPlayAsync(list, list.FindIndex(x => x.ID == songViewModel.ID));
        }



        public async Task Init(ArtistViewModel artist)
        {
            var b = ThreadPool.RunAsync(async x =>
            {
                var art = await MainPageViewModel.Current.GetArtistInfoAsync(artist.RawName);
                if (art != null)
                    await CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
                    {
                        Artist.Description = art.Description;
                        Artist.Avatar = art.AvatarUri;
                    });
            });

            var opr = SQLOperator.Current();

            var albums = await opr.GetAlbumsOfArtistAsync(artist.RawName);
            var songs = await opr.GetSongsAsync(albums.SelectMany(s => s.Songs));
            var empty = artist.RawName.IsNullorEmpty();
            for (int i = songs.Count - 1; i >= 0; i--)
            {
                if (empty)
                {
                    if (songs[i].Performers == null || songs[i].Performers.Length == 0)
                    {
                        continue;
                    }
                    else
                    {
                        songs.RemoveAt(i);
                    }
                }
                else
                {
                    var flag = false;
                    if (songs[i].Performers != null)
                        foreach (var p in songs[i].Performers)
                        {
                            if (artist.RawName == p)
                            {
                                flag = true;
                                break;
                            }
                        }
                    if (!flag)
                        if (songs[i].AlbumArtists == null || songs[i].AlbumArtists.Length == 0)
                        {
                            songs.RemoveAt(i);
                            continue;
                        }
                        else
                        {
                            foreach (var p in songs[i].AlbumArtists)
                            {
                                if (artist.RawName == p)
                                {
                                    flag = true;
                                    break;
                                }
                            }
                        }
                    if (!flag)
                        songs.RemoveAt(i);
                }

            }
            var grouped = GroupedItem<SongViewModel>.CreateGroupsByAlpha(songs.ConvertAll(x => new SongViewModel(x)));

            var a = albums.OrderByDescending(x => x.Year);
            var genres = (from alb in a
                          where !alb.Genres.IsNullorEmpty()
                          group alb by alb.Genres into grp
                          orderby grp.Count() descending
                          select grp.Key).FirstOrDefault();
            await CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                AlbumList.Clear();
                foreach (var item in a)
                {
                    AlbumList.Add(new AlbumViewModel(item));
                }
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
                SongsCount = SmartFormat.Smart.Format(Consts.Localizer.GetString("SmartAlbums"), AlbumList.Count);
                Genres = genres.IsNullorEmpty() ? Consts.Localizer.GetString("VariousGenresText") : string.Join(Consts.CommaSeparator, genres);
            });
        }

        internal async Task PlayAlbumAsync(AlbumViewModel album)
        {
            var songs = await album.GetSongsAsync();
            await MainPageViewModel.Current.InstantPlayAsync(songs);
        }
    }
}
