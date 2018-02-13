// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Aurora.Music.Core.Models;
using Aurora.Shared.Helpers;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;

namespace Aurora.Music.Core.Extension
{
    class ITunesSearcher
    {
        private const string queryUrl = "https://itunes.apple.com/search";
        private const string topUrl = "https://itunes.apple.com/{0}/rss/toppodcasts/limit={1}/genre={2}/json";

        /// <summary>
        /// <see cref="https://affiliate.itunes.apple.com/resources/documentation/genre-mapping/"/>
        /// </summary>
        private static readonly Dictionary<string, string> genres =
            new Dictionary<string, string>()
            {
                ["Arts"] = "1301",
                ["Comedy"] = "1303",
                ["Education"] = "1304",
                ["Kids & Family"] = "1305",
                ["Health"] = "1307",
                ["TV & Film"] = "1309",
                ["Music"] = "1310",
                ["News & Politics"] = "1311",
                ["Religion & Spirituality"] = "1314",
                ["Science & Medicine"] = "1315",
                ["Sports & Recreation"] = "1316",
                ["Technology"] = "1318",
                ["Business"] = "1321",
                ["Games & Hobbies"] = "1323",
                ["Society & Culture"] = "1324",
                ["Government & Organizations"] = "1325"
            };

        public static async Task<ITunesSearchResult> Search(string keyword)
        {
            var parameter = HttpUtility.ParseQueryString("");
            parameter["term"] = keyword;
            parameter["country"] = CultureInfoHelper.CurrentRegionISO;
            parameter["media"] = "podcast";
            parameter["limit"] = "10";
            try
            {
                var res = await ApiRequestHelper.HttpGet(queryUrl, parameter);
                return JsonConvert.DeserializeObject<ITunesSearchResult>(res);
            }
            catch (System.Exception)
            {
                return null;
            }
        }
    }

    class ITunesSearchResult
    {
        public int resultCount { get; set; }
        public List<Result> results { get; set; }
    }
    class Result
    {
        public string wrapperType { get; set; }
        public string kind { get; set; }
        public string artistId { get; set; }
        public string collectionId { get; set; }
        public string trackId { get; set; }
        public string artistName { get; set; }
        public string collectionName { get; set; }
        public string trackName { get; set; }
        public string collectionCensoredName { get; set; }
        public string trackCensoredName { get; set; }
        public string artistViewUrl { get; set; }
        public string collectionViewUrl { get; set; }
        public string feedUrl { get; set; }
        public string trackViewUrl { get; set; }
        public string artworkUrl100 { get; set; }
    }
}
