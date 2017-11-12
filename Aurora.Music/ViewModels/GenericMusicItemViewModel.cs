using Aurora.Music.Core.Tools;
using Aurora.Shared.Extensions;
using Aurora.Shared.MVVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;
using Aurora.Music.Core.Storage;
using Aurora.Music.Core.Models;
using Windows.UI;
using Windows.UI.Xaml.Media;
using System.Text.RegularExpressions;

namespace Aurora.Music.ViewModels
{
    class GenericMusicItemViewModel : ViewModelBase
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Addtional { get; set; }

        private Uri artwork;

        public Uri Artwork
        {
            get { return artwork; }
            set
            {
                artwork = value;
            }
        }

        public Color MainColor { get; set; }

        public int[] IDs { get; set; }

        public int ContextualID { get; set; }

        public GenericMusicItemViewModel()
        {

        }


        public static void ColorToHSV(System.Drawing.Color color, out double hue, out double saturation, out double value)
        {
            int max = Math.Max(color.R, Math.Max(color.G, color.B));
            int min = Math.Min(color.R, Math.Min(color.G, color.B));

            hue = color.GetHue();
            saturation = (max == 0) ? 0 : 1d - (1d * min / max);
            value = max / 255d;
        }


        public static Color ColorFromHSV(double hue, double saturation, double value)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            value = value * 255;
            var v = Convert.ToByte(value);
            var p = Convert.ToByte(value * (1 - saturation));
            var q = Convert.ToByte(value * (1 - f * saturation));
            var t = Convert.ToByte(value * (1 - (1 - f) * saturation));

            if (hi == 0)
                return Color.FromArgb(255, v, t, p);
            else if (hi == 1)
                return Color.FromArgb(255, q, v, p);
            else if (hi == 2)
                return Color.FromArgb(255, p, v, t);
            else if (hi == 3)
                return Color.FromArgb(255, p, q, v);
            else if (hi == 4)
                return Color.FromArgb(255, t, p, v);
            else
                return Color.FromArgb(255, v, p, q);
        }

        public SolidColorBrush GetMainColor(double d)
        {
            System.Drawing.Color color = System.Drawing.Color.FromArgb(MainColor.R, MainColor.G, MainColor.B);
            ColorToHSV(color, out var h, out var s, out var v);
            v *= d;

            return new SolidColorBrush(ColorFromHSV(h, s, v));
        }

        public GenericMusicItemViewModel(Core.Models.Album album)
        {
            ContextualID = album.ID;
            Title = album.Name;
            Addtional = string.Join(", ", album.AlbumArtists);
            Description = album.Songs.Length + album.Songs.Length == 1 ? " Song" : " Songs";
            Artwork = album.PicturePath.IsNullorEmpty() ? null : new Uri(album.PicturePath);
            IDs = album.Songs;
        }

        public GenericMusicItemViewModel(Core.Models.Song song)
        {
            ContextualID = song.ID;
            Title = song.Title;
            Addtional = string.Join(", ", song.Performers);
            Description = TimeSpanFormatter.GetSongDurationFormat(song.Duration);
            Artwork = song.PicturePath.IsNullorEmpty() ? null : new Uri(song.PicturePath);
            IDs = new int[] { song.ID };
        }
        public GenericMusicItemViewModel(Core.Models.GenericMusicItem item)
        {
            ContextualID = item.ContextualID;
            Title = item.Title;
            Description = item.Description;
            Addtional = item.Addtional;
            IDs = item.IDs;
            Artwork = item.PicturePath.IsNullorEmpty() ? null : new Uri(item.PicturePath);
        }

        internal async Task<IList<Song>> GetSongsAsync()
        {
            var opr = SQLOperator.Current();
            return await opr.GetSongsAsync(IDs);
        }

        public override string ToString()
        {
            var title = Title;
            if (title.Length > 20)
            {
                title = title.Substring(0, 20);
                title += "…";
            }
            return $"{title} - {Description}";
        }
    }
}
