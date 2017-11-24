using Aurora.Music.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
