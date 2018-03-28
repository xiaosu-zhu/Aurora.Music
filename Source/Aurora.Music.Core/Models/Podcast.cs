// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Aurora.Music.Core.Extension;
using Aurora.Music.Core.Storage;
using Aurora.Music.Core.Tools;
using Aurora.Shared.Extensions;
using Aurora.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using Windows.Storage;

namespace Aurora.Music.Core.Models
{
    public class Podcast : PlayList
    {
        public string Author { get; set; }
        public string XMLUrl { get; internal set; }
        public string XMLPath { get; set; }
        public DateTime LastUpdate { get; set; }
        public bool Subscribed { get; set; }

        public Podcast() { }

        public async Task<bool> FindUpdated()
        {
            string resXML = string.Empty;
            bool b = false;
            for (int i = 0; i < 3; i++)
            {
                resXML = await ApiRequestHelper.HttpGet(XMLUrl);
                try
                {
                    FindUpdated(resXML);
                    b = true;
                    break;
                }
                catch (Exception)
                {
                }
            }
            if (!b) return false;
            try
            {
                var folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Podcasts", CreationCollisionOption.OpenIfExists);
                var file = await folder.CreateFileAsync($"{XMLPath}.xml", CreationCollisionOption.OpenIfExists);

                await FileIO.WriteTextAsync(file, resXML);
            }
            catch (Exception)
            {

            }
            await SaveAsync();
            return Count > 0;
        }

        public static async Task<IEnumerable<OnlineMusicItem>> GetiTunesTop(int count)
        {
            var res = await ITunesSearcher.TopCharts(count);
            return res.feed.entry.Select(a => new OnlineMusicItem(a.Name.label, a.Summary?.label, a.Artist.label, null, a.ID.attributes["im:id"], a.Image[2].label));
        }

        private void FindUpdated(string resXML)
        {
            var ment = new XmlDocument(); ment.LoadXml(resXML);

            XmlNamespaceManager ns = new XmlNamespaceManager(ment.NameTable);
            ns.AddNamespace("itunes", "http://www.itunes.com/dtds/podcast-1.0.dtd");

            Title = ment.SelectSingleNode("/rss/channel/title").InnerText;
            Description = ment.SelectSingleNode("/rss/channel/description").InnerText;
            HeroArtworks = new string[] { (ment.SelectSingleNode("/rss/channel/image/url") ?? ment.SelectSingleNode($"/rss/channel/itunes:image/@href", ns))?.InnerText ?? Consts.BlackPlaceholder };
            Author = ment.SelectSingleNode($"/rss/channel/itunes:author", ns)?.InnerText;

            var items = ment.SelectNodes("/rss/channel/item");
            var last = LastUpdate;
            for (int i = 0; i < items.Count; i++)
            {
                var d = DateTime.Parse(items[i].SelectSingleNode("./pubDate")?.InnerText ?? DateTime.Now.ToString());
                if (LastUpdate < d)
                {
                    last = d;
                }
                else
                {
                    continue;
                }
                Add(new Song()
                {
                    Title = items[i].SelectSingleNode("./title").InnerText,
                    FilePath = items[i].SelectSingleNode("./enclosure/@url").InnerText,
                    Performers = new string[] { (items[i].SelectSingleNode("./author") ?? items[i].SelectSingleNode($"./itunes:author", ns))?.InnerText },
                    AlbumArtists = new string[] { (items[i].SelectSingleNode("./author") ?? items[i].SelectSingleNode($"./itunes:author", ns))?.InnerText },
                    Duration = (items[i].SelectSingleNode($"./itunes:duration", ns)?.InnerText ?? "0:00").ParseDuration(),
                    Album = HttpUtility.HtmlDecode((items[i].SelectSingleNode("./description")?.InnerText ?? Title)).Replace("<br>", Environment.NewLine),
                    IsOnline = true,
                    OnlineUri = new Uri(items[i].SelectSingleNode("./enclosure/@url").InnerText),
                    OnlineID = items[i].SelectSingleNode("./guid").InnerText,
                    IsPodcast = true,
                    PubDate = d,
                    PicturePath = items[i].SelectSingleNode("./itunes:image/@href", ns)?.InnerText ?? HeroArtworks[0]
                });
            }
            Sort((a, s) => -1 * (a.PubDate.CompareTo(s.PubDate)));
            LastUpdate = last;
        }

        public Podcast(PODCAST p)
        {
            ID = p.ID;
            Title = p.Title;
            Description = p.Description;
            Author = p.Author;
            XMLUrl = p.XMLUrl;
            XMLPath = p.XMLPath;
            Subscribed = p.Subscribed;
            LastUpdate = p.LastUpdate;
        }

