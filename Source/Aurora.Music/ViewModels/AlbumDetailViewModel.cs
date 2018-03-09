// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Aurora.Music.Core;
using Aurora.Music.Core.Storage;
using Aurora.Shared.Extensions;
using Aurora.Shared.MVVM;
using SmartFormat;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.System.Threading;

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

        private Uri heroImage;
        public Uri HeroImage
        {
            get { return heroImage; }
            set { SetProperty(ref heroImage, value); }
        }

        public string SongsCount(AlbumViewModel a)
        {
            if (a != null)
            {
                return Smart.Format(Consts.Localizer.GetString("SmartSongs"), a.SongsID.Length);
            }
            return Smart.Format(Consts.Localizer.GetString("SmartSongs"), 0);
        }

        public string GenresToString(AlbumViewModel a)
        {
            if (a != null && !a.Genres.IsNullorEmpty())
            {
                return string.Join(Consts.CommaSeparator, a.Genres);
            }
            return Consts.Localizer.GetString("VariousGenresText");
        }

        public DelegateCommand PlayAll
        {
            get
            {
                return new DelegateCommand(async () =>
                {
                    await MainPageViewModel.Current.InstantPlay(await Album.GetSongsAsync());
                });
            }
        }

        public AlbumDetailViewModel()
        {
            SongList = new ObservableCollection<SongViewModel>();
        }

        public async Task GetSongsAsync(AlbumViewModel a)
        {
            Album = a;
            await a.GetSongsAsync();
            await CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                SongList.Clear();
                for (int i = 0; i < a.Songs.Count; i++)
                {
                    SongList.Add(new SongViewModel(a.Songs[i])
                    {
                        Index = (uint)i
                    });
                }
                foreach (var item in SongList)
                {
                    item.RefreshFav();
                }
            });
            Core.Models.AlbumInfo info = null;
            try
            {
                if (Album.Name != null)
                    info = await MainPageViewModel.Current.GetAlbumInfoAsync(Album.Name, Album.AlbumArtists?.FirstOrDefault());
            }
            catch (Exception)
            {
            }
            await CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                if (info != null)
                {
                    if (Album.ArtworkUri == null && info.AltArtwork != null)
                    {
                        Album.ArtworkUri = info.AltArtwork;
                        var task = ThreadPool.RunAsync(async k =>
                        {
                            if (!Album.IsOnline)
                            {
                                await SQLOperator.Current().UpdateAlbumArtworkAsync(album.ID, info.AltArtwork.OriginalString);
                            }
                        });
                    }
                    Album.Description = info.Description;
                }
                else
                {
                    Album.Description = $"# {Consts.Localizer.GetString("LocaAlbumTitle")}";
                }
            });
        }

        internal async Task PlayAt(SongViewModel songViewModel)
        {
            await MainPageViewModel.Current.InstantPlay(await Album.GetSongsAsync(), songList.IndexOf(songViewModel));
        }
    }
}
