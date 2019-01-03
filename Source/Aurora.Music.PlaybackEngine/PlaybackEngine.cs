// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Aurora.Music.Core.Models;

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
                    IPlayer p;
                    switch (Settings.Current.PlaybackEngine)
                    {
                        case Engine.System:
                            p = new Player();
                            break;
                        case Engine.Neon:
                            p = new NeonPlayer();
                            break;
                        default:
                            p = new Player();
                            Settings.Current.PlaybackEngine = Engine.System;
                            Settings.Current.Save();
                            break;
                    }
                    current = p;
                    return p;
                }
            }
        }
    }
}
