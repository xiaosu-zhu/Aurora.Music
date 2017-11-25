using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Music.Core.Models
{
    public class OnlineSong : Song
    {
        public Uri SongUri { get; private set; }
    }

    public class OnlineMusicItem : GenericMusicItem
    {
        public string[] OnlineID { get; private set; }

        public OnlineMusicItem(string title, string description, string addtional, string[] id)
        {
            Title = title;
            Description = description;
            Addtional = addtional;
            OnlineID = id;
            IsOnline = true;
        }
    }
}
