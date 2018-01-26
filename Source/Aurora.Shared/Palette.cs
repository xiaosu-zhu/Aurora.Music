// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Aurora.Shared.Helpers;
using Windows.UI;

namespace Aurora.Shared
{
    public static class Palette
    {
        private static Color[] colorarr = new Color[]
        {
            //light yellow
            Color.FromArgb(0xff, 0xff, 0xb9, 0x00),
            //yellow orange
            Color.FromArgb(0xff, 0xff, 0x8c, 0x00),
            //vibrant orange
            Color.FromArgb(0xff, 0xf7, 0x6c, 0x0c),
            //dark orange
            Color.FromArgb(0xff, 0xca, 0x50, 0x10),
            //red orange
            Color.FromArgb(0xff, 0xda, 0x3b, 0x01),
            //light red
            Color.FromArgb(0xff, 0xef, 0x69, 0x50),
            //dark red
            Color.FromArgb(0xff, 0xd1, 0x34, 0x38),
            //vibrant red
            Color.FromArgb(0xff, 0xff, 0x43, 0x43),
            //red pink
            Color.FromArgb(0xff, 0xe7, 0x48, 0x56),
            //pink red
            Color.FromArgb(0xff, 0xe8, 0x11, 0x23),
            //light pink
            Color.FromArgb(0xff, 0xea, 0x00, 0x5e),
            //pink
            Color.FromArgb(0xff, 0xc3, 0x00, 0x52),
            //vibrant pink
            Color.FromArgb(0xff, 0xe3, 0x00, 0x8c),
            //dark pink
            Color.FromArgb(0xff, 0xbf, 0x00, 0x77),
            //pink blue
            Color.FromArgb(0xff, 0xc2, 0x39, 0xb3),
            //dark pink blue
            Color.FromArgb(0xff, 0x9a, 0x00, 0x89),
            //ms blue
            Color.FromArgb(0xff, 0x00, 0x78, 0xd7),
            //dark blue
            Color.FromArgb(0xff, 0x00, 0x63, 0xb1),
            //purple blue
            Color.FromArgb(0xff, 0x00, 0x63, 0xb1),
            //blue purple
            Color.FromArgb(0xff, 0x6B, 0x69, 0xd6),
            //purple
            Color.FromArgb(0xff, 0x87, 0x64, 0xb8),
            //dark purple
            Color.FromArgb(0xff, 0x74, 0x4D, 0xa9),
            //red purple
            Color.FromArgb(0xff, 0xb1, 0x46, 0xc2),
            //dark red purple
            Color.FromArgb(0xff, 0x88, 0x17, 0x98),
            //cyan
            Color.FromArgb(0xff, 0x00, 0x99, 0xbc),
            //dark cyan
            Color.FromArgb(0xff, 0x2d, 0x7d, 0x9a),
            //light cyan
            Color.FromArgb(0xff, 0x00, 0xb7, 0xc3),
            //green cyan
            Color.FromArgb(0xff, 0x03, 0x83, 0x87),
            //light green cyan
            Color.FromArgb(0xff, 0x0, 0xb2, 0x94),
            //cyan green
            Color.FromArgb(0xff, 0x01, 0x85, 0x74),
            //light blue green
            Color.FromArgb(0xff, 0x0, 0xcc, 0x6a),
            //dark blue green
            Color.FromArgb(0xff, 0x10, 0x89, 0x3e),
            //red gray
            Color.FromArgb(0xff, 0x7a, 0x75, 0x74),
            //dark red gray
            Color.FromArgb(0xff, 0x5d, 0x5a, 0x58),
            //blue gray
            Color.FromArgb(0xff, 0x68, 0x76, 0x8a),
            //dark blue gray
            Color.FromArgb(0xff, 0x51, 0x5c, 0x6b),
            //green gray
            Color.FromArgb(0xff, 0x56, 0x7c, 0x73),
            //dark green gray
            Color.FromArgb(0xff, 0x48, 0x68, 0x60),
            //red green
            Color.FromArgb(0xff, 0x49, 0x82, 0x05),
            //green
            Color.FromArgb(0xff, 0x10, 0x7c, 0x10),
            //gray
            Color.FromArgb(0xff, 0x76, 0x76, 0x76),
            //purple gray
            Color.FromArgb(0xff, 0x4c, 0x4a, 0x48),
            //cyan gray
            Color.FromArgb(0xff, 0x69, 0x79, 0x7e),
            //dark cyan gray
            Color.FromArgb(0xff, 0x4a, 0x54, 0x59),
            //light green gray
            Color.FromArgb(0xff, 0x64, 0x7c, 0x64),
            //deep green gray
            Color.FromArgb(0xff, 0x52, 0x5e, 0x54),
            //brown
            Color.FromArgb(0xff, 0x84, 0x75, 0x45),
            //brown gray
            Color.FromArgb(0xff, 0x7e, 0x73, 0x5f)
        };

