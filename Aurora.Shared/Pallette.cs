using Aurora.Shared.Helpers;
using Windows.UI;

namespace Aurora.Shared
{
    public static class Palette
    {

        private static readonly Color[] pallette = new Color[]
        {
            //blue
            Color.FromArgb(0xff, 0x21, 0x96, 0xf3),
            //red
            Color.FromArgb(0xff, 0xf4, 0x43, 0x36),
            //orange
            Color.FromArgb(0xff, 0xff, 0x98, 0x00),
            //green
            Color.FromArgb(0xff, 0x4c, 0xaf, 0x50),
            //cyan
            Color.FromArgb(0xff, 0x00, 0xbc, 0xd4),
            //pink
            Color.FromArgb(0xff, 0xe9, 0x1e, 0x63),
            //purple
            Color.FromArgb(0xff, 0x9c, 0x27, 0xb0),
            //deep purple
            Color.FromArgb(0xff, 0x67, 0x3a, 0xb7),
            //indigo
            Color.FromArgb(0xff, 0x3f, 0x51, 0xb5),
            //light blue
            Color.FromArgb(0xff, 0x03, 0xa9, 0xf4),
            //teal
            Color.FromArgb(0xff, 0x00, 0x96, 0x88),
            //light green
            Color.FromArgb(0xff, 0x8b, 0xc3, 0x4a),
            //Yellow
            Color.FromArgb(0xff, 0xff, 0xeb, 0x3b),
            //amber
            Color.FromArgb(0xff, 0xff, 0xc1, 0x07),
            //deep orange
            Color.FromArgb(0xff, 0xff, 0x57, 0x22),
            //brown
            Color.FromArgb(0xff, 0x79, 0x55, 0x48),
            //gray
            Color.FromArgb(0xff, 0x9e, 0x9e, 0x9e),
            //blue gray
            Color.FromArgb(0xff, 0x60, 0x7d, 0x8b),
            //lime
            Color.FromArgb(0xff, 0xcd, 0xdc, 0x39)
        };

        public static Color Red
        {
            get
            {
                return pallette[1];
            }
        }

        public static Color Blue
        {
            get
            {
                return pallette[0];
            }
        }

        public static Color Orange
        {
            get
            {
                return pallette[2];
            }
        }

        public static Color Green
        {
            get
            {
                return pallette[3];
            }
        }

        public static Color Cyan
        {
            get
            {
                return pallette[4];
            }
        }

        public static Color Pink
        {
            get
            {
                return pallette[5];
            }
        }

        public static Color Purple
        {
            get
            {
                return pallette[6];
            }
        }

        public static Color DeepPurple
        {
            get
            {
                return pallette[7];
            }
        }
        public static Color Indigo
        {
            get
            {
                return pallette[8];
            }
        }

        public static Color LightBlue
        {
            get
            {
                return pallette[9];
            }
        }

        public static Color Teal
        {
            get
            {
                return pallette[10];
            }
        }

        public static Color LightGreen
        {
            get
            {
                return pallette[11];
            }
        }

        public static Color Yellow
        {
            get
            {
                return pallette[12];
            }
        }

        public static Color Amber
        {
            get
            {
                return pallette[13];
            }
        }

        public static Color DeepOrange
        {
            get
            {
                return pallette[14];
            }
        }

        public static Color Brown
        {
            get
            {
                return pallette[15];
            }
        }

        public static Color Gray
        {
            get
            {
                return pallette[16];
            }
        }

        public static Color BlueGray
        {
            get
            {
                return pallette[17];
            }
        }

        public static Color Lime
        {
            get
            {
                return pallette[18];
            }
        }

        public static Color GetRandom()
        {
            return pallette[Tools.Random.Next(18)];
        }

        public static byte RGBtoL(Color color)
        {
            return (byte)(((color.R * 299) + (color.G * 587) + (color.B * 114)) / 1000);
        }
    }
}
