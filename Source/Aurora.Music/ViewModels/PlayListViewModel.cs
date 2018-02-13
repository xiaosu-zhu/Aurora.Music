// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Aurora.Music.Core;
using Aurora.Music.Core.Models;
using Aurora.Shared.Extensions;
using Aurora.Shared.MVVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Aurora.Music.ViewModels
{
    sealed class PlayListViewModel : ViewModelBase
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string[] Tags { get; set; } = new string[] { };
        public List<SongViewModel> Songs { get; set; } = new List<SongViewModel>();
        public int[] SongsID { get; set; } = new int[] { };
        public int ID { get; internal set; }
        private List<Uri> heroArtworks = new List<Uri>();
        public List<Uri> HeroArtworks
        {
            get { return heroArtworks; }
            set { SetProperty(ref heroArtworks, value); }
        }

        internal PlayListViewModel(PlayList p)
        {
            ID = p.ID;
            Title = p.Title;
            Description = p.Description;
            Tags = p.Tags;
            HeroArtworks = p.HeroArtworks == null ? null : Array.ConvertAll(p.HeroArtworks, x => new Uri(x)).ToList();
            Songs = p?.ConvertAll(x => new SongViewModel(x));
            SongsID = p.SongsID;
        }

        public PlayListViewModel()
        {
        }

        public string SongsCount()
        {
            if (SongsID == null)
            {
                SmartFormat.Smart.Format(Consts.Localizer.GetString("SmartSongs"), 0);
            }
            return SmartFormat.Smart.Format(Consts.Localizer.GetString("SmartSongs"), (SongsID ?? new int[] { }).Length);
        }

        internal async Task SaveAsync()
        {
            var p = new PlayList()
            {
                ID = ID,
                HeroArtworks = HeroArtworks == null ? new string[] { } : HeroArtworks.ConvertAll(x => x.OriginalString).ToArray(),
                Title = Title,
                Description = Description,
                Tags = Tags ?? new string[] { },
                SongsID = SongsID ?? new int[] { }
            };

            ID = await p.SaveAsync();
        }

        internal async Task AddAsync(int[] SongID)
        {
            var songs = await Song.GetAsync(SongID);

            var list = new List<int>();

            foreach (var song in songs)
            {
                if (SongsID.Contains(song.ID))
                {
                    return;
                }
                list.Add(song.ID);
                if (!song.PicturePath.IsNullorEmpty())
                {
                    bool b = true;
                    // confirm no duplicate
                    foreach (var item in HeroArtworks)
                    {
                        if (item.OriginalString == song.PicturePath)
                        {
                            b = false;
                            break;
                        }
                    }
                    if (b)
                    {
                        HeroArtworks.Insert(0, new Uri(song.PicturePath));
                    }
                }
            }

            list.AddRange(SongsID);
            SongsID = list.ToArray();

            if (HeroArtworks.Count > 4)
            {
                HeroArtworks.RemoveRange(4, HeroArtworks.Count - 4);
            }

            await SaveAsync();
        }
    }
}
