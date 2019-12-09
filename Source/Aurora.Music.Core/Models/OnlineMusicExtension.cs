// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Aurora.Shared.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppExtensions;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;

namespace Aurora.Music.Core.Models
{
    public class OnlineMusicExtension : Extension
    {
        public OnlineMusicExtension(AppExtension ext, IPropertySet properties) : base(ext, properties)
        {
        }

        public override async Task<object> ExecuteAsync(ValueSet parameters)
        {

            if (_serviceName.IsNullorEmpty())
            {
                throw new InvalidProgramException("Extension is not a service");
            }
            try
            {
                // do app service call
                using (var connection = new AppServiceConnection())
                {
                    // service name was in properties
                    connection.AppServiceName = _serviceName;

                    // package Family Name is in the extension
                    connection.PackageFamilyName = this.AppExtension.Package.Id.FamilyName;

                    // open connection
                    AppServiceConnectionStatus status = await connection.OpenAsync();
                    if (status != AppServiceConnectionStatus.Success)
                    {
                        throw new InvalidOperationException(status.ToString());
                    }
                    else
                    {
                        // send request to service
                        // get response
                        AppServiceResponse response = await connection.SendMessageAsync(parameters);
                        if (response.Status == AppServiceResponseStatus.Success)
                        {
                            ValueSet message = response.Message as ValueSet;
                            if (message.ContainsKey("status") && (int)message["status"] == 1)
                            {
                                if (message.ContainsKey("search_result") && message["search_result"] is string s)
                                {
                                    return GetGenericMusicItem(s);
                                }
                                if (message.ContainsKey("song_result") && message["song_result"] is string t)
                                {
                                    return GetOnlineSong(t);
                                }
                                if (message.ContainsKey("album_result") && message["album_result"] is string r)
                                {
                                    return GetAlbum(r, message["songs"] as string, message["album_artists"] as string);
                                }
                                if (message.ContainsKey("playlists") && message["playlists"] is string p)
                                {
                                    return GetPlayLists(p);
                                }
                                if (message.ContainsKey("playlist_result") && message["playlist_result"] is string a)
                                {
                                    return GetAlbum(a, message["songs"] as string, message["album_artists"] as string);
                                }
                            }
                        }
                    }
                }
                return null;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private List<OnlineMusicItem> GetPlayLists(string p)
        {
            var playlist = JsonConvert.DeserializeObject<PropertySet[]>(p);
            return playlist.Select(a => new OnlineMusicItem(a["title"] as string, a["description"] as string, a["addtional"] as string, null, a["id"] as string, a["picture_path"] as string)).ToList();
        }

        private Album GetPlaylist(string r, string s, string art)
        {
            var set = JsonConvert.DeserializeObject<PropertySet>(r);
            var songs = JsonConvert.DeserializeObject<PropertySet[]>(s);
            var artists = JsonConvert.DeserializeObject<PropertySet[]>(art);
            var a = new Album
            {
                Name = set["name"] as string,
                Decsription = set["description"] as string,
                Year = Convert.ToUInt32(set["year"]),
                IsOnline = true,
                OnlineIDs = songs.Select(x => x["id"] as string).ToArray(),
                AlbumArtists = artists.Select(x => x["name"] as string).ToArray(),
                DiscCount = Convert.ToUInt32(set["disc_count"]),
                TrackCount = Convert.ToUInt32(set["track_count"]),
                PicturePath = set["picture_path"] as string,
                Genres = (set["genres"] as Newtonsoft.Json.Linq.JArray).Select(x => x.ToString()).ToArray(),
            };
            a.SongItems = new List<Song>();
            foreach (var item in songs)
            {
                a.SongItems.Add(new Song()
                {
                    Title = item["title"] as string,
                    Album = item["album"] as string,
                    Performers = (item["performers"] as Newtonsoft.Json.Linq.JArray).Select(x => x.ToString()).ToArray(),
                    AlbumArtists = (item["album_artists"] as Newtonsoft.Json.Linq.JArray).Select(x => x.ToString()).ToArray(),
                    PicturePath = item["picture_path"] as string,
                    OnlineID = item["id"] as string,
                    OnlineAlbumID = item["album_id"] as string,
                    IsOnline = true,
                    IsAvaliable = !string.IsNullOrEmpty(item["file_url"] as string),
                    BitRate = Convert.ToUInt32(item["bit_rate"]),
                    Year = Convert.ToUInt32(item["year"]),
                    OnlineUri = new Uri(item["file_url"] as string),
                    FilePath = item["file_url"] as string,
                    Track = Convert.ToUInt32(item["track"]),
                    Duration = TimeSpan.Parse(item["duration"] as string),
                    FileType = item["file_type"] as string
                });
            }

            a.OnlineArtistIDs = artists.Select(x => x["id"] as string).ToArray();

            // TODO: Optional Properties

            return a;
        }

        private Album GetAlbum(string r, string s, string art)
        {
            var set = JsonConvert.DeserializeObject<PropertySet>(r);
            var songs = JsonConvert.DeserializeObject<PropertySet[]>(s);
            var artists = JsonConvert.DeserializeObject<PropertySet[]>(art);
            var a = new Album
            {
                Name = set["name"] as string,
                Decsription = set["description"] as string,
                Year = Convert.ToUInt32(set["year"]),
                IsOnline = true,
                OnlineIDs = songs.Select(x => x["id"] as string).ToArray(),
                AlbumArtists = artists.Select(x => x["name"] as string).ToArray(),
                DiscCount = Convert.ToUInt32(set["disc_count"]),
                TrackCount = Convert.ToUInt32(set["track_count"]),
                PicturePath = set["picture_path"] as string,
                Genres = (set["genres"] as Newtonsoft.Json.Linq.JArray).Select(x => x.ToString()).ToArray(),
            };
            a.SongItems = new List<Song>();
            foreach (var item in songs)
            {
                a.SongItems.Add(new Song()
                {
                    Title = item["title"] as string,
                    Album = item["album"] as string,
                    Performers = (item["performers"] as Newtonsoft.Json.Linq.JArray).Select(x => x.ToString()).ToArray(),
                    AlbumArtists = (item["album_artists"] as Newtonsoft.Json.Linq.JArray).Select(x => x.ToString()).ToArray(),
                    PicturePath = item["picture_path"] as string,
                    OnlineID = item["id"] as string,
                    OnlineAlbumID = item["album_id"] as string,
                    IsOnline = true,
                    IsAvaliable = !string.IsNullOrEmpty(item["file_url"] as string),
                    BitRate = Convert.ToUInt32(item["bit_rate"]),
                    Year = Convert.ToUInt32(item["year"]),
                    OnlineUri = new Uri(item["file_url"] as string),
                    FilePath = item["file_url"] as string,
                    Track = Convert.ToUInt32(item["track"]),
                    Duration = TimeSpan.Parse(item["duration"] as string),
                    FileType = item["file_type"] as string
                });
            }

            a.OnlineArtistIDs = artists.Select(x => x["id"] as string).ToArray();

            // TODO: Optional Properties

            return a;
        }

        private Song GetOnlineSong(string t)
        {
            var set = JsonConvert.DeserializeObject<PropertySet>(t);

            // Required Properties
            var song = new Song
            {
                Title = set["title"] as string,
                OnlineAlbumID = set["album_id"] as string,
                Album = set["album"] as string,
                Performers = (set["performers"] as Newtonsoft.Json.Linq.JArray).Select(x => x.ToString()).ToArray(),
                AlbumArtists = (set["album_artists"] as Newtonsoft.Json.Linq.JArray).Select(x => x.ToString()).ToArray(),
                PicturePath = set["picture_path"] as string,
                OnlineUri = new Uri(set["file_url"] as string),
                FilePath = set["file_url"] as string,
                IsOnline = true,
                IsAvaliable = !string.IsNullOrEmpty(set["file_url"] as string),
                BitRate = Convert.ToUInt32(set["bit_rate"]),
                Year = Convert.ToUInt32(set["year"]),
                OnlineID = set["id"] as string,
                Track = Convert.ToUInt32(set["track"]),
                TrackCount = Convert.ToUInt32(set["track_count"]),
                Duration = TimeSpan.Parse(set["duration"] as string),
                FileType = set["file_type"] as string
            };

            // TODO: Optional Properties
            return song;
        }

        private IEnumerable<OnlineMusicItem> GetGenericMusicItem(string result)
        {
            var set = JsonConvert.DeserializeObject<PropertySet[]>(result);
            var res = new List<OnlineMusicItem>();
            foreach (var p in set)
            {
                MediaType t;
                switch (p["type"])
                {
                    case "song":
                        t = MediaType.Song;
                        break;
                    case "album":
                        t = MediaType.Album;
                        break;
                    case "artist":
                        t = MediaType.Artist;
                        break;
                    case "playlist":
                        t = MediaType.PlayList;
                        break;
                    default:
                        t = MediaType.Song;
                        break;
                }
                res.Add(new OnlineMusicItem(p["title"] as string, p["description"] as string, p["addtional"] as string, (p["id"] as Newtonsoft.Json.Linq.JArray).Select(x => x.ToString()).ToArray(), p["album_id"] as string)
                {
                    InnerType = t,
                    PicturePath = p["picture_path"] as string,
                });
            }
            return res;
        }
    }
}
