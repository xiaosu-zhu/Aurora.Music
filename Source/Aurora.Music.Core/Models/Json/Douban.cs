// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System.Collections.Generic;

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


    public class SingersItemModel
    {
        ///<summary>
        /// 
        /// </summary>
        public List<string> style { get; set; }

        ///<summary>
        /// 清水信之
        /// </summary>
        public string name { get; set; }

        ///<summary>
        /// 
        /// </summary>
        public List<string> region { get; set; }

        ///<summary>
        /// 清水信之
        /// </summary>
        public string name_usual { get; set; }

        ///<summary>
        /// 
        /// </summary>
        public List<string> genre { get; set; }

        ///<summary>
        /// 
        /// </summary>
        public string avatar { get; set; }

        ///<summary>
        /// 
        /// </summary>
        public int related_site_id { get; set; }

        ///<summary>
        /// 
        /// </summary>
        public bool is_site_artist { get; set; }

        ///<summary>
        /// 
        /// </summary>
        public string id { get; set; }

    }



    public class ReleaseModel
    {
        ///<summary>
        /// 
        /// </summary>
        public string link { get; set; }

        ///<summary>
        /// 
        /// </summary>
        public string id { get; set; }

        ///<summary>
        /// 
        /// </summary>
        public string ssid { get; set; }

    }

    public class PlaySource
    {
        public double confidence { get; set; }
        public string source_full_name { get; set; }
        public string file_url { get; set; }
        public string source { get; set; }
        public string source_id { get; set; }
        public bool playable { get; set; }
        public string page_url { get; set; }
    }

    public class SongItemModel
    {
        ///<summary>
        /// 
        /// </summary>
        public List<PlaySource> all_play_sources { get; set; }

        ///<summary>
        /// 
        /// </summary>
        public string albumtitle { get; set; }

        ///<summary>
        /// 
        /// </summary>
        public string url { get; set; }

        ///<summary>
        /// 
        /// </summary>
        public string file_ext { get; set; }

        ///<summary>
        /// 
        /// </summary>
        public string album { get; set; }

        ///<summary>
        /// 
        /// </summary>
        public string ssid { get; set; }

        ///<summary>
        /// 
        /// </summary>
        public string title { get; set; }

        ///<summary>
        /// 
        /// </summary>
        public string sid { get; set; }

        ///<summary>
        /// 
        /// </summary>
        public string sha256 { get; set; }

        ///<summary>
        /// 
        /// </summary>
        public int status { get; set; }

        ///<summary>
        /// 
        /// </summary>
        public string picture { get; set; }

        ///<summary>
        /// 
        /// </summary>
        public long update_time { get; set; }

        ///<summary>
        /// 
        /// </summary>
        public string alert_msg { get; set; }

        ///<summary>
        /// 
        /// </summary>
        public bool is_douban_playable { get; set; }

        ///<summary>
        /// 
        /// </summary>
        public string public_time { get; set; }

        ///<summary>
        /// 
        /// </summary>
        public List<string> partner_sources { get; set; }

        ///<summary>
        /// 
        /// </summary>
        public List<SingersItemModel> singers { get; set; }

        ///<summary>
        /// 
        /// </summary>
        public int like { get; set; }

        ///<summary>
        /// 清水信之
        /// </summary>
        public string artist { get; set; }

        ///<summary>
        /// 
        /// </summary>
        public bool is_royal { get; set; }

        ///<summary>
        /// 
        /// </summary>
        public string subtype { get; set; }

        ///<summary>
        /// 
        /// </summary>
        public int length { get; set; }

        ///<summary>
        /// 
        /// </summary>
        public ReleaseModel release { get; set; }

        ///<summary>
        /// 
        /// </summary>
        public string aid { get; set; }

        ///<summary>
        /// 
        /// </summary>
        public string kbps { get; set; }

        ///<summary>
        /// 
        /// </summary>
        public Dictionary<string, int> available_formats { get; set; }

    }



    public class playlist
    {
        ///<summary>
        /// 
        /// </summary>
        public string warning { get; set; }

        public string err { get; set; }

        ///<summary>
        /// 
        /// </summary>
        public int r { get; set; }

        ///<summary>
        /// 
        /// </summary>
        public int version_max { get; set; }

        ///<summary>
        /// 
        /// </summary>
        public int is_show_quick_start { get; set; }

        ///<summary>
        /// 
        /// </summary>
        public List<SongItemModel> song { get; set; }

    }

}
