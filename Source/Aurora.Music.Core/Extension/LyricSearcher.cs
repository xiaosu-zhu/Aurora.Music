// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Aurora.Music.Core.Extension.Json;
using Aurora.Shared.Extensions;
using Aurora.Shared.Helpers;
using Aurora.Shared.Helpers.Crypto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Windows.Storage;

namespace Aurora.Music.Core.Extension
{
    public class LyricSearcher
    {
        const string qqLyric = "https://api.darlin.me/music/lyric/{0}";

        public static async Task<string> GetSongLrcByID(string mid)
        {
            var s = await OnlineMusicSearcher.GetSongAsync(mid);
            if (s != null && !s.DataItems.IsNullorEmpty())
            {
                try
                {
                    string result = await ApiRequestHelper.HttpGet(string.Format(qqLyric, s.DataItems[0].Id));
                    result = result.Split('(')[1].TrimEnd(')');
                    if (result != null)
                        return HttpUtility.HtmlDecode(CryptoHelper.FromBase64(Newtonsoft.Json.JsonConvert.DeserializeAnonymousType(result, new { lyric = "" }).lyric));
                }
                catch (Exception)
                {
                    return null;
                }

            }
            return null;
        }

        public static async Task<IEnumerable<KeyValuePair<string, string>>> GetSongLrcListAsync(string title, string performer = null)
        {
            string res;
            if (!performer.IsNullorEmpty())
            {
                res = await ApiRequestHelper.HttpGet($"http://gecimi.com/api/lyric/{title}/{performer}");
            }
            else
            {
                res = await ApiRequestHelper.HttpGet($"http://gecimi.com/api/lyric/{title}");
            }
            if (res.IsNullorEmpty())
                return null;
            var json = Newtonsoft.Json.JsonConvert.DeserializeObject<GecimiJson>(res);
            if (json.Count > 0)
            {
                return json.ResultItems.Select(x => new KeyValuePair<string, string>(x.Song, x.Lrc));
            }
            return null;
        }

        public static async Task<string> SearchLrcLocalAsync(string title, string artists, string album)
        {
            var fileName = $"{title}-{artists ?? ""}-{artists ?? ""}";
            fileName = Shared.Utils.InvalidFileNameChars.Aggregate(fileName, (current, c) => current.Replace(c + "", "_"));

            var folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Lyrics", CreationCollisionOption.OpenIfExists);
            try
            {
                var file = await folder.TryGetItemAsync($"{fileName}.lrc");
                if (file == null)
                    return null;
                return await FileIO.ReadTextAsync(file as StorageFile);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static async Task SaveLrcLocalAsync(string title, string artists, string album, string result)
        {
            try
            {
                if (title.IsNullorEmpty() || result.IsNullorEmpty())
                {
                    return;
                }
                var fileName = $"{title}-{artists ?? ""}-{artists ?? ""}";
                fileName = Shared.Utils.InvalidFileNameChars.Aggregate(fileName, (current, c) => current.Replace(c + "", "_"));

                var folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Lyrics", CreationCollisionOption.OpenIfExists);

                var file = await folder.CreateFileAsync($"{fileName}.lrc", CreationCollisionOption.FailIfExists);
                await FileIO.WriteTextAsync(file, result);
            }
            catch (Exception)
            {

            }
        }
    }
}
