using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Music.Core.Extension.Json
{
    [DataContract]
    public class QcItemModel
    {
        ///<summary>
        /// 五月天
        /// </summary>
        [DataMember(Name = "text")]
        public string Text { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "type")]
        public int Type { get; set; }

    }



    [DataContract]
    public class SemanticModel
    {
        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "curnum")]
        public int Curnum { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "curpage")]
        public int Curpage { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "list")]
        public List<string> ListItems { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "totalnum")]
        public int Totalnum { get; set; }

    }



    [DataContract]
    public class ActionModel
    {
        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "alert")]
        public int Alert { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "icons")]
        public int Icons { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "msg")]
        public int Msg { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "switch")]
        public int Switch { get; set; }

    }



    [DataContract]
    public class AlbumModel
    {
        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "id")]
        public int Id { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "mid")]
        public string Mid { get; set; }

        ///<summary>
        /// 后青春期的诗
        /// </summary>
        [DataMember(Name = "name")]
        public string Name { get; set; }

        ///<summary>
        /// 《疯岳撬佳人》电影插曲
        /// </summary>
        [DataMember(Name = "subtitle")]
        public string Subtitle { get; set; }

        ///<summary>
        /// 后青春期的诗
        /// </summary>
        [DataMember(Name = "title")]
        public string Title { get; set; }

        ///<summary>
        /// 后青春期的诗
        /// </summary>
        [DataMember(Name = "title_hilight")]
        public string Title_Hilight { get; set; }

    }



    [DataContract]
    public class FileModel
    {
        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "media_mid")]
        public string Media_Mid { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "size_128")]
        public int Size_128 { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "size_320")]
        public int Size_320 { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "size_aac")]
        public int Size_Aac { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "size_ape")]
        public int Size_Ape { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "size_dts")]
        public int Size_Dts { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "size_flac")]
        public int Size_Flac { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "size_ogg")]
        public int Size_Ogg { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "size_try")]
        public int Size_Try { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "strMediaMid")]
        public string StrMediaMid { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "try_begin")]
        public int Try_Begin { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "try_end")]
        public int Try_End { get; set; }

    }


    [DataContract]
    public class KsongModel
    {
        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "id")]
        public int Id { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "mid")]
        public string Mid { get; set; }

    }



    [DataContract]
    public class MvModel
    {
        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "id")]
        public int Id { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "vid")]
        public string Vid { get; set; }

    }



    [DataContract]
    public class PayModel
    {
        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "pay_down")]
        public int Pay_Down { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "pay_month")]
        public int Pay_Month { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "pay_play")]
        public int Pay_Play { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "pay_status")]
        public int Pay_Status { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "price_album")]
        public int Price_Album { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "price_track")]
        public int Price_Track { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "time_free")]
        public int Time_Free { get; set; }

    }



    [DataContract]
    public class SingerItemModel
    {
        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "id")]
        public int Id { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "mid")]
        public string Mid { get; set; }

        ///<summary>
        /// 五月天
        /// </summary>
        [DataMember(Name = "name")]
        public string Name { get; set; }

        ///<summary>
        /// 五月天
        /// </summary>
        [DataMember(Name = "title")]
        public string Title { get; set; }

        ///<summary>
        /// 五月天
        /// </summary>
        [DataMember(Name = "title_hilight")]
        public string Title_Hilight { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "type")]
        public int Type { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "uin")]
        public int Uin { get; set; }

    }



    [DataContract]
    public class VolumeModel
    {
        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "gain")]
        public double Gain { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "lra")]
        public double Lra { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "peak")]
        public double Peak { get; set; }

    }



    [DataContract]
    public class GrpItemModel
    {
        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "action")]
        public ActionModel Action { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "album")]
        public AlbumModel Album { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "chinesesinger")]
        public int Chinesesinger { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "docid")]
        public string Docid { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "file")]
        public FileModel File { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "fnote")]
        public int Fnote { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "genre")]
        public int Genre { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "id")]
        public int Id { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "index_album")]
        public int Index_Album { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "index_cd")]
        public int Index_Cd { get; set; }

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
        [DataMember(Name = "ksong")]
        public KsongModel Ksong { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "language")]
        public int Language { get; set; }

        ///<summary>
        /// 《疯岳撬佳人》电影插曲
        /// </summary>
        [DataMember(Name = "lyric")]
        public string Lyric { get; set; }

        ///<summary>
        /// 《疯岳撬佳人》电影插曲
        /// </summary>
        [DataMember(Name = "lyric_hilight")]
        public string Lyric_Hilight { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "mid")]
        public string Mid { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "mv")]
        public MvModel Mv { get; set; }

        ///<summary>
        /// 你不是真正的快乐
        /// </summary>
        [DataMember(Name = "name")]
        public string Name { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "newStatus")]
        public int NewStatus { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "nt")]
        public int Nt { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "pay")]
        public PayModel Pay { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "pure")]
        public int Pure { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "singer")]
        public List<SingerItemModel> SingerItems { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "status")]
        public int Status { get; set; }

        ///<summary>
        /// 《疯岳撬佳人》电影插曲
        /// </summary>
        [DataMember(Name = "subtitle")]
        public string Subtitle { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "t")]
        public int T { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "tag")]
        public int Tag { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "time_public")]
        public string Time_Public { get; set; }

        ///<summary>
        /// 你不是真正的快乐
        /// </summary>
        [DataMember(Name = "title")]
        public string Title { get; set; }

        ///<summary>
        /// 你不是真正的快乐
        /// </summary>
        [DataMember(Name = "title_hilight")]
        public string Title_Hilight { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "type")]
        public int Type { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "url")]
        public string Url { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "ver")]
        public int Ver { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "volume")]
        public VolumeModel Volume { get; set; }

    }
    

    [DataContract]
    public class ListItemModel
    {
        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "action")]
        public ActionModel Action { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "album")]
        public AlbumModel Album { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "chinesesinger")]
        public int Chinesesinger { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "docid")]
        public string Docid { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "file")]
        public FileModel File { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "fnote")]
        public int Fnote { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "genre")]
        public int Genre { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "grp")]
        public List<GrpItemModel> GrpItems { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "id")]
        public int Id { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "index_album")]
        public int Index_Album { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "index_cd")]
        public int Index_Cd { get; set; }

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
        [DataMember(Name = "ksong")]
        public KsongModel Ksong { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "language")]
        public int Language { get; set; }

        ///<summary>
        /// 《疯岳撬佳人》电影插曲
        /// </summary>
        [DataMember(Name = "lyric")]
        public string Lyric { get; set; }

        ///<summary>
        /// 《疯岳撬佳人》电影插曲
        /// </summary>
        [DataMember(Name = "lyric_hilight")]
        public string Lyric_Hilight { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "mid")]
        public string Mid { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "mv")]
        public MvModel Mv { get; set; }

        ///<summary>
        /// 你不是真正的快乐
        /// </summary>
        [DataMember(Name = "name")]
        public string Name { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "newStatus")]
        public int NewStatus { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "nt")]
        public long Nt { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "pay")]
        public PayModel Pay { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "pure")]
        public int Pure { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "singer")]
        public List<SingerItemModel> SingerItems { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "status")]
        public int Status { get; set; }

        ///<summary>
        /// 《疯岳撬佳人》电影插曲
        /// </summary>
        [DataMember(Name = "subtitle")]
        public string Subtitle { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "t")]
        public int T { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "tag")]
        public int Tag { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "time_public")]
        public string Time_Public { get; set; }

        ///<summary>
        /// 你不是真正的快乐
        /// </summary>
        [DataMember(Name = "title")]
        public string Title { get; set; }

        ///<summary>
        /// 你不是真正的快乐
        /// </summary>
        [DataMember(Name = "title_hilight")]
        public string Title_Hilight { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "type")]
        public int Type { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "url")]
        public string Url { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "ver")]
        public int Ver { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "volume")]
        public VolumeModel Volume { get; set; }

    }



    [DataContract]
    public class SongModel
    {
        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "curnum")]
        public int Curnum { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "curpage")]
        public int Curpage { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "list")]
        public List<ListItemModel> ListItems { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "totalnum")]
        public int Totalnum { get; set; }

    }



    [DataContract]
    public class ZhidaModel
    {
        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "chinesesinger")]
        public int Chinesesinger { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "type")]
        public int Type { get; set; }

    }



    [DataContract]
    public class DataModel
    {
        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "keyword")]
        public string Keyword { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "priority")]
        public int Priority { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "qc")]
        public List<QcItemModel> QcItems { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "semantic")]
        public SemanticModel Semantic { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "song")]
        public SongModel Song { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "taglist")]
        public List<string> TaglistItems { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "totaltime")]
        public int Totaltime { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "zhida")]
        public ZhidaModel Zhida { get; set; }

    }



    [DataContract]
    public class QQMusicSearchJson
    {
        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "code")]
        public int Code { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "data")]
        public DataModel Data { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "message")]
        public string Message { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "notice")]
        public string Notice { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "subcode")]
        public int Subcode { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "time")]
        public long Time { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "tips")]
        public string Tips { get; set; }

    }

    [DataContract]
    public class QQMusicFileJson
    {
        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "code")]
        public int Code { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "sip")]
        public List<string> SipItems { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "thirdip")]
        public List<string> ThirdipItems { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "testfile2g")]
        public string Testfile2g { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "testfilewifi")]
        public string Testfilewifi { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "key")]
        public string Key { get; set; }

    }
}
