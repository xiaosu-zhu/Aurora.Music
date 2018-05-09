// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Aurora.Music.Core;
using Aurora.Music.Core.Models;
using Aurora.Music.Core.Storage;
using Aurora.Shared.MVVM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.StartScreen;

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
                    await MainPageViewModel.Current.InstantPlayAsync(list);
                });
            }
        }

        public int SortIndex { get; internal set; } = 0;

        public async Task InitAsync()
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
            var favors = await SQLOperator.Current().GetFavoriteAsync();

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

                foreach (var item in grouped)
                {
                    foreach (var song in item)
                    {
                        if (favors.Count == 0)
                            return;
                        if (favors.Contains(song.ID))
                        {
                            song.Favorite = true;
                            favors.Remove(song.ID);
                        }
                    }
                }

                var tileId = "songs";

                IsPinned = SecondaryTile.Exists(tileId);

                if (IsPinned)
                {
                    Core.Tools.Tile.UpdateImage(tileId, SongsList.SelectMany(a => a.Where(c => c.Artwork != null).Select(b => b.Artwork.OriginalString)).Distinct().OrderBy(x => Guid.NewGuid()).Take(10).ToList(), Consts.Localizer.GetString("SongsText"), Consts.Localizer.GetString("SongsTile"));
                }
            });
        }

        internal async Task PlayAlbumAsync(AlbumViewModel album)
        {
            var songs = await album.GetSongsAsync();
            await MainPageViewModel.Current.InstantPlayAsync(songs);
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
            //foreach (var item in SongsList)
            //{
            //    foreach (var song in item)
            //    {
            //        song.RefreshFav();
            //    }
            //}
            var favors = await SQLOperator.Current().GetFavoriteAsync();
            foreach (var item in SongsList)
            {
                foreach (var song in item)
                {
                    if (favors.Count == 0)
                        return;
                    if (favors.Contains(song.ID))
                    {
                        song.Favorite = true;
                        favors.Remove(song.ID);
                    }
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
            await MainPageViewModel.Current.InstantPlayAsync(list, list.FindIndex(x => x.ID == songViewModel.ID));
        }


        public string PinnedtoGlyph(bool b)
        {
            return b ? "\uE196" : "\uE141";
        }
        public string PinnedtoText(bool b)
        {
            return b ? Consts.Localizer.GetString("UnPinText") : Consts.Localizer.GetString("PinText");
        }

        public DelegateCommand PintoStart
        {
            get => new DelegateCommand(async () =>
            {
                // Construct a unique tile ID, which you will need to use later for updating the tile
                var tileId = "songs";
                if (SecondaryTile.Exists(tileId))
                {
                    // Initialize a secondary tile with the same tile ID you want removed
                    var toBeDeleted = new SecondaryTile(tileId);

                    // And then unpin the tile
                    await toBeDeleted.RequestDeleteAsync();
                }
                else
                {
                    // Use a display name you like
                    var displayName = Consts.Localizer.GetString("SongsText");

                    // Provide all the required info in arguments so that when user
                    // clicks your tile, you can navigate them to the correct content
                    var arguments = $"as-music:///library/songs";

                    // Initialize the tile with required arguments
                    var tile = new SecondaryTile
                    {
                        Arguments = arguments,
                        TileId = tileId,
                        DisplayName = displayName
                    };
                    tile.VisualElements.Square150x150Logo = new Uri("ms-appx:///Assets/Square150x150Logo.png");
                    // Enable wide and large tile sizes
                    tile.VisualElements.Wide310x150Logo = new Uri("ms-appx:///Assets/Wide310x150Logo.png");
                    tile.VisualElements.Square310x310Logo = new Uri("ms-appx:///Assets/LargeTile.png");

                    // Add a small size logo for better looking small tile
                    tile.VisualElements.Square71x71Logo = new Uri("ms-appx:///Assets/SmallTile.png");

                    // Add a unique corner logo for the secondary tile
                    tile.VisualElements.Square44x44Logo = new Uri("ms-appx:///Assets/Square44x44Logo.png");

                    tile.VisualElements.ShowNameOnSquare150x150Logo = true;
                    tile.VisualElements.ShowNameOnWide310x150Logo = true;
                    tile.VisualElements.ShowNameOnSquare310x310Logo = true;

                    // Pin the tile
                    await tile.RequestCreateAsync();
                }

                IsPinned = SecondaryTile.Exists(tileId);

                if (IsPinned)
                {
                    Core.Tools.Tile.UpdateImage(tileId, SongsList.SelectMany(a => a.Where(c => c.Artwork != null).Select(b => b.Artwork.OriginalString)).Distinct().OrderBy(x => Guid.NewGuid()).Take(10).ToList(), Consts.Localizer.GetString("SongsText"), Consts.Localizer.GetString("SongsTile"));
                }
            });
        }

        private bool isPinned;
        public bool IsPinned
        {
            get { return isPinned; }
            set { SetProperty(ref isPinned, value); }
        }
    }
}
