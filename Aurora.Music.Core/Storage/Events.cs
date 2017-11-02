using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Music.Core.Storage
{
    public class SongsAddedEventArgs
    {
        public SONG[] NewSongs { get; set; }

        public SongsAddedEventArgs(SONG[] songs)
        {
            NewSongs = songs;
        }
    }

    public class AlbumModifiedEventArgs
    {
        public ALBUM[] ChangedAlbums { get; set; }

        public AlbumModifiedEventArgs(ALBUM[] album)
        {
            ChangedAlbums = album;
        }
    }
}
