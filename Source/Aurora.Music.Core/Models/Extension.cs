using Aurora.Shared.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.AppExtensions;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;
using Windows.System;

namespace Aurora.Music.Core.Models
{
    public abstract class Extension
    {
        protected PropertySet _properties;
        protected string _serviceName;
        protected object lockable = new object();


        public Extension(AppExtension ext, IPropertySet properties)
        {
            AppExtension = ext;
            _properties = properties as PropertySet;

            #region Properties
            if (_properties != null)
            {
                if (_properties.ContainsKey("Service"))
                {
                    PropertySet serviceProperty = _properties["Service"] as PropertySet;
                    _serviceName = serviceProperty["#text"].ToString();
                }
            }
            #endregion

            //AUMID + Extension ID is the unique identifier for an extension
            UniqueId = ext.AppInfo.AppUserModelId + "$|$" + ext.Id;
        }

        #region Properties
        public string UniqueId { get; }
        public AppExtension AppExtension { get; protected set; }
        #endregion

        public abstract Task<object> ExecuteAsync(params KeyValuePair<string, object>[] parameters);

        public void Unload()
        {

        }

        public void New(AppExtension ext)
        {
            if (UniqueId == ext.AppInfo.AppUserModelId + "$|$" + ext.Id)
            {
                AppExtension = ext;
                #region Properties
                if (_properties != null)
                {
                    if (_properties.ContainsKey("Service"))
                    {
                        PropertySet serviceProperty = _properties["Service"] as PropertySet;
                        _serviceName = serviceProperty["#text"].ToString();
                    }
                }
                #endregion
            }
        }

        public static async Task<List<Extension>> Load(string lyricExtensionID)
        {
            if (lyricExtensionID.IsNullorEmpty())
            {
                lyricExtensionID = Consts.AppUserModelId + "$|$BuiltIn";
            }
            var catalog = AppExtensionCatalog.Open(Consts.ExtensionContract);
            var extesions = await catalog.FindAllAsync();
            foreach (var ext in extesions)
            {
                if (lyricExtensionID == ext.AppInfo.AppUserModelId + "$|$" + ext.Id)
                {
                    var properties = await ext.GetExtensionPropertiesAsync();

                    var categoryProperty = properties["Category"] as PropertySet;

                    var categories = (categoryProperty["#text"] as string).Split(';');

                    var results = new List<Extension>();

                    foreach (var category in categories)
                    {
                        switch (category)
                        {
                            case "Lyric":
                                results.Add(new LyricExtension(ext, properties));
                                break;
                            case "OnlineMusic":
                                results.Add(new OnlineMusicExtension(ext, properties));
                                break;
                            case "OnlineMeta":
                                results.Add(new OnlineMetaExtension(ext, properties));
                                break;
                            default:
                                break;
                        }
                    }
                    return results;
                }
            }
            throw new ArgumentException("Can't find specific Extension");
        }
        //{
        //if (_loaded)
        //{
        //    #region App Service
        //    // App services are a better approach!
        //    try
        //    {
        //        // do app service call
        //        using (var connection = new AppServiceConnection())
        //        {
        //            // service name was in properties
        //            connection.AppServiceName = _serviceName;

        //            // package Family Name is in the extension
        //            connection.PackageFamilyName = AppExtension.Package.Id.FamilyName;

        //            // open connection
        //            AppServiceConnectionStatus status = await connection.OpenAsync();
        //            if (status != AppServiceConnectionStatus.Success)
        //            {
        //                throw new InvalidOperationException(status.ToString());
        //            }
        //            else
        //            {
        //                // send request to service
        //                var request = new ValueSet
        //                {
        //                    { "Command", "Load" }
        //                };

        //                //TODO: request

        //                // get response
        //                AppServiceResponse response = await connection.SendMessageAsync(request);
        //                if (response.Status == AppServiceResponseStatus.Success)
        //                {
        //                    ValueSet message = response.Message as ValueSet;
        //                    //TODO: handle response
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        throw e;
        //    }
        //    #endregion
        //}
        //}

    }
}
