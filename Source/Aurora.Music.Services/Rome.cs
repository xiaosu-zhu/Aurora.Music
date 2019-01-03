using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aurora.Shared.Extensions;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Windows.Foundation.Collections;
using Windows.Storage;

namespace Aurora.Music.Services
{
    public sealed class Rome : IBackgroundTask
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

            var message = args.Request.Message;
            var returnData = new ValueSet();

            string command = message["q"] as string;

            switch (command)
            {
                // A remote device want to push text to this app
                case "push":
                    try
                    {
                        if (message["json"] is string json)
                        {
                            try
                            {
                                var file = await ApplicationData.Current.LocalFolder.CreateFileAsync("RoamingCheckPoint", CreationCollisionOption.ReplaceExisting);
                                await FileIO.WriteTextAsync(file, json);
                                returnData.Add("status", 1);
                            }
                            catch (Exception)
                            {
                                returnData.Add("status", 0);
                            }
                        }
                        else
                        {
                            returnData.Add("status", 0);
                        }
                    }
                    catch (Exception)
                    {
                        returnData.Add("status", 0);
                    }

                    break;
                // A remote device want to pull text from this app
                case "pull":
                    try
                    {
                        if (await ApplicationData.Current.LocalFolder.TryGetItemAsync("RoamingCheckPoint") is StorageFile f)
                        {
                            var j = await FileIO.ReadTextAsync(f);
                            if (!j.IsNullorEmpty())
                            {
                                returnData.Add("status", 0);
                            }
                            else
                            {
                                returnData.Add("json", j);
                                returnData.Add("status", 1);
                            }
                        }
                        else
                            returnData.Add("status", 0);
                    }
                    catch (Exception)
                    {
                        returnData.Add("status", 0);
                    }
                    break;

                case "clear":
                    if (await ApplicationData.Current.LocalFolder.TryGetItemAsync("RoamingCheckPoint") is StorageFile clear)
                    {
                        await clear.DeleteAsync();
                    }
                    returnData.Add("status", 1);
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
