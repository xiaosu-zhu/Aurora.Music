// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Aurora.Music.Core.Models;
using Aurora.Shared.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;

namespace Aurora.Music.Core.Extension
{
    public static class ITunesSearcher
    {
        private const string queryUrl = "https://itunes.apple.com/search";
        private const string topUrl = "https://itunes.apple.com/{0}/rss/toppodcasts/limit={1}/genre={2}/json";
        private const string topAllUrl = "https://itunes.apple.com/{0}/rss/toppodcasts/limit={1}/json";
        private const string lookupUrl = "https://itunes.apple.com/lookup?id={0}";

        /// <summary>
        /// <see cref="https://affiliate.itunes.apple.com/resources/documentation/genre-mapping/"/>
        /// </summary>
        private static readonly List<KeyValuePair<string, string>> genres =
            new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("Arts", "1301"),
                new KeyValuePair<string, string>("Comedy", "1303"),
                new KeyValuePair<string, string>("Education", "1304"),
                new KeyValuePair<string, string>("Kids & Family", "1305"),
                new KeyValuePair<string, string>("Health", "1307"),
                new KeyValuePair<string, string>("TV & Film", "1309"),
                new KeyValuePair<string, string>("Music", "1310"),
                new KeyValuePair<string, string>("News & Politics", "1311"),
                new KeyValuePair<string, string>("Religion & Spirituality", "1314"),
                new KeyValuePair<string, string>("Science & Medicine", "1315"),
                new KeyValuePair<string, string>("Sports & Recreation", "1316"),
                new KeyValuePair<string, string>("Technology", "1318"),
                new KeyValuePair<string, string>("Business", "1321"),
                new KeyValuePair<string, string>("Games & Hobbies", "1323"),
                new KeyValuePair<string, string>("Society & Culture", "1324"),
                new KeyValuePair<string, string>("Government & Organizations", "1325")
            };

        public static async Task<List<OnlineMusicItem>> LookUp(string onlineAlbumID)
        {
            var res = await ApiRequestHelper.HttpGet(string.Format(lookupUrl, onlineAlbumID));
            var result = JsonConvert.DeserializeObject<ITunesSearchResult>(res);
            return result.results.ConvertAll(a => new OnlineMusicItem(a.trackName, a.artistName, a.feedUrl, null, a.feedUrl, a.artworkUrl100)
            {
                InnerType = MediaType.Podcast,
            });
        }

        internal static async Task<ITunesSearchResult> Search(string keyword)
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
            catch (Exception)
            {
                return null;
            }
        }

        public static async Task<ITunesTop> TopCharts(int count)
        {
            try
            {
                var res = await ApiRequestHelper.HttpGet(string.Format(topAllUrl, CultureInfoHelper.CurrentRegionISO, count));
                return JsonConvert.DeserializeObject<ITunesTop>(res);
            }
            catch (Exception)
            {
                return null;
            }
        }
        public static async Task<ITunesTop> TopGenres(string key, int count)
        {
            try
            {
                var res = await ApiRequestHelper.HttpGet(string.Format(topUrl, CultureInfoHelper.CurrentRegionISO, count, key));
                return JsonConvert.DeserializeObject<ITunesTop>(res);
            }
            catch (Exception)
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



    class ITunesTopGroup : List<ITunesTop>
    {
        public string Key { get; set; }
    }

    public class ITunesTop
    {
        public Feed feed { get; set; }
    }
    public class TopEntry
    {
        [JsonProperty("im:name")]
        public TopObject Name { get; set; }
        [JsonProperty("im:image")]
        public List<TopObject> Image { get; set; }
        [JsonProperty("summary")]
        public TopObject Summary { get; set; }
        [JsonProperty("id")]
        public TopObject ID { get; set; }
        [JsonProperty("im:artist")]
        public TopObject Artist { get; set; }
        [JsonProperty("category")]
        public TopObject Category { get; set; }
    }
    public class Feed
    {
        public List<TopEntry> entry { get; set; }
    }
    public class TopObject
    {
        public string label { get; set; }
        public Dictionary<string, string> attributes { get; set; }
    }
}
