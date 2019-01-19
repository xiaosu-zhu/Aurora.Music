using Aurora.Music.Core;
using Aurora.Music.Core.Extension;
using Aurora.Music.Core.Models;
using Aurora.Shared.Extensions;
using Aurora.Shared.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Web;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Windows.Foundation.Collections;

namespace Aurora.Music.Services
{
    public sealed class ExtensionService : IBackgroundTask
    {
        private BackgroundTaskDeferral backgroundTaskDeferral;
        private AppServiceConnection appServiceconnection;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            backgroundTaskDeferral = taskInstance.GetDeferral(); // Get a deferral so that the service isn't terminated.
            taskInstance.Canceled += OnTaskCanceled; // Associate a cancellation handler with the background task.

            // Retrieve the app service connection and set up a listener for incoming app service requests.
            var details = taskInstance.TriggerDetails as AppServiceTriggerDetails;
            appServiceconnection = details.AppServiceConnection;
            appServiceconnection.RequestReceived += OnRequestReceived;
        }

        private async void OnRequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            // Get a deferral because we use an awaitable API below to respond to the message
            // and we don't want this call to get cancelled while we are waiting.
            var messageDeferral = args.GetDeferral();

            var message = args.Request.Message;
            var returnData = new ValueSet();

            string command = message["q"] as string;

