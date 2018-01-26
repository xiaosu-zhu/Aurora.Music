// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Aurora.Music.Core.Extension.Json;
using Aurora.Music.Core.Extension.Json.QQMusicAlbum;
using Aurora.Music.Core.Extension.Json.QQMusicArtist;
using Aurora.Music.Core.Extension.Json.QQMusicSong;
using Aurora.Shared.Extensions;
using Aurora.Shared.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Aurora.Music.Core.Extension
{
    public class OnlineMusicSearcher
    {
        private const string url = "https://c.y.qq.com/soso/fcgi-bin/client_search_cp";

        public static async Task<QQMusicSearchJson> SearchAsync(string keyword, int? page = 1, int? count = 10)
        {
            var queryString = HttpUtility.ParseQueryString(string.Empty);
            queryString["format"] = "json";
            queryString["aggr"] = "1";
            queryString["lossless"] = "1";
            queryString["cr"] = "1";
            queryString["new_json"] = "1";
            if (page.HasValue)
            {
                queryString["p"] = page.ToString();
            }
            else
            {
                queryString["p"] = "1";
            }
            if (count.HasValue)
            {
                queryString["n"] = count.ToString();
            }
            else
            {
                queryString["n"] = "10";
            }
            queryString["w"] = keyword;

            var result = await ApiRequestHelper.HttpGet(url, queryString);
            try
            {
                if (result != null)
                {
                    return JsonConvert.DeserializeObject<QQMusicSearchJson>(result);
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

        private const string fileUrl = "https://c.y.qq.com/base/fcgi-bin/fcg_musicexpress.fcg";
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

        public static string GenerateFileTypeByID(string media_ID, uint bitrate = 256)
        {
            var f = fileFormats.First(x => x.BitRate <= bitrate);
            return f.Format;
        }

        public static async Task<string> GenerateFileUriByID(string media_ID, uint bitrate = 256)
        {
            if ((DateTime.Now - stamp).TotalMinutes > 1 || !key.IsNullorEmpty())
            {
                guid = (Shared.Helpers.Tools.Random.Next() % 10000000000);
                stamp = DateTime.Now;
                var queryString = HttpUtility.ParseQueryString(string.Empty);
                queryString["json"] = "3";
                queryString["guid"] = guid.ToString();
                queryString["format"] = "json";
                var result = await ApiRequestHelper.HttpGet(fileUrl, queryString);
                if (result.IsNullorEmpty())
                {
                    return null;
                }
                var stage = JsonConvert.DeserializeObject<QQMusicFileJson>(result);
                if (stage.Code != 0)
                {
                    return null;
                }
                key = stage.Key;
            }

            var f = fileFormats.First(x => x.BitRate <= bitrate);

            var final = streamUrl + f.Prefix + media_ID + f.Format + "?vkey=" + key + "&guid=" + guid.ToString() + "&uid=0&fromtag=30";
            return final;
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
