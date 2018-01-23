using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Music.Core.Models
{
    public class OnlineMusicItem : GenericMusicItem
    {
        public string[] OnlineID { get; private set; }
        public string OnlineAlbumId { get; }

        public OnlineMusicItem(string title, string description, string addtional, string[] id, string albumId)
        {
            Title = title;
            Description = description;
            Addtional = addtional;
            OnlineID = id;
            IsOnline = true;
            OnlineAlbumId = albumId;
        }
    }
}
