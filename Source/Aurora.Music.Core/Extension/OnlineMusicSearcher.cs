// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Aurora.Music.Core.Extension.Json;
using Aurora.Music.Core.Extension.Json.QQMusicAlbum;
using Aurora.Music.Core.Extension.Json.QQMusicArtist;
using Aurora.Music.Core.Extension.Json.QQMusicSong;
using Aurora.Music.Core.Models;
using Aurora.Shared.Extensions;
using Aurora.Shared.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Aurora.Music.Core.Extension
{
    public class OnlineMusicSearcher
    {
        private const string seachUrl = @"http://i.y.qq.com/s.music/fcgi-bin/search_for_qq_cp?g_tk=938407465&uin=0&format=jsonp&inCharset=utf-8&outCharset=utf-8&notice=0&platform=h5&needNewCode=1&w={0}&zhidaqu=1&catZhida=1&t=0&flag=1&ie=utf-8&sem=1&aggr=0&perpage=100&n={1}&p=0&remoteplace=txt.mqq.all&_=1459991037831&jsonpCallback=jsonp4";

        public static async Task<QQMusicSearchJson> SearchAsync(string keyword)
        {
            var url = string.Format(seachUrl, keyword, Settings.Current.PreferredSearchCount.ToString());
            var header = new Dictionary<string, string>()
            {
                ["Accept-Language"] = "zh-CN",
                ["Accept"] = "application/json, text/plain, */*",
                ["Referer"] = "http://y.qq.com/",
                ["Origin"] = "http://y.qq.com/",
            };
            var result = await ApiRequestHelper.HttpGet(url, addHeader: header);
            try
            {
                if (result != null)
                {
                    var length = result.Length - 8;
                    return JsonConvert.DeserializeObject<QQMusicSearchJson>(result.Substring(7, length));
                }
                return null;
            }
            catch (JsonReaderException)
            {
                return null;
            }
        }

        private const string songUrl = "https://c.y.qq.com/v8/fcg-bin/fcg_play_single_song.fcg";

        public static async Task<QQMusicSongJson> GetSongAsync(string song_mid)
        {
            var queryString = HttpUtility.ParseQueryString(string.Empty);

            queryString["songmid"] = song_mid;
            queryString["platform"] = "yqq";
            queryString["format"] = "json";
            var response = await ApiRequestHelper.HttpGet(songUrl, queryString);
            if (response.IsNullorEmpty())
            {
                return null;
            }
            try
            {
                return JsonConvert.DeserializeObject<QQMusicSongJson>(response);
            }
            catch (JsonReaderException)
            {
                return null;
            }
        }

        private const string albumUrl = "https://c.y.qq.com/v8/fcg-bin/fcg_v8_album_detail_cp.fcg";

        public static async Task<QQMusicAlbumJson> GetAlbumAsync(string mid)
        {
            var queryString = HttpUtility.ParseQueryString(string.Empty);

            queryString["albummid"] = mid;
            queryString["platform"] = "mac";
            queryString["format"] = "json";
            queryString["newsong"] = "1";
            var response = await ApiRequestHelper.HttpGet(albumUrl, queryString);
            if (response.IsNullorEmpty())
            {
                return null;
            }
            try
            {
                return JsonConvert.DeserializeObject<QQMusicAlbumJson>(response);
            }
            catch (JsonReaderException)
            {
                return null;
            }
        }

        public static Task<QQMusicArtistJson> GetArtistAsync(string v)
        {
            throw new NotImplementedException();
        }

        private const string fileUrl = @"https://u.y.qq.com/cgi-bin/musicu.fcg?loginUin=0&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq.json&needNewCode=0&data=%7B%22req_0%22%3A%7B%22module%22%3A%22vkey.GetVkeyServer%22%2C%22method%22%3A%22CgiGetVkey%22%2C%22param%22%3A%7B%22guid%22%3A%2210000%22%2C%22songmid%22%3A%5B%22{0}%22%5D%2C%22songtype%22%3A%5B0%5D%2C%22uin%22%3A%220%22%2C%22loginflag%22%3A1%2C%22platform%22%3A%2220%22%7D%7D%2C%22comm%22%3A%7B%22uin%22%3A0%2C%22format%22%3A%22json%22%2C%22ct%22%3A20%2C%22cv%22%3A0%7D%7D";
        private const string streamUrl = "https://dl.stream.qqmusic.qq.com/";

        private static readonly List<QQMusicFileFormat> fileFormats = new List<QQMusicFileFormat>()
        {
            new QQMusicFileFormat(320u, "M800", ".mp3"),
            new QQMusicFileFormat(192u, "C600", ".m4a"),
            new QQMusicFileFormat(128u, "M500", ".mp3"),
            new QQMusicFileFormat(96u, "C400", ".mp3"),
            new QQMusicFileFormat(48u, "C200", ".mp3"),
        };

        private static DateTime stamp = DateTime.MinValue;
        private static long guid;
        private static string key;

        public static string GenerateFileTypeByID(string media_ID, uint bitrate)
        {
            var f = fileFormats.First(x => x.BitRate <= bitrate);
            return f.Format;
        }

        public static async Task<string> GenerateFileUriByID(string media_ID, uint bitrate)
        {
            var result = await ApiRequestHelper.HttpGet(string.Format(fileUrl, media_ID));
            if (result.IsNullorEmpty())
            {
                return null;
            }
            var data = JsonConvert.DeserializeObject<QQMusicFileJson>(result);
            try
            {
                var final = data.req_0.data.Sip[0] + data.req_0.data.MidUrlInfo[0].purl;
                return final;
            }
            catch
            {
                return null;
            }
        }

        class QQMusicFileFormat
        {
            public uint BitRate { get; }
            public string Prefix { get; }
            public string Format { get; }
            public QQMusicFileFormat(uint bitrate, string prefix, string format)
            {
                BitRate = bitrate;
                Prefix = prefix;
                Format = format;
            }
        }

        private const string picUrl = "https://y.gtimg.cn/music/photo_new/T002R500x500M000{0}.jpg?max_age=2592000";

        public static string GeneratePicturePathByID(string v)
        {
            return string.Format(picUrl, v);
        }
    }
}
