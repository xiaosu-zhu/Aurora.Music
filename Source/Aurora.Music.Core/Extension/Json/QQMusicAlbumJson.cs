// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Aurora.Music.Core.Extension.Json.QQMusicAlbum
{
    [DataContract]
    public class CompanyModel
    {
        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "brief")]
        public string Brief { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "headPic")]
        public string HeadPic { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "id")]
        public int Id { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "is_show")]
        public int Is_Show { get; set; }

        ///<summary>
        /// 相信音乐
        /// </summary>
        [DataMember(Name = "name")]
        public string Name { get; set; }

    }



    [DataContract]
    public class GetAlbumDescModel
    {
        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Falbum_desc")]
        public string Falbum_Desc { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Falbum_id")]
        public string Falbum_Id { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Falbum_tag3")]
        public string Falbum_Tag3 { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Falbum_tag5")]
        public string Falbum_Tag5 { get; set; }

    }



    [DataContract]
    public class GetAlbumInfoModel
    {
        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Falbum_c_id")]
        public string Falbum_C_Id { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Falbum_id")]
        public string Falbum_Id { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Falbum_mid")]
        public string Falbum_Mid { get; set; }

        ///<summary>
        /// 后青春期的诗
        /// </summary>
        [DataMember(Name = "Falbum_name")]
        public string Falbum_Name { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Fape_size")]
        public string Fape_Size { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Fape_type")]
        public string Fape_Type { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Farea")]
        public string Farea { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Fattribute_1")]
        public string Fattribute_1 { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Fattribute_2")]
        public string Fattribute_2 { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Fattribute_3")]
        public string Fattribute_3 { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Fattribute_4")]
        public string Fattribute_4 { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Fattribute_5")]
        public string Fattribute_5 { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Fcd_index")]
        public string Fcd_Index { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Fclass")]
        public string Fclass { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Fcompany_id")]
        public string Fcompany_Id { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Fcondition")]
        public string Fcondition { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Fcue_size")]
        public string Fcue_Size { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Fgenre")]
        public string Fgenre { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Findex")]
        public string Findex { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Flanguage")]
        public string Flanguage { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Fmodify_time")]
        public string Fmodify_Time { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Fmovie")]
        public object Fmovie { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Fother_name")]
        public string Fother_Name { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Fphoto")]
        public string Fphoto { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Fprice")]
        public string Fprice { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Fpublic_time")]
        public string Fpublic_Time { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Fsinger_all")]
        public string Fsinger_All { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Fsinger_id1")]
        public string Fsinger_Id1 { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Fsinger_id2")]
        public string Fsinger_Id2 { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Fsinger_id3")]
        public string Fsinger_Id3 { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Fsinger_id4")]
        public string Fsinger_Id4 { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Fstatus")]
        public string Fstatus { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Ftopic_id")]
        public string Ftopic_Id { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Ftype")]
        public string Ftype { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Fupc")]
        public string Fupc { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Fupload_time")]
        public string Fupload_Time { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "buy")]
        public int Buy { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "buypage")]
        public string Buypage { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "likeit")]
        public int Likeit { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "price")]
        public int Price { get; set; }

    }



    [DataContract]
    public class GetCompanyInfoModel
    {
        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Fcompany_id")]
        public string Fcompany_Id { get; set; }

        ///<summary>
        /// 相信音乐
        /// </summary>
        [DataMember(Name = "Fcompany_name")]
        public string Fcompany_Name { get; set; }

    }



    [DataContract]
    public class GetOtherAlbumInfoItemModel
    {
        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Falbum_id")]
        public string Falbum_Id { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Falbum_mid")]
        public string Falbum_Mid { get; set; }

        ///<summary>
        /// 自伝 History of Tomorrow
        /// </summary>
        [DataMember(Name = "Falbum_name")]
        public string Falbum_Name { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Fpublic_time")]
        public string Fpublic_Time { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Fsinger_id1")]
        public string Fsinger_Id1 { get; set; }

    }


    [DataContract]
    public class SingerExternModel
    {
        ///<summary>
        /// 五月天
        /// </summary>
        [DataMember(Name = "Finfo_content")]
        public string Finfo_Content { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Finfo_name")]
        public string Finfo_Name { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Fsinger_id")]
        public string Fsinger_Id { get; set; }
    }



    [DataContract]
    public class GetSingerInfoModel
    {
        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Farea")]
        public string Farea { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Fattribute_1")]
        public string Fattribute_1 { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Fattribute_2")]
        public string Fattribute_2 { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Fattribute_3")]
        public string Fattribute_3 { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Fattribute_4")]
        public string Fattribute_4 { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Fattribute_5")]
        public string Fattribute_5 { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Fcompany_id")]
        public string Fcompany_Id { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Fcondition")]
        public string Fcondition { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Fgenre")]
        public string Fgenre { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Fgrade")]
        public string Fgrade { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Findex")]
        public string Findex { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Fmodify_time")]
        public string Fmodify_Time { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Fname_spell")]
        public string Fname_Spell { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Forigin")]
        public string Forigin { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Fother_name")]
        public string Fother_Name { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Fphoto")]
        public string Fphoto { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Fsinger_id")]
        public string Fsinger_Id { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Fsinger_mid")]
        public string Fsinger_Mid { get; set; }

        ///<summary>
        /// 五月天
        /// </summary>
        [DataMember(Name = "Fsinger_name")]
        public string Fsinger_Name { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Fsrc_photo")]
        public string Fsrc_Photo { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Fstatus")]
        public string Fstatus { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Ftype")]
        public string Ftype { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Fupload_time")]
        public string Fupload_Time { get; set; }

    }

    [DataContract]
    public class SongExternModel
    {
        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Flyric_status")]
        public string Flyric_Status { get; set; }

        ///<summary>
        /// 怪兽
        /// </summary>
        [DataMember(Name = "Fmusic_author")]
        public string Fmusic_Author { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Ftrack_id")]
        public string Ftrack_Id { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Ftrack_tag")]
        public string Ftrack_Tag { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Ftrack_tag2")]
        public string Ftrack_Tag2 { get; set; }

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
    public class ExtraModel
    {
        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Fis_only")]
        public string Fis_Only { get; set; }

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
        public double Peak { get; set; }

    }



    [DataContract]
    public class GetSongInfoItemModel
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
        [DataMember(Name = "extra")]
        public ExtraModel Extra { get; set; }

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
    public class SingerInfoItemModel
    {
        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Farea")]
        public string Farea { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Fattribute_1")]
        public string Fattribute_1 { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Fattribute_2")]
        public string Fattribute_2 { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Fattribute_3")]
        public string Fattribute_3 { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Fattribute_4")]
        public string Fattribute_4 { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Fattribute_5")]
        public string Fattribute_5 { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Fcompany_id")]
        public string Fcompany_Id { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Fcondition")]
        public string Fcondition { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Fgenre")]
        public string Fgenre { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Fgrade")]
        public string Fgrade { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Findex")]
        public string Findex { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Fmodify_time")]
        public string Fmodify_Time { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Fname_spell")]
        public string Fname_Spell { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Forigin")]
        public string Forigin { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Fother_name")]
        public string Fother_Name { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Fphoto")]
        public string Fphoto { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Fsinger_id")]
        public string Fsinger_Id { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Fsinger_mid")]
        public string Fsinger_Mid { get; set; }

        ///<summary>
        /// 五月天
        /// </summary>
        [DataMember(Name = "Fsinger_name")]
        public string Fsinger_Name { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Fsrc_photo")]
        public string Fsrc_Photo { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Fstatus")]
        public string Fstatus { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Ftype")]
        public string Ftype { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "Fupload_time")]
        public string Fupload_Time { get; set; }

    }



    [DataContract]
    public class DataModel
    {
        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "albumTips")]
        public object AlbumTips { get; set; }

        ///<summary>
        /// 录音室专辑
        /// </summary>
        [DataMember(Name = "albumtype")]
        public string Albumtype { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "cm_count")]
        public int Cm_Count { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "company")]
        public CompanyModel Company { get; set; }

        ///<summary>
        /// Pop 流行
        /// </summary>
        [DataMember(Name = "genre")]
        public string Genre { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "genre_id")]
        public int Genre_Id { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "getAlbumDesc")]
        public GetAlbumDescModel GetAlbumDesc { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "getAlbumInfo")]
        public GetAlbumInfoModel GetAlbumInfo { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "getCompanyInfo")]
        public GetCompanyInfoModel GetCompanyInfo { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "getOtherAlbumIDs")]
        public List<int> GetOtherAlbumIDsItems { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "getOtherAlbumInfo")]
        public List<GetOtherAlbumInfoItemModel> GetOtherAlbumInfoItems { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "getRecommendAlbumIDs")]
        public string GetRecommendAlbumIDs { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "getRecommendAlbumInfo")]
        public string GetRecommendAlbumInfo { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "getShoufaAlbum")]
        public string GetShoufaAlbum { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "getShowIDs")]
        public List<string> GetShowIDsItems { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "getShowInfo")]
        public object GetShowInfo { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "getSingerExtern")]
        public Dictionary<string, SingerExternModel> GetSingerExtern { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "getSingerInfo")]
        public GetSingerInfoModel GetSingerInfo { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "getSongExtern")]
        public Dictionary<string, SongExternModel> GetSongExtern { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "getSongInfo")]
        public List<GetSongInfoItemModel> GetSongInfoItems { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "getSongidsFromAlbumid")]
        public List<int> GetSongidsFromAlbumidItems { get; set; }

        ///<summary>
        /// 国语
        /// </summary>
        [DataMember(Name = "language")]
        public string Language { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "radio_anchor")]
        public int Radio_Anchor { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "singerInfo")]
        public List<SingerInfoItemModel> SingerInfoItems { get; set; }

    }



    [DataContract]
    public class QQMusicAlbumJson
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

    }


}
