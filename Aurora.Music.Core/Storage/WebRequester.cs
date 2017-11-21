using Aurora.Music.Core.Models.Json;
using Aurora.Shared.Extensions;
using Aurora.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Aurora.Music.Core.Storage
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
    }
}
