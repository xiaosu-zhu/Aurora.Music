// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Aurora.Music.Core.Storage;
using Aurora.Shared.Extensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Aurora.Music.Core.Models
{
    public class PlayList : List<Song>
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string[] Tags { get; set; }
        public int ID { get; set; }

        /// <summary>
        /// max: 3, choose the latest added songs
        /// </summary>
        public string[] HeroArtworks { get; set; }
        public int[] SongsID { get; set; }

        internal PlayList(PLAYLIST p)
        {
            ID = p.ID;
            Title = p.Title;
            Description = p.Description;
            Tags = p.Tags.IsNullorEmpty() ? new string[] { } : p.Tags.Split(new string[] { Consts.ArraySeparator }, StringSplitOptions.RemoveEmptyEntries);
            HeroArtworks = p.HeroArtworks.IsNullorEmpty() ? new string[] { } : p.HeroArtworks.Split(new string[] { Consts.ArraySeparator }, StringSplitOptions.RemoveEmptyEntries);

            var ids = p.IDs.IsNullorEmpty() ? new string[] { } : p.IDs.Split('|', StringSplitOptions.RemoveEmptyEntries);
            SongsID = Array.ConvertAll(ids, (a) =>
            {
                return int.Parse(a);
            });

        }

        public PlayList()
        {
        }

        public virtual async Task<int> SaveAsync()
        {
            return await SQLOperator.Current().UpdatePlayListAsync(new PLAYLIST(this));
        }

        public override string ToString()
        {
            return $"{Title} - " + (Description.IsNullorEmpty() ? "No Desc" : Description);
        }
    }
}
