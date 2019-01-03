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
using System.Text;
using System.Threading.Tasks;
using TagLib;
using Windows.ApplicationModel.Core;
using Windows.Media.Playlists;
using Windows.Storage;

namespace Aurora.Music.ViewModels
{
    internal class StoragePlaylistViewModel : ViewModelBase
    {
        private Playlist playlist;

        private ObservableCollection<GroupedItem<StorageSongViewModel>> songsList;
        public ObservableCollection<GroupedItem<StorageSongViewModel>> SongsList
        {
            get { return songsList; }
            set { SetProperty(ref songsList, value); }
        }

        private List<Uri> heroImage;
        public List<Uri> HeroImage
        {
            get { return heroImage; }
            set { SetProperty(ref heroImage, value); }
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
        private StorageFile playlistFile;

        public string Title
        {
            get { return title; }
            set { SetProperty(ref title, value); }
        }

        public DelegateCommand PlayAll
        {
            get
            {
                return new DelegateCommand(async () =>
                {
                    await MainPageViewModel.Current.InstantPlayAsync(SongsList.SelectMany(a => a.Select(b => b.File)).ToList());
                });
            }
        }


        public DelegateCommand Delete
        {
            get
            {
                return new DelegateCommand(async () =>
                {
                    await playlistFile.DeleteAsync();
                    LibraryPage.Current.RemoveStoragePlayList(playlistFile.Name);
                });
            }
        }

        public StoragePlaylistViewModel()
        {
            SongsList = new ObservableCollection<GroupedItem<StorageSongViewModel>>();
        }


        public void Init(StorageFile playlistFile)
        {
            this.playlistFile = playlistFile;
            Task.Run(async () =>
            {
                playlist = await Playlist.LoadAsync(playlistFile);
                var list = new List<StorageSongViewModel>();
                foreach (var item in playlist.Files)
                {
                    try
                    {
                        using (var properties = TagLib.File.Create(item.AsAbstraction()))
                        {
                            var tag = properties.Tag;
                            var song = await Song.CreateAsync(tag, item.Path, await item.GetViolatePropertiesAsync(), properties.Properties, null);
                            list.Add(new StorageSongViewModel(song)
                            {
                                File = item
                            });
                        }
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
                var grouped = GroupedItem<StorageSongViewModel>.CreateGroupsByAlpha(list);
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
                    SongsCount = SmartFormat.Smart.Format(Consts.Localizer.GetString("SmartSongs"), list.Count);
                    Description = playlistFile.Path;
                    Title = playlistFile.DisplayName;
                });
            });
        }


        internal void ChangeSort(int selectedIndex)
        {
            var songs = SongsList.SelectMany(a => a).ToList();
            SongsList.Clear();
            IEnumerable<GroupedItem<StorageSongViewModel>> grouped;

            switch (selectedIndex)
            {
                case 0:
                    grouped = GroupedItem<StorageSongViewModel>.CreateGroupsByAlpha(songs);
                    break;
                case 1:
                    grouped = GroupedItem<StorageSongViewModel>.CreateGroups(songs, x => x.FormattedAlbum);
                    break;
                case 2:
                    grouped = GroupedItem<StorageSongViewModel>.CreateGroups(songs, x => x.GetFormattedArtists());
                    break;
                default:
                    grouped = GroupedItem<StorageSongViewModel>.CreateGroups(songs, x => x.Song.Year, true);
                    break;
            }
            foreach (var item in grouped)
            {
                item.Aggregate((x, y) =>
                {
                    y.Index = x.Index + 1;
                    return y;
                });
                SongsList.Add(item);
            }
        }

        internal async Task PlayAt(StorageSongViewModel songViewModel)
        {
            //var list = await SQLOperator.Current().GetSongsAsync(Model.SongsID);
            await MainPageViewModel.Current.InstantPlayAsync(SongsList.SelectMany(a => a.Select(b => b.File)).ToList(), SongsList.SelectMany(a => a).ToList().FindIndex(x => x == songViewModel));
        }
    }

    internal class StorageSongViewModel : SongViewModel
    {
        public StorageFile File { get; set; }

        public StorageSongViewModel(Song song) : base(song)
        {

        }
    }
}
