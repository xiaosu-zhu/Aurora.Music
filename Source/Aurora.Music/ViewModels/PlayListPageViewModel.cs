// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Aurora.Music.Core;
using Aurora.Music.Core.Models;
using Aurora.Music.Core.Storage;
using Aurora.Music.Pages;
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
    sealed class PlayListPageViewModel : ViewModelBase
    {

        private ObservableCollection<GroupedItem<SongViewModel>> songsList;
        public ObservableCollection<GroupedItem<SongViewModel>> SongsList
        {
            get { return songsList; }
            set { SetProperty(ref songsList, value); }
        }

        private string desc;
        public string Description
        {
            get { return desc; }
            set { SetProperty(ref desc, value); }
        }

        private string songsCount;
        public string SongsCount
        {
            get { return songsCount; }
            set { SetProperty(ref songsCount, value); }
        }

        private string title;
        public string Title
        {
            get { return title; }
            set { SetProperty(ref title, value); }
        }

        public PlayListPageViewModel()
        {
            SongsList = new ObservableCollection<GroupedItem<SongViewModel>>();
        }

        public DelegateCommand PlayAll
        {
            get
            {
                return new DelegateCommand(async () =>
                {
                    await MainPageViewModel.Current.InstantPlayAsync(await SQLOperator.Current().GetSongsAsync(Model.SongsID));
                });
            }
        }

        public DelegateCommand Delete
        {
            get
            {
                return new DelegateCommand(async () =>
                {
                    await SQLOperator.Current().RemovePlayListAsync(Model.ID);
                    LibraryPage.Current.RemovePlayList(Model);
                });
            }
        }

        public PlayList Model { get; private set; }
        public int SortIndex { get; internal set; } = 0;

        public async Task InitAsync(PlayList model)
        {
            Model = await SQLOperator.Current().GetPlayListAsync(model.ID);
            if (Model == null)
            {
                return;
            }

            var songs = await SQLOperator.Current().GetSongsAsync(Model.SongsID);

            IEnumerable<GroupedItem<SongViewModel>> grouped;

            switch (Settings.Current.PlaylistSort)
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
                Description = Model.Description;
                Title = Model.Title;
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

                var tileId = $"playlist{Model.ID}";
                IsPinned = SecondaryTile.Exists(tileId);

                if (IsPinned)
                {
                    Core.Tools.Tile.UpdateImage(tileId, SongsList.SelectMany(a => a.Where(c => c.Artwork != null).Select(b => b.Artwork.OriginalString)).Distinct().OrderBy(x => Guid.NewGuid()).Take(10).ToList(), Model.Title, Model.Description);
                }
            });
        }

        internal async Task DeleteSong(SongViewModel songViewModel)
        {
            Model.SongsID = Model.SongsID.Where(a => a != songViewModel.ID).ToArray();
            await Model.SaveAsync();
        }

        internal async void ChangeSort(int selectedIndex)
        {
            SongsList.Clear();
            var songs = await SQLOperator.Current().GetSongsAsync(Model.SongsID);
            IEnumerable<GroupedItem<SongViewModel>> grouped;

            switch (selectedIndex)
            {
                case 0:
                    grouped = GroupedItem<SongViewModel>.CreateGroupsByAlpha(songs.ConvertAll(x => new SongViewModel(x)));
                    Settings.Current.PlaylistSort = SortMode.Alphabet;
                    break;
                case 1:
                    grouped = GroupedItem<SongViewModel>.CreateGroups(songs.ConvertAll(x => new SongViewModel(x)), x => x.FormattedAlbum);
                    Settings.Current.PlaylistSort = SortMode.Album;
                    break;
                case 2:
                    grouped = GroupedItem<SongViewModel>.CreateGroups(songs.ConvertAll(x => new SongViewModel(x)), x => x.GetFormattedArtists());
                    Settings.Current.PlaylistSort = SortMode.Artist;
                    break;
                case 3:
                    grouped = GroupedItem<SongViewModel>.CreateGroups(songs.ConvertAll(x => new SongViewModel(x)), x => x.Song.Year);
                    Settings.Current.PlaylistSort = SortMode.Year;
                    break;
                default:
                    grouped = GroupedItem<SongViewModel>.CreateGroupsByAlpha(songs.ConvertAll(x => new SongViewModel(x)));
                    Settings.Current.PlaylistSort = SortMode.Alphabet;
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
            var list = await SQLOperator.Current().GetSongsAsync(Model.SongsID);
            await MainPageViewModel.Current.InstantPlayAsync(list, list.FindIndex(x => x.ID == songViewModel.ID));
        }

        internal async Task EditDescription(string text)
        {
            Model.Description = text;
            await Model.SaveAsync();
            Description = text;
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
                var tileId = $"playlist{Model.ID}";
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
                    var displayName = Model.Title;

                    // Provide all the required info in arguments so that when user
                    // clicks your tile, you can navigate them to the correct content
                    var arguments = $"as-music:///library/playlist/id/{Model.ID}";

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
                    Core.Tools.Tile.UpdateImage(tileId, SongsList.SelectMany(a => a.Where(c => c.Artwork != null).Select(b => b.Artwork.OriginalString)).Distinct().OrderBy(x => Guid.NewGuid()).Take(10).ToList(), Model.Title, Model.Description);
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
