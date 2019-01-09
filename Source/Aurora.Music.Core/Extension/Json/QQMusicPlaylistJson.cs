using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Music.Core.Extension.Json
{
    [DataContract]
    public class CreatorModel
    {
        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "type")]
        public int Type { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "qq")]
        public long Qq { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "encrypt_uin")]
        public string Encrypt_Uin { get; set; }

        ///<summary>
        /// 念安娜
        /// </summary>
        [DataMember(Name = "name")]
        public string Name { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "isVip")]
        public int IsVip { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "avatarUrl")]
        public string AvatarUrl { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "followflag")]
        public int Followflag { get; set; }

    }



    [DataContract]
    public class PlayListItemModel
    {
        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "dissid")]
        public string Dissid { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "createtime")]
        public string Createtime { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "commit_time")]
        public string Commit_Time { get; set; }

        ///<summary>
        /// 抒情小调：洗涤烦恼不打烊
        /// </summary>
        [DataMember(Name = "dissname")]
        public string Dissname { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "imgurl")]
        public string Imgurl { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "introduction")]
        public string Introduction { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "listennum")]
        public int Listennum { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "score")]
        public double Score { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "version")]
        public double Version { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "creator")]
        public CreatorModel Creator { get; set; }

    }



    [DataContract]
    public class PlaylistDataModel
    {
        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "uin")]
        public long Uin { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "categoryId")]
        public long CategoryId { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "sortId")]
        public long SortId { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "sum")]
        public ulong Sum { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "sin")]
        public long Sin { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "ein")]
        public long Ein { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "list")]
        public List<PlayListItemModel> ListItems { get; set; }

    }



    [DataContract]
    public class QQMusicPlaylistJson
    {
        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "code")]
        public int Code { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "subcode")]
        public int Subcode { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "message")]
        public string Message { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "default")]
        public int Default { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "data")]
        public PlaylistDataModel Data { get; set; }

    }


}