        public static Color LightYellow
        {
            get
            {
                return colorarr[0];
            }
        }

        public static Color YellowOrange
        {
            get
            {
                return colorarr[1];
            }
        }

        public static Color VibrantOrange
        {
            get
            {
                return colorarr[2];
            }
        }

        public static Color DarkOrange
        {
            get
            {
                return colorarr[3];
            }
        }

        public static Color RedOrange
        {
            get
            {
                return colorarr[4];
            }
        }

        public static Color LightRed
        {
            get
            {
                return colorarr[5];
            }
        }

        public static Color DarkRed
        {
            get
            {
                return colorarr[6];
            }
        }

        public static Color VibrantRed
        {
            get
            {
                return colorarr[7];
            }
        }
        public static Color RedPink
        {
            get
            {
                return colorarr[8];
            }
        }

        public static Color PinkRed
        {
            get
            {
                return colorarr[9];
            }
        }

        public static Color LightPink
        {
            get
            {
                return colorarr[10];
            }
        }

        public static Color Pink
        {
            get
            {
                return colorarr[11];
            }
        }

        public static Color VibrantPink
        {
            get
            {
                return colorarr[12];
            }
        }

        public static Color DarkPink
        {
            get
            {
                return colorarr[13];
            }
        }

        public static Color PinkBlue
        {
            get
            {
                return colorarr[14];
            }
        }

        public static Color DarkPinkBlue
        {
            get
            {
                return colorarr[15];
            }
        }

        public static Color Blue
        {
            get
            {
                return colorarr[16];
            }
        }

        public static Color DarkBlue
        {
            get
            {
                return colorarr[17];
            }
        }

        public static Color PurpleBlue
        {
            get
            {
                return colorarr[18];
            }
        }

        public static Color BluePurple
        {
            get
            {
                return colorarr[19];
            }
        }

        public static Color Purple
        {
            get
            {
                return colorarr[20];
            }
        }

        public static Color DarkPurple
        {
            get
            {
                return colorarr[21];
            }
        }

        public static Color RedPurple
        {
            get
            {
                return colorarr[22];
            }
        }

        public static Color DarkRedPurple
        {
            get
            {
                return colorarr[23];
            }
        }

        public static Color Cyan
        {
            get
            {
                return colorarr[24];
            }
        }

        public static Color DarkCyan
        {
            get
            {
                return colorarr[25];
            }
        }

        public static Color LightCyan
        {
            get
            {
                return colorarr[26];
            }
        }

        public static Color GreenCyan
        {
            get
            {
                return colorarr[28];
            }
        }

        public static Color LightGreenCyan
        {
            get
            {
                return colorarr[28];
            }
        }

        public static Color CyanGreen
        {
            get
            {
                return colorarr[29];
            }
        }

        public static Color LightBlueGreen
        {
            get
            {
                return colorarr[30];
            }
        }

        public static Color DarkBlueGreen
        {
            get
            {
                return colorarr[31];
            }
        }

        public static Color RedGray
        {
            get
            {
                return colorarr[32];
            }
        }

        public static Color DarkRedGray
        {
            get
            {
                return colorarr[33];
            }
        }

        public static Color BlueGray
        {
            get
            {
                return colorarr[34];
            }
        }

        public static Color DarkBlueGray
        {
            get
            {
                return colorarr[35];
            }
        }

        public static Color GreenGray
        {
            get
            {
                return colorarr[36];
            }
        }

        public static Color DarkGreenGray
        {
            get
            {
                return colorarr[37];
            }
        }

        public static Color RedGreen
        {
            get
            {
                return colorarr[38];
            }
        }

        public static Color Green
        {
            get
            {
                return colorarr[39];
            }
        }

        public static Color Gray
        {
            get
            {
                return colorarr[40];
            }
        }

        public static Color PurpleGray
        {
            get
            {
                return colorarr[41];
            }
        }

        public static Color CyanGray
        {
            get
            {
                return colorarr[42];
            }
        }

        public static Color DarkCyanGray
        {
            get
            {
                return colorarr[43];
            }
        }

        public static Color LightGreenGray
        {
            get
            {
                return colorarr[44];
            }
        }

        public static Color DeepGreenGray
        {
            get
            {
                return colorarr[45];
            }
        }

        public static Color Brown
        {
            get
            {
                return colorarr[46];
            }
        }

        public static Color BrownGray
        {
            get
            {
                return colorarr[47];
            }
        }

        public static Color GetRandom()
        {
            return colorarr[Tools.Random.Next(colorarr.Length)];
        }

        public static byte RGBtoL(Color color)
        {
            return (byte)(((color.R * 299) + (color.G * 587) + (color.B * 114)) / 1000);
        }

        public static bool IsDarkColor(Color c)
        {
            return (5 * c.G + 2 * c.R + c.B) <= 8 * 128;
        }
    }
}
