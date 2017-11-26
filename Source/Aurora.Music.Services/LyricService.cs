using Aurora.Music.Core.Extension;
using Aurora.Music.Core.Models;
using Aurora.Music.Core.Storage;
using Aurora.Shared.Extensions;
using Aurora.Shared.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace Aurora.Music.Services
{
    public sealed class LyricService : IBackgroundTask
    {
        private BackgroundTaskDeferral backgroundTaskDeferral;
        private AppServiceConnection appServiceconnection;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            this.backgroundTaskDeferral = taskInstance.GetDeferral(); // Get a deferral so that the service isn't terminated.
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

            ValueSet message = args.Request.Message;
            ValueSet returnData = new ValueSet();

            string command = message["q"] as string;

            switch (command)
            {
                case "lyric":

                    var title = message["title"] as string;
                    message.TryGetValue("artist", out object art);
                    var artists = art as string;

                    var localLrc = await LyricSearcher.SearchLrcLocalAsync(title, artists);
                    if (!localLrc.IsNullorEmpty())
                    {
                        returnData.Add("result", localLrc);

                        returnData.Add("status", 1);
                        break;
                    }

                    var substitutes = await LyricSearcher.GetSongLrcListAsync(title, artists);
                    if (!substitutes.IsNullorEmpty())
                    {
                        var result = await ApiRequestHelper.HttpGet(substitutes.First().Value);
                        if (!result.IsNullorEmpty())
                        {
                            await LyricSearcher.SaveLrcLocalAsync(title, artists, result);
                        }
                        returnData.Add("result", result);
                    }
                    else
                    {
                        returnData.Add("result", null);
                    }
                    returnData.Add("status", 1);
                    break;

                case "online_music":
                    var action = message["action"] as string;
                    switch (action)
                    {
                        case "search":
                            message.TryGetValue("page", out object page);
                            message.TryGetValue("count", out object count);
                            var result = await OnlineMusicSearcher.SearchAsync(message["keyword"] as string, page as int?, count as int?);
                            var resultList = new List<PropertySet>();
                            if (result == null)
                            {
                                returnData.Add("status", 0);
                                break;
                            }

                            foreach (var item in result.Data.Song.ListItems)
                            {
                                var set = new PropertySet
                                {
                                    ["title"] = item.Title,
                                    ["description"] = item.SingerItems[0]?.Title,
                                    ["addtional"] = item.Album.Title,
                                    ["picture_path"] = null,
                                    ["type"] = "song",
                                    ["id"] = new string[] { item.Mid }
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
                                DateTime.TryParseExact(song.DataItems[0].Album.Time_Public, "yyyy-MM-dd", null, DateTimeStyles.None, out DateTime t);

                                // TODO: property

                                var songRes = new PropertySet
                                {
                                    ["title"] = song.DataItems[0].Title,
                                    ["album"] = song.DataItems[0].Album.Name,
                                    ["performers"] = song.DataItems[0].SingerItems.Select(x => x.Name).ToArray(),
                                    ["year"] = 0u,
                                    ["bit_rate"] = 192u
                                };
                                songRes["album_artists"] = songRes["performers"];
                                var picture = OnlineMusicSearcher.GeneratePicturePathByID(song.DataItems[0].Album.Mid);
                                songRes["picture_path"] = picture;
                                songRes["file_url"] = await OnlineMusicSearcher.GenerateFileUriByID(message["id"] as string);
                                returnData.Add("song_result", JsonConvert.SerializeObject(songRes));
                                returnData.Add("status", 1);
                            }

                            break;
                        case "album":
                            var album = await OnlineMusicSearcher.GetAlbumAsync(message["id"] as string);
                            break;
                        case "artist":
                            var artist = await OnlineMusicSearcher.GetArtistAsync(message["id"] as string);
                            break;
                        default:
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
            if (this.backgroundTaskDeferral != null)
            {
                // Complete the service deferral.
                this.backgroundTaskDeferral.Complete();
            }
        }

    }
}
