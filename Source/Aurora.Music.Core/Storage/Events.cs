// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Aurora.Music.Core.Models;

namespace Aurora.Music.Core.Storage
{
    public class SongsAddedEventArgs
    {
        public Song[] NewSongs { get; set; }

        public SongsAddedEventArgs(Song[] songs)
        {
            NewSongs = songs;
        }
    }

    public class AlbumModifiedEventArgs
    {
        public Album[] ChangedAlbums { get; set; }

        public AlbumModifiedEventArgs(Album[] album)
        {
            ChangedAlbums = album;
        }
    }
}
