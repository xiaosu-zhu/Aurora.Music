using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Music.Core.Extension.Json
{
    [DataContract]
    public class TagsItemModel
    {
        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "id")]
        public long Id { get; set; }

        ///<summary>
        /// 情歌
        /// </summary>
        [DataMember(Name = "name")]
        public string Name { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "pid")]
        public long Pid { get; set; }

    }

    [DataContract]
    public class PayModel1
    {
        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "payalbum")]
        public int Payalbum { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "payalbumprice")]
        public int Payalbumprice { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "paydownload")]
        public int Paydownload { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "payinfo")]
        public int Payinfo { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "payplay")]
        public int Payplay { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "paytrackmouth")]
        public int Paytrackmouth { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "paytrackprice")]
        public int Paytrackprice { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "timefree")]
        public int Timefree { get; set; }

    }



    [DataContract]
    public class PreviewModel
    {
        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "trybegin")]
        public int Trybegin { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "tryend")]
        public int Tryend { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "trysize")]
        public int Trysize { get; set; }

    }



    [DataContract]
    public class SongListModel
    {
        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "albumdesc")]
        public string Albumdesc { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "albumid")]
        public long Albumid { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "albummid")]
        public string Albummid { get; set; }

        ///<summary>
        /// 强迫症少女日记
        /// </summary>
        [DataMember(Name = "albumname")]
        public string Albumname { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "alertid")]
        public long Alertid { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "belongCD")]
        public int BelongCD { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "cdIdx")]
        public int CdIdx { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "interval")]
        public int Interval { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "isonly")]
        public int Isonly { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "label")]
        public string Label { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "msgid")]
        public int Msgid { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "pay")]
        public PayModel1 Pay { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "preview")]
        public PreviewModel Preview { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "rate")]
        public int Rate { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "singer")]
        public List<SingerItemModel> SingerItems { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "size128")]
        public int Size128 { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "size320")]
        public int Size320 { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "size5_1")]
        public int Size5_1 { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "sizeape")]
        public int Sizeape { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "sizeflac")]
        public int Sizeflac { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "sizeogg")]
        public int Sizeogg { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "songid")]
        public long Songid { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "songmid")]
        public string Songmid { get; set; }

        ///<summary>
        /// 挑个时间让我梦见你
        /// </summary>
        [DataMember(Name = "songname")]
        public string Songname { get; set; }

        ///<summary>
        /// 挑个时间让我梦见你
        /// </summary>
        [DataMember(Name = "songorig")]
        public string Songorig { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "songtype")]
        public int Songtype { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "strMediaMid")]
        public string StrMediaMid { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "stream")]
        public int Stream { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "switch")]
        public int Switch { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "type")]
        public int Type { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "vid")]
        public string Vid { get; set; }

    }





    [DataContract]
    public class CdlistItemModel
    {
        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "disstid")]
        public string Disstid { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "dirid")]
        public long Dirid { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "dir_show")]
        public int Dir_Show { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "coveradurl")]
        public string Coveradurl { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "dissid")]
        public long Dissid { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "login")]
        public string Login { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "uin")]
        public string Uin { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "encrypt_uin")]
        public string Encrypt_Uin { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "owndir")]
        public int Owndir { get; set; }

        ///<summary>
        /// 抒情小调：洗涤烦恼不打烊
        /// </summary>
        [DataMember(Name = "dissname")]
        public string Dissname { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "logo")]
        public string Logo { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "pic_mid")]
        public string Pic_Mid { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "album_pic_mid")]
        public string Album_Pic_Mid { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "pic_dpi")]
        public int Pic_Dpi { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "isAd")]
        public int IsAd { get; set; }

        ///<summary>
        /// 每一首歌都有它想表达的意思，或是忧伤，或是快乐，或是心动，每一次心情的体验则来源于这些歌曲的完美氛围。从旋律到人声，从歌词到节奏，听歌的你会随着歌曲的氛围改变而改变。<br><br>这大概就是音乐赋予我们的魅力吧！这些年推荐了很多好听的小众歌曲，也从中领悟了很多道理。感谢大家那么支持我，那么一起听歌吧！
        /// </summary>
        [DataMember(Name = "desc")]
        public string Desc { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "ctime")]
        public long Ctime { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "mtime")]
        public long Mtime { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "headurl")]
        public string Headurl { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "ifpicurl")]
        public string Ifpicurl { get; set; }

        ///<summary>
        /// 念安娜
        /// </summary>
        [DataMember(Name = "nick")]
        public string Nick { get; set; }

        ///<summary>
        /// 念安娜
        /// </summary>
        [DataMember(Name = "nickname")]
        public string Nickname { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "type")]
        public int Type { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "singerid")]
        public long Singerid { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "singermid")]
        public string Singermid { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "isvip")]
        public int Isvip { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "isdj")]
        public int Isdj { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "tags")]
        public List<TagsItemModel> TagsItems { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "songnum")]
        public int Songnum { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "songids")]
        public string Songids { get; set; }


        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "songlist")]
        public List<SongListModel> SongList { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "songtypes")]
        public string Songtypes { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "disstype")]
        public int Disstype { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "dir_pic_url2")]
        public string Dir_Pic_Url2 { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "song_update_time")]
        public long Song_Update_Time { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "song_update_num")]
        public int Song_Update_Num { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "total_song_num")]
        public int Total_Song_Num { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "song_begin")]
        public int Song_Begin { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "cur_song_num")]
        public int Cur_Song_Num { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "visitnum")]
        public long Visitnum { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "cmtnum")]
        public long Cmtnum { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "buynum")]
        public long Buynum { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "scoreavage")]
        public string Scoreavage { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "scoreusercount")]
        public long Scoreusercount { get; set; }

    }



    [DataContract]
    public class QQMusicDiscJson
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
        [DataMember(Name = "accessed_plaza_cache")]
        public int Accessed_Plaza_Cache { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "accessed_favbase")]
        public int Accessed_Favbase { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "login")]
        public string Login { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "cdnum")]
        public int Cdnum { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "cdlist")]
        public List<CdlistItemModel> CdlistItems { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "realcdnum")]
        public int Realcdnum { get; set; }

    }
}