        internal static async Task<Podcast> BuildFromXMLAsync(string resXML, string XMLUrl)
        {
            var a = new Podcast();
            await a.ReadXML(resXML);

            var p = await SQLOperator.Current().TryGetPODCAST(XMLUrl);

            var fileName = $"{a.Title}-{Guid.NewGuid().ToString()}";
            if (p != null)
            {
                fileName = p.XMLPath;
            }
            else
            {

            }
            var folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Podcasts", CreationCollisionOption.OpenIfExists);

            fileName = Shared.Utils.InvalidFileNameChars.Aggregate(fileName, (current, c) => current.Replace(c + "", "_"));

            var file = await folder.CreateFileAsync($"{fileName}.xml", CreationCollisionOption.OpenIfExists);
            await FileIO.WriteTextAsync(file, resXML);
            a.XMLUrl = XMLUrl;
            a.XMLPath = fileName;
            a.ID = p?.ID ?? default(int);
            a.Subscribed = p?.Subscribed ?? false;


            await a.SaveAsync();

            return a;
        }

        public async static Task<Podcast> ReadFromLocalAsync(int iD)
        {
            var p = await SQLOperator.Current().GetItemByIDAsync<PODCAST>(iD);
            var fileName = p.XMLPath;
            try
            {
                var folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Podcasts", CreationCollisionOption.OpenIfExists);
                var file = await folder.TryGetItemAsync($"{fileName}.xml");
                if (file == null) return null;
                var str = await FileIO.ReadTextAsync(file as StorageFile);
                var a = new Podcast
                {
                    XMLUrl = p.XMLUrl,
                    XMLPath = p.XMLPath,
                    Subscribed = p.Subscribed
                };
                await a.ReadXML(str);
                return a;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task Refresh()
        {
            var resXML = await ApiRequestHelper.HttpGet(XMLUrl);

            await ReadXML(resXML);

            try
            {
                var folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Podcasts", CreationCollisionOption.OpenIfExists);
                var file = await folder.CreateFileAsync($"{XMLPath}.xml", CreationCollisionOption.OpenIfExists);

                await FileIO.WriteTextAsync(file, resXML);
            }
            catch (Exception)
            {

            }

            await SaveAsync();
        }

        public static async Task<Podcast> GetiTunesPodcast(string url)
        {
            var resXML = await ApiRequestHelper.HttpGet(url);
            return await BuildFromXMLAsync(resXML, url);
        }

        public override async Task<int> SaveAsync()
        {
            return await SQLOperator.Current().UpdatePodcastAsync(new PODCAST(this));
        }

        private static readonly Dictionary<string, string> _timeZones = new Dictionary<string, string>() {
            {"ACDT", "+10:30"},
            {"ACST", "+09:30"},
            {"ADT", "-03:00"},
            {"AEDT", "+11:00"},
            {"AEST", "+10:00"},
            {"AHDT", "-09:00"},
            {"AHST", "-10:00"},
            {"AST", "-04:00"},
            {"AT", "-02:00"},
            {"AWDT", "+09:00"},
            {"AWST", "+08:00"},
            {"BAT", "+03:00"},
            {"BDST", "+02:00"},
            {"BET", "-11:00"},
            {"BST", "-03:00"},
            {"BT", "+03:00"},
            {"BZT2", "-03:00"},
            {"CADT", "+10:30"},
            {"CAST", "+09:30"},
            {"CAT", "-10:00"},
            {"CCT", "+08:00"},
            {"CDT", "-05:00"},
            {"CED", "+02:00"},
            {"CET", "+01:00"},
            {"CEST", "+02:00"},
            {"CST", "-06:00"},
            {"EAST", "+10:00"},
            {"EDT", "-04:00"},
            {"EED", "+03:00"},
            {"EET", "+02:00"},
            {"EEST", "+03:00"},
            {"EST", "-05:00"},
            {"FST", "+02:00"},
            {"FWT", "+01:00"},
            {"GMT", "+00:00"},
            {"GST", "+10:00"},
            {"HDT", "-09:00"},
            {"HST", "-10:00"},
            {"IDLE", "+12:00"},
            {"IDLW", "-12:00"},
            {"IST", "+05:30"},
            {"IT", "+03:30"},
            {"JST", "+09:00"},
            {"JT", "+07:00"},
            {"MDT", "-06:00"},
            {"MED", "+02:00"},
            {"MET", "+01:00"},
            {"MEST", "+02:00"},
            {"MEWT", "+01:00"},
            {"MST", "-07:00"},
            {"MT", "+08:00"},
            {"NDT", "-02:30"},
            {"NFT", "-03:30"},
            {"NT", "-11:00"},
            {"NST", "+06:30"},
            {"NZ", "+11:00"},
            {"NZST", "+12:00"},
            {"NZDT", "+13:00"},
            {"NZT", "+12:00"},
            {"PDT", "-07:00"},
            {"PST", "-08:00"},
            {"ROK", "+09:00"},
            {"SAD", "+10:00"},
            {"SAST", "+09:00"},
            {"SAT", "+09:00"},
            {"SDT", "+10:00"},
            {"SST", "+02:00"},
            {"SWT", "+01:00"},
            {"USZ3", "+04:00"},
            {"USZ4", "+05:00"},
            {"USZ5", "+06:00"},
            {"USZ6", "+07:00"},
            {"UT", "-00:00"},
            {"UTC", "-00:00"},
            {"UZ10", "+11:00"},
            {"WAT", "-01:00"},
            {"WET", "-00:00"},
            {"WST", "+08:00"},
            {"YDT", "-08:00"},
            {"YST", "-09:00"},
            {"ZP4", "+04:00"},
            {"ZP5", "+05:00"},
            {"ZP6", "+06:00"}
        };

        DateTime ParseRFC1123(string datetime)
        {
            foreach (var key in _timeZones)
            {
                if (datetime.EndsWith($" {key.Key}"))
                {
                    datetime = datetime.Replace(key.Key, key.Value);
                    break;
                }
            }
            return DateTime.Parse(datetime);
        }

        public async Task ReadXML(string res)
        {
            Clear();
            var ment = new XmlDocument(); ment.LoadXml(res);

            XmlNamespaceManager ns = new XmlNamespaceManager(ment.NameTable);
            ns.AddNamespace("itunes", "http://www.itunes.com/dtds/podcast-1.0.dtd");

            Title = ment.SelectSingleNode("/rss/channel/title").InnerText;
            Description = ment.SelectSingleNode("/rss/channel/description").InnerText;
            HeroArtworks = new string[] { (ment.SelectSingleNode("/rss/channel/image/url") ?? ment.SelectSingleNode($"/rss/channel/itunes:image/@href", ns))?.InnerText ?? Consts.BlackPlaceholder };
            Author = ment.SelectSingleNode($"/rss/channel/itunes:author", ns)?.InnerText;

            var folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Podcasts", CreationCollisionOption.OpenIfExists);

            var items = ment.SelectNodes("/rss/channel/item");

            for (int i = 0; i < items.Count; i++)
            {

                var d = ParseRFC1123(items[i].SelectSingleNode("./pubDate")?.InnerText ?? DateTime.Now.ToString());
                if (LastUpdate < d)
                {
                    LastUpdate = d;
                }
                var s = new Song()
                {
                    Title = items[i].SelectSingleNode("./title").InnerText,
                    FilePath = items[i].SelectSingleNode("./enclosure/@url").InnerText,
                    Performers = new string[] { (items[i].SelectSingleNode("./author") ?? items[i].SelectSingleNode($"./itunes:author", ns))?.InnerText },
                    AlbumArtists = new string[] { (items[i].SelectSingleNode("./author") ?? items[i].SelectSingleNode($"./itunes:author", ns))?.InnerText },
                    Duration = (items[i].SelectSingleNode($"./itunes:duration", ns)?.InnerText ?? "0:00").ParseDuration(),
                    Album = HttpUtility.HtmlDecode((items[i].SelectSingleNode("./description")?.InnerText ?? Title)).Replace("<br>", Environment.NewLine),
                    IsOnline = true,
                    OnlineUri = new Uri(items[i].SelectSingleNode("./enclosure/@url").InnerText),
                    OnlineID = items[i].SelectSingleNode("./guid").InnerText,
                    IsPodcast = true,
                    PubDate = d,
                    PicturePath = items[i].SelectSingleNode("./itunes:image/@href", ns)?.InnerText ?? HeroArtworks[0]
                };
                var fileName = s.GetFileName();
                var file = await folder.TryGetItemAsync(fileName);
                if (file != null)
                {
                    s.IsOnline = false;
                    s.FilePath = file.Path;
                }
                Add(s);
            }
            Sort((a, s) => -1 * (a.PubDate.CompareTo(s.PubDate)));
        }

        public async static Task<List<OnlineMusicItem>> SearchPodcasts(string text)
        {
            var result = await ITunesSearcher.Search(text);
            if (result == null)
            {
                return null;
            }
            return result.results.ConvertAll(a => new OnlineMusicItem(a.trackName, a.artistName, a.feedUrl, null, a.feedUrl, a.artworkUrl100)
            {
                InnerType = MediaType.Podcast,
            });
        }
    }
}
