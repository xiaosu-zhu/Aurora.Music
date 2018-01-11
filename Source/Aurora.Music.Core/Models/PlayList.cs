using Aurora.Music.Core.Storage;
using Aurora.Shared.Extensions;
using Aurora.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Music.Core.Models
{
    public class PlayList
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string[] Tags { get; set; }
        public List<Song> Songs { get; set; }
        public int ID { get; set; }
        public string[] HeroArtworks { get; set; }
        public int[] SongsID { get; set; }

        internal PlayList(PLAYLIST p)
        {
            ID = p.ID;
            Title = p.Title;
            Description = p.Description;
            Tags = p.Tags.IsNullorEmpty() ? new string[] { } : p.Tags.Split(new string[] { "$|$" }, StringSplitOptions.RemoveEmptyEntries);
            HeroArtworks = p.HeroArtworks.IsNullorEmpty() ? new string[] { } : p.HeroArtworks.Split(new string[] { "$|$" }, StringSplitOptions.RemoveEmptyEntries);

            var ids = p.IDs.IsNullorEmpty() ? new string[] { } : p.IDs.Split('|', StringSplitOptions.RemoveEmptyEntries);
            SongsID = Array.ConvertAll(ids, (a) =>
            {
                return int.Parse(a);
            });

        }

        public PlayList()
        {
        }

        public async Task SaveAsync()
        {
            await SQLOperator.Current().UpdatePlayListAsync(new PLAYLIST(this));
        }
    }
}
