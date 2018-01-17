using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Music.Core.Models.Json
{
    public class Douban
    {
        public Group[] groups { get; set; }
    }

    public class Group
    {
        public Channel[] chls { get; set; }
        public int group_id { get; set; }
        public string group_name { get; set; }
    }

    public class Channel
    {
        public string name { get; set; }
        public string intro { get; set; }
        public string artist { get; set; }
        public Relation channel_relation { get; set; }
        public string cover { get; set; }
        public int channel_type { get; set; }
        public string start { get; set; }
        public int song_num { get; set; }
        public string collected { get; set; }
        public int id { get; set; }
    }

    public class Relation
    {
        public Song song { get; set; }
    }

    public class Song
    {
        public string id { get; set; }
        public string ssid { get; set; }
    }
}
