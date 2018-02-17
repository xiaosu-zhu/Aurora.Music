// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Aurora.Music.Core.Extension;
using Aurora.Music.Core.Storage;
using Aurora.Shared.Extensions;
using Aurora.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
            var resXML = await ApiRequestHelper.HttpGet(XMLUrl);
            FindUpdated(resXML);
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

            for (int i = 0; i < items.Count; i++)
            {
                var d = DateTime.Parse(items[i].SelectSingleNode("./pubDate")?.InnerText ?? DateTime.Now.ToString());
                if (LastUpdate < d)
                {
                    LastUpdate = d;
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
        }

        public Podcast(PODCAST p)
        {
            XMLUrl = p.XMLUrl;
            XMLPath = p.XMLPath;
            Subscribed = p.Subscribed;
            LastUpdate = p.LastUpdate;
        }

        internal static async Task<Podcast> BuildFromXMLAsync(string resXML, string XMLUrl)
        {
            var a = new Podcast();
            a.ReadXML(resXML);

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
                var file = await folder.GetFileAsync($"{fileName}.xml");
                var str = await FileIO.ReadTextAsync(file);
                var a = new Podcast
                {
                    XMLUrl = p.XMLUrl,
                    XMLPath = p.XMLPath,
                    Subscribed = p.Subscribed
                };
                a.ReadXML(str);
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
            ReadXML(resXML);
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

        public void ReadXML(string res)
        {
            var ment = new XmlDocument(); ment.LoadXml(res);

            XmlNamespaceManager ns = new XmlNamespaceManager(ment.NameTable);
            ns.AddNamespace("itunes", "http://www.itunes.com/dtds/podcast-1.0.dtd");

            Title = ment.SelectSingleNode("/rss/channel/title").InnerText;
            Description = ment.SelectSingleNode("/rss/channel/description").InnerText;
            HeroArtworks = new string[] { (ment.SelectSingleNode("/rss/channel/image/url") ?? ment.SelectSingleNode($"/rss/channel/itunes:image/@href", ns))?.InnerText ?? Consts.BlackPlaceholder };
            Author = ment.SelectSingleNode($"/rss/channel/itunes:author", ns)?.InnerText;

            var items = ment.SelectNodes("/rss/channel/item");

            for (int i = 0; i < items.Count; i++)
            {
                var d = DateTime.Parse(items[i].SelectSingleNode("./pubDate")?.InnerText ?? DateTime.Now.ToString());
                if (LastUpdate < d)
                {
                    LastUpdate = d;
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
