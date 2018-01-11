using Aurora.Music.Core.Models;
using Aurora.Shared.MVVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Music.ViewModels
{
    sealed class PlayListViewModel : ViewModelBase
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string[] Tags { get; set; }
        public List<SongViewModel> Songs { get; set; }
        public int[] SongsID { get; set; }
        public int ID { get; internal set; }
        public List<Uri> HeroArtworks { get; internal set; }

        internal PlayListViewModel(PlayList p)
        {
            ID = p.ID;
            Title = p.Title;
            Description = p.Description;
            Tags = p.Tags;
            HeroArtworks = p.HeroArtworks == null ? null : Array.ConvertAll(p.HeroArtworks, x => new Uri(x)).ToList();
            Songs = p.Songs?.ConvertAll(x => new SongViewModel(x));
            SongsID = p.SongsID;
        }

        public PlayListViewModel()
        {
        }

        public string SongsCount()
        {
            if (SongsID == null || SongsID.Length == 0)
            {
                return "0 songs";
            }
            return $"{SongsID.Length} " + (SongsID.Length == 1 ? "song" : "songs");
        }

        internal async Task SaveAsync()
        {
            var p = new PlayList()
            {
                ID = ID,
                HeroArtworks = HeroArtworks == null ? new string[] { } : HeroArtworks.ConvertAll(x => x.OriginalString).ToArray(),
                Title = Title,
                Description = Description,
                Tags = Tags ?? new string[] { }
            };
            p.SongsID = SongsID ?? new int[] { };

            await p.SaveAsync();
        }
    }
}
