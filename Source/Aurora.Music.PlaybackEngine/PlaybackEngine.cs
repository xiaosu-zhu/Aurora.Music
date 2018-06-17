// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Aurora.Music.PlaybackEngine
{
    public static class PlaybackEngine
    {
        private static IPlayer current;
        public static IPlayer Current
        {
            get
            {
                if (current != null)
                {
                    return current;
                }
                else
                {
                    var p = new NeonPlayer();
                    current = p;
                    return p;
                }
            }
        }
    }
}
