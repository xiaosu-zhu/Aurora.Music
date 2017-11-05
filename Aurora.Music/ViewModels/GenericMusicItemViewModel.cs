using Aurora.Music.Core.Utils;
using Aurora.Shared.Extensions;
using Aurora.Shared.MVVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace Aurora.Music.ViewModels
{
    class GenericMusicItemViewModel : ViewModelBase
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Addtional { get; set; }

        public BitmapImage Artwork { get; set; }

        public int[] IDs { get; set; }

        public GenericMusicItemViewModel()
        {

        }

        public GenericMusicItemViewModel(Core.Models.Album album)
        {
            Title = album.Name;
            Addtional = string.Join(", ", album.AlbumArtists);
            Description = album.Songs.Length + album.Songs.Length == 1 ? " Song" : " Songs";
            Artwork = album.PicturePath.IsNullorEmpty() ? null : new BitmapImage(new Uri(album.PicturePath));
            IDs = album.Songs;
        }

        public GenericMusicItemViewModel(Core.Models.Song song)
        {
            Title = song.Title;
            Addtional = string.Join(", ", song.Performers);
            Description = TimeSpanFormatter.GetSongDurationFormat(song.Duration);
            Artwork = song.PicturePath.IsNullorEmpty() ? null : new BitmapImage(new Uri(song.PicturePath));
            IDs = new int[] { song.ID };
        }
        public GenericMusicItemViewModel(Core.Models.GenericMusicItem item)
        {
            Title = item.Title;
            Description = item.Description;
            Addtional = item.Addtional;
            IDs = item.IDs;
            Artwork = item.PicturePath.IsNullorEmpty() ? null : new BitmapImage(new Uri(item.PicturePath));
        }
    }
}
