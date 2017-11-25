using Aurora.Music.Core.Extension.Json;
using Aurora.Shared.Extensions;
using Aurora.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Aurora.Music.Core.Extension
{
    public class LyricSearcher
    {
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

        public static async Task<string> SearchLrcLocalAsync(string title, string artists)
        {
            var fileName = Shared.Utils.InvalidFileNameChars.Aggregate(title, (current, c) => current.Replace(c + "", "_"));
            fileName += artists.IsNullorEmpty() ? "" : $"-{Shared.Utils.InvalidFileNameChars.Aggregate(artists, (current, c) => current.Replace(c + "", "_"))}";

            var folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Lyrics", CreationCollisionOption.OpenIfExists);
            try
            {
                var file = await folder.GetFileAsync($"{fileName}.lrc");
                if (file == null)
                    return null;
                return await FileIO.ReadTextAsync(file);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static async Task SaveLrcLocalAsync(string title, string artists, string result)
        {
            var fileName = Shared.Utils.InvalidFileNameChars.Aggregate(title, (current, c) => current.Replace(c + "", "_"));
            fileName += artists.IsNullorEmpty() ? "" : $"-{Shared.Utils.InvalidFileNameChars.Aggregate(artists, (current, c) => current.Replace(c + "", "_"))}";

            var folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Lyrics", CreationCollisionOption.OpenIfExists);
            try
            {
                var file = await folder.CreateFileAsync($"{fileName}.lrc", CreationCollisionOption.FailIfExists);
                await FileIO.WriteTextAsync(file, result);
            }
            catch (Exception)
            {

            }
        }
    }
}
