// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Aurora.Music.Core.Extension.Json.QQMusicArtist
{
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
        [DataMember(Name = "msgdown")]
        public int Msgdown { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "msgfav")]
        public int Msgfav { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "msgid")]
        public int Msgid { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "msgpay")]
        public int Msgpay { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "msgshare")]
        public int Msgshare { get; set; }

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
        /// 
        /// </summary>
        [DataMember(Name = "subtitle")]
        public string Subtitle { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "time_public")]
        public string Time_Public { get; set; }

        ///<summary>
        /// 后青春期的诗
        /// </summary>
        [DataMember(Name = "title")]
        public string Title { get; set; }

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
        [DataMember(Name = "size_128mp3")]
        public int Size_128mp3 { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "size_192aac")]
        public int Size_192aac { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "size_192ogg")]
        public int Size_192ogg { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "size_24aac")]
        public int Size_24aac { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "size_320mp3")]
        public int Size_320mp3 { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "size_48aac")]
        public int Size_48aac { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "size_96aac")]
        public int Size_96aac { get; set; }

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
        [DataMember(Name = "size_try")]
        public int Size_Try { get; set; }

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
        [DataMember(Name = "name")]
        public string Name { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "title")]
        public string Title { get; set; }

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
        public int Peak { get; set; }

    }



    [DataContract]
    public class MusicDataModel
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
        [DataMember(Name = "bpm")]
        public int Bpm { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "data_type")]
        public int Data_Type { get; set; }

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
        [DataMember(Name = "label")]
        public string Label { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "language")]
        public int Language { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "mid")]
        public string Mid { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "modify_stamp")]
        public int Modify_Stamp { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "mv")]
        public MvModel Mv { get; set; }

        ///<summary>
        /// 突然好想你
        /// </summary>
        [DataMember(Name = "name")]
        public string Name { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "pay")]
        public PayModel Pay { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "singer")]
        public List<SingerItemModel> SingerItems { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "size_128")]
        public string Size_128 { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "size_128mp3")]
        public string Size_128mp3 { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "size_192ogg")]
        public string Size_192ogg { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "size_320")]
        public string Size_320 { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "size_320mp3")]
        public string Size_320mp3 { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "size_48aac")]
        public string Size_48aac { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "size_aac")]
        public string Size_Aac { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "size_ogg")]
        public string Size_Ogg { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "status")]
        public int Status { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "subtitle")]
        public string Subtitle { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "time_public")]
        public string Time_Public { get; set; }

        ///<summary>
        /// 突然好想你
        /// </summary>
        [DataMember(Name = "title")]
        public string Title { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "trace")]
        public string Trace { get; set; }

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
        [DataMember(Name = "version")]
        public int Version { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "volume")]
        public VolumeModel Volume { get; set; }

    }



    [DataContract]
    public class VidModel
    {
        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Fmv_id")]
        public string Fmv_Id { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Fstatus")]
        public string Fstatus { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Fvid")]
        public string Fvid { get; set; }

    }



    [DataContract]
    public class ListItemModel
    {
        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Flisten_count1")]
        public string Flisten_Count1 { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Fupload_time")]
        public string Fupload_Time { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "index")]
        public int Index { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "isnew")]
        public int Isnew { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "listenCount")]
        public int ListenCount { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "musicData")]
        public MusicDataModel MusicData { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "playurl")]
        public string Playurl { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "price")]
        public int Price { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "vid")]
        public VidModel Vid { get; set; }

    }



    [DataContract]
    public class DataModel
    {
        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "list")]
        public List<ListItemModel> ListItems { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "singer_id")]
        public string Singer_Id { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "singer_mid")]
        public string Singer_Mid { get; set; }

        ///<summary>
        /// 五月天
        /// </summary>
        [DataMember(Name = "singer_name")]
        public string Singer_Name { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "total")]
        public int Total { get; set; }

    }



    [DataContract]
    public class QQMusicArtistJson
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
        [DataMember(Name = "subcode")]
        public int Subcode { get; set; }

    }


}
