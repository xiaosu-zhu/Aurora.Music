using Aurora.Music.Core.Models;
using Aurora.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Windows.Data.Xml.Dom;
using Windows.Foundation.Collections;

namespace Aurora.Music.Core.Extension
{
    public static class LastfmSearcher
    {
        private const string API_KEY = KEY.LASTFM;

        private const string API_URL = "http://ws.audioscrobbler.com/2.0/";

        public static async Task<AlbumInfo> GetAlbumInfo(string album, string artist)
        {
            try
            {
                var param = HttpUtility.ParseQueryString(string.Empty);
                param["method"] = "album.getinfo";
                param["api_key"] = API_KEY;
                param["artist"] = artist;
                param["album"] = album;
                param["lang"] = CultureInfoHelper.GetCurrentCulture().TwoLetterISOLanguageName;
                param["autocorrect"] = "1";
                var result = await ApiRequestHelper.HttpGet(API_URL, param);
                var xml = new XmlDocument(); xml.LoadXml(result);
                if (xml.SelectSingleNode("/lfm/@status").InnerText == "ok")
                {
                    var info = new AlbumInfo()
                    {
                        Name = (xml.SelectSingleNode("/lfm/album/name")).InnerText,
                        Artist = (xml.SelectSingleNode("/lfm/album/artist")).InnerText,
                    };
                    var imageNode = xml.SelectSingleNode("/lfm/album/image[@size='']");
                    if (imageNode == null)
                    {

                    }
                    else
                    {
                        if (Uri.TryCreate(imageNode.InnerText, UriKind.Absolute, out var u))
                        {
                            info.AltArtwork = u;
                        }
                    }

                    var list = new List<string>();

                    foreach (var item in xml.SelectNodes("/lfm/album/tags/tag"))
                    {
                        list.Add($"[{item.SelectSingleNode("./name").InnerText}]({item.SelectSingleNode("./url").InnerText})");
                    }

                    if (list.Count == 0)
                    {
                        list.Add("None");
                    }

                    var listener = xml.SelectSingleNode("/lfm/album/listeners").InnerText;
                    var played = xml.SelectSingleNode("/lfm/album/playcount").InnerText;

                    var wikiNode = (xml.SelectSingleNode("/lfm/album/wiki/content"));
                    if (wikiNode == null)
                    {
                        info.Description = $"# {info.Name} by {info.Artist}\r\n\r\n{listener} listeners and played {played} times.\r\n\r\n## Tags\r\n\r\n\r\n{string.Join(", ", list)}.\r\n\r\n---\r\n\r\nSee [Last.FM]({xml.SelectSingleNode("/lfm/album/url").InnerText}).";
                    }
                    else
                    {
                        info.Description = $"# {info.Name} by {info.Artist}\r\n\r\n{listener} listeners and played {played} times.\r\n\r\n## Tags\r\n\r\n\r\n{string.Join(", ", list)}.\r\n\r\n---\r\n\r\n## Wiki\r\n{wikiNode.InnerText.Replace("\n", "\r\n\r\n")}\r\n\r\n---\r\n\r\nSee [Last.FM]({xml.SelectSingleNode("/lfm/album/url").InnerText}).";
                    }
                    return info;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static async Task<Artist> GetArtistInfo(string art)
        {
            try
            {
                var param = HttpUtility.ParseQueryString(string.Empty);
                param["method"] = "artist.getinfo";
                param["api_key"] = API_KEY;
                param["artist"] = art;
                param["lang"] = CultureInfoHelper.GetCurrentCulture().TwoLetterISOLanguageName;
                param["autocorrect"] = "1";
                var result = await ApiRequestHelper.HttpGet(API_URL, param);
                var xml = new XmlDocument(); xml.LoadXml(result);
                if (xml.SelectSingleNode("/lfm/@status").InnerText == "ok")
                {
                    var artist = new Artist
                    {
                        Name = xml.SelectSingleNode("/lfm/artist/name").InnerText
                    };
                    var imageNode = xml.SelectSingleNode("/lfm/artist/image[@size='']");
                    if (imageNode == null)
                    {

                    }
                    else
                    {
                        if (Uri.TryCreate(imageNode.InnerText, UriKind.Absolute, out var u))
                        {
                            artist.AvatarUri = u;
                        }
                    }

                    var list = new List<string>();

                    foreach (var item in xml.SelectNodes("/lfm/artist/tags/tag"))
                    {
                        list.Add($"[{item.SelectSingleNode("./name").InnerText}]({item.SelectSingleNode("./url").InnerText})");
                    }

                    if (list.Count == 0)
                    {
                        list.Add("None");
                    }

                    var listener = xml.SelectSingleNode("/lfm/artist/stats/listeners").InnerText;
                    var played = xml.SelectSingleNode("/lfm/artist/stats/playcount").InnerText;

                    var bioNode = xml.SelectSingleNode("/lfm/artist/bio");
                    if (bioNode != null)
                    {
                        artist.Description = $"# {artist.Name}\r\n\r\n{listener} listeners and played {played} times.\r\n\r\n## Tags\r\n\r\n\r\n{string.Join(", ", list)}.\r\n\r\n---\r\n\r\n## Wiki\r\n{bioNode.SelectSingleNode("./content").InnerText.Replace("\n", "\r\n\r\n")}\r\n\r\n---\r\n\r\nSee [Last.FM]({xml.SelectSingleNode("/lfm/artist/url").InnerText}).";
                    }
                    else
                    {
                        artist.Description = $"# {artist.Name}\r\n\r\n{listener} listeners and played {played} times.\r\n\r\n## Tags\r\n\r\n\r\n{string.Join(", ", list)}.\r\n\r\n---\r\n\r\nSee [Last.FM]({xml.SelectSingleNode("/lfm/artist/url").InnerText}).";
                    }

                    return artist;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static object ReadXml(string result)
        {
            var xml = new XmlDocument();
            xml.LoadXml(result);

            return null;
        }
    }
}