            switch (command)
            {
                case "lyric":
                    var title = message["title"] as string;
                    message.TryGetValue("artist", out object art);
                    var artists = art as string;
                    message.TryGetValue("album", out object alb);
                    var lyalbum = alb as string;

                    var localLrc = await LyricSearcher.SearchLrcLocalAsync(title, artists, lyalbum);
                    if (!localLrc.IsNullorEmpty())
                    {
                        returnData.Add("result", localLrc);

                        returnData.Add("status", 1);
                        break;
                    }
                    if (message.ContainsKey("ID") && message.ContainsKey("service") && message["service"] as string == "Aurora.Music.Services")
                    {
                        var result = await LyricSearcher.GetSongLrcByID(message["ID"] as string);
                        if (!result.IsNullorEmpty())
                        {
                            await LyricSearcher.SaveLrcLocalAsync(title, artists, lyalbum, result);
                            returnData.Add("result", result);
                            returnData.Add("status", 1);
                            break;
                        }
                    }
                    var substitutes = await LyricSearcher.GetSongLrcListAsync(title, artists);
                    if (!substitutes.IsNullorEmpty())
                    {
                        var result = await ApiRequestHelper.HttpGet(substitutes.First().Value);
                        if (!result.IsNullorEmpty())
                        {
                            result = HttpUtility.HtmlDecode(result);
                            await LyricSearcher.SaveLrcLocalAsync(title, artists, lyalbum, result);
                            returnData.Add("result", result);
                            returnData.Add("status", 1);
                        }
                        else
                        {
                            returnData.Add("result", null);
                            returnData.Add("status", 0);
                        }
                    }
                    else
                    {
                        returnData.Add("result", null);
                        returnData.Add("status", 0);
                    }
                    break;

                case "online_music":
                    var action = message["action"] as string;
                    switch (action)
                    {
                        case "search":
                            var result = await OnlineMusicSearcher.SearchAsync(message["keyword"] as string);
                            var resultList = new List<PropertySet>();
                            if (result == null || result.Data == null)
                            {
                                returnData.Add("status", 0);
                                break;
                            }

                            foreach (var item in result.Data.Song.ListItems)
                            {
                                var set = new PropertySet
                                {
                                    ["title"] = item.Title,
                                    ["description"] = item.SingerItems[0]?.Name,
                                    ["addtional"] = item.AlbumName,
                                    ["picture_path"] = OnlineMusicSearcher.GeneratePicturePathByID(item.AlbumMid),
                                    ["type"] = "song",
                                    ["id"] = new string[] { item.Mid },
                                    ["album_id"] = item.AlbumMid
                                };
                                resultList.Add(set);
                            }


                            if (!resultList.IsNullorEmpty())
                            {
                                returnData.Add("search_result", JsonConvert.SerializeObject(resultList.ToArray()));
                                returnData.Add("status", 1);
                            }
                            break;
                        case "song":
                            var song = await OnlineMusicSearcher.GetSongAsync(message["id"] as string);
                            if (song != null && !song.DataItems.IsNullorEmpty())
                            {
                                DateTime.TryParseExact(song.DataItems[0].Album.Time_Public, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var t);

                                // TODO: property
                                var songRes = new PropertySet
                                {
                                    ["title"] = song.DataItems[0].Title,
                                    ["id"] = song.DataItems[0].Mid,
                                    ["album"] = song.DataItems[0].Album.Name,
                                    ["album_id"] = song.DataItems[0].Album.Mid,
                                    ["performers"] = song.DataItems[0].SingerItems.Select(x => x.Name).ToArray(),
                                    ["year"] = t.Year,
                                    ["bit_rate"] = Settings.Current.GetPreferredBitRate() * 1000,
                                    ["track"] = song.DataItems[0].Index_Album,
                                    ["track_count"] = 0,
                                    ["duration"] = TimeSpan.FromSeconds(song.DataItems[0].Interval).ToString()
                                };
                                songRes["album_artists"] = songRes["performers"];
                                var picture = OnlineMusicSearcher.GeneratePicturePathByID(song.DataItems[0].Album.Mid);
                                songRes["picture_path"] = picture;
                                songRes["file_url"] = await OnlineMusicSearcher.GenerateFileUriByID(message["id"] as string, Settings.Current.GetPreferredBitRate());
                                songRes["file_type"] = OnlineMusicSearcher.GenerateFileTypeByID(message["id"] as string, Settings.Current.GetPreferredBitRate());
                                returnData.Add("song_result", JsonConvert.SerializeObject(songRes));
                                returnData.Add("status", 1);
                            }

                            break;
                        case "album":
                            var album = await OnlineMusicSearcher.GetAlbumAsync(message["id"] as string);
                            if (album != null && album.Data != null)
                            {
                                DateTime.TryParseExact(album.Data.GetAlbumInfo.Fpublic_Time, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var t);
                                var albumRes = new PropertySet
                                {
                                    ["name"] = album.Data.GetAlbumInfo.Falbum_Name,
                                    ["description"] = album.Data.GetAlbumDesc.Falbum_Desc.Replace("\n", "\r\n\r\n"),
                                    ["year"] = t.Year,
                                    ["track_count"] = album.Data.GetSongInfoItems.Count,
                                    ["disc_count"] = album.Data.GetSongInfoItems.Max(x => x.Index_Cd) + 1,
                                    ["picture_path"] = OnlineMusicSearcher.GeneratePicturePathByID(message["id"] as string),
                                    ["genres"] = new string[] { album.Data.Genre }
                                };
                                returnData.Add("album_result", JsonConvert.SerializeObject(albumRes));
                                returnData.Add("songs", JsonConvert.SerializeObject(album.Data.GetSongInfoItems.Select(x =>
                                {
                                    var p = new PropertySet()
                                    {
                                        ["id"] = x.Mid,
                                        ["title"] = x.Name,
                                        ["album"] = x.Album.Name,
                                        ["album_id"] = x.Album.Mid,
                                        ["performers"] = x.SingerItems.Select(y => y.Name).ToArray(),
                                        ["year"] = t.Year,
                                        ["bit_rate"] = Settings.Current.GetPreferredBitRate() * 1000,
                                        ["picture_path"] = OnlineMusicSearcher.GeneratePicturePathByID(x.Album.Mid),
                                        ["track"] = x.Index_Album,
                                        ["duration"] = TimeSpan.FromSeconds(x.Interval).ToString(),
                                        ["file_url"] = AsyncHelper.RunSync(async () => await OnlineMusicSearcher.GenerateFileUriByID(x.Mid, Settings.Current.GetPreferredBitRate())),
                                        ["file_type"] = OnlineMusicSearcher.GenerateFileTypeByID(x.Mid, Settings.Current.GetPreferredBitRate())
                                    };
                                    p["album_artists"] = p["performers"];
                                    return p;
                                })));
                                returnData.Add("album_artists", JsonConvert.SerializeObject(album.Data.SingerInfoItems.Select(x =>
                                {
                                    return new PropertySet()
                                    {
                                        ["name"] = x.Fsinger_Name,
                                        ["id"] = x.Fsinger_Mid,
                                    };
                                })));
                                returnData.Add("status", 1);
                            }
                            break;
                        case "artist":
                            var artist = await OnlineMusicSearcher.GetArtistAsync(message["id"] as string);
                            break;
                        case "require_playlist":
                            var playlists = await OnlineMusicSearcher.GetPlaylist((int)message["offset"], (int)message["count"]);
                            if (playlists != null && playlists.Data != null)
                            {
                                returnData.Add("status", 1);
                                returnData.Add("playlists", JsonConvert.SerializeObject(playlists.Data.ListItems.Select(a =>
                                {
                                    return new PropertySet()
                                    {
                                        ["title"] = a.Dissname,
                                        ["addtional"] = DateTime.ParseExact(a.Createtime, "yyyy-M-dd", CultureInfo.InvariantCulture).PubDatetoString(Consts.Today, "ddd", "M/dd ddd", "yy/M/dd", Consts.Next, Consts.Last),
                                        ["description"] = a.Creator.Name,
                                        ["id"] = a.Dissid,
                                        ["picture_path"] = a.Imgurl
                                    };
                                })));
                            }
                            break;
                        case "show_playlist":
                            var playlist = await OnlineMusicSearcher.ShowPlaylist(message["id"] as string);
                            if (playlist != null && playlist.CdlistItems != null)
                            {
                                returnData.Add("status", 1);
                                returnData.Add("playlist_result", JsonConvert.SerializeObject(new PropertySet()
                                {
                                    ["name"] = playlist.CdlistItems[0].Dissname,
                                    ["description"] = System.Net.WebUtility.HtmlDecode(playlist.CdlistItems[0].Desc).Replace("<br>", Environment.NewLine + Environment.NewLine).Replace("<br/>", Environment.NewLine + Environment.NewLine).Replace("<br >", Environment.NewLine + Environment.NewLine).Replace("<br />", Environment.NewLine + Environment.NewLine),
                                    ["id"] = playlist.CdlistItems[0].Disstid,
                                    ["picture_path"] = playlist.CdlistItems[0].Logo,
                                    ["year"] = 0,
                                    ["disc_count"] = 1,
                                    ["track_count"] = playlist.CdlistItems[0].SongList.Count,
                                    ["genres"] = playlist.CdlistItems[0].TagsItems.Select(a => a.Name)
                                }
                                ));

                                returnData.Add("songs", JsonConvert.SerializeObject(playlist.CdlistItems[0].SongList.Select((x, index) =>
                                {
                                    var p = new PropertySet()
                                    {
                                        ["id"] = x.Songmid,
                                        ["title"] = x.Songname,
                                        ["album"] = x.Albumname,
                                        ["album_id"] = x.Albumid,
                                        ["performers"] = x.SingerItems.Select(y => y.Name).ToArray(),
                                        ["year"] = 0,
                                        ["bit_rate"] = Settings.Current.GetPreferredBitRate() * 1000,
                                        ["picture_path"] = OnlineMusicSearcher.GeneratePicturePathByID(x.Albummid),
                                        ["track"] = index + 1,
                                        ["duration"] = TimeSpan.FromSeconds(x.Interval).ToString(),
                                        ["file_url"] = AsyncHelper.RunSync(async () => await OnlineMusicSearcher.GenerateFileUriByID(x.Songmid, Settings.Current.GetPreferredBitRate())),
                                        ["file_type"] = OnlineMusicSearcher.GenerateFileTypeByID(x.Songmid, Settings.Current.GetPreferredBitRate())
                                    };
                                    p["album_artists"] = p["performers"];
                                    return p;
                                })));
                                returnData.Add("album_artists", JsonConvert.SerializeObject(
                                    new PropertySet[]{
                                        new PropertySet()
                                        {
                                            ["name"] = playlist.CdlistItems[0].Nickname,
                                            ["id"] = playlist.CdlistItems[0].Singerid,
                                        }
                                    }
                                ));
                            }
                            break;
                        default:
                            break;
                    }
                    break;
                case "online_meta":
                    var meta_action = message["action"] as string;
                    switch (meta_action)
                    {
                        case "album":
                            var meta_album = await LastfmSearcher.GetAlbumInfo(message["album"] as string, message["artist"] as string);
                            if (meta_album != null)
                            {
                                returnData.Add("status", 1);
                                returnData.Add("album_result", JsonConvert.SerializeObject(new PropertySet()
                                {
                                    ["name"] = meta_album.Name,
                                    ["artwork"] = meta_album.AltArtwork?.OriginalString,
                                    ["desc"] = meta_album.Description,
                                    ["artist"] = meta_album.Artist,
                                    ["year"] = meta_album.Year
                                }));
                            }
                            break;
                        case "artist":
                            var meta_artist = await LastfmSearcher.GetArtistInfo(message["artist"] as string);
                            if (meta_artist != null)
                            {
                                returnData.Add("status", 1);
                                returnData.Add("artist_result", JsonConvert.SerializeObject(new PropertySet()
                                {
                                    ["name"] = meta_artist.Name,
                                    ["avatar"] = meta_artist.AvatarUri?.OriginalString,
                                    ["desc"] = meta_artist.Description,
                                }));
                            }
                            break;
                    }
                    break;
                default:
                    returnData.Add("status", 0);
                    break;
            }


            await args.Request.SendResponseAsync(returnData);
            // Return the data to the caller.
            // Complete the deferral so that the platform knows that we're done responding to the app service call.
            // Note for error handling: this must be called even if SendResponseAsync() throws an exception.
            messageDeferral.Complete();
        }

        private void OnTaskCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            if (backgroundTaskDeferral != null)
            {
                // Complete the service deferral.
                backgroundTaskDeferral.Complete();
            }
        }

    }
}
