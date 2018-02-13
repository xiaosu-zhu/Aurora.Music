// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Aurora.Shared.Extensions;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppExtensions;
using Windows.Foundation.Collections;

namespace Aurora.Music.Core.Models
{
    public abstract class Extension
    {
        protected PropertySet _properties;
        protected string _serviceName;

        public string ServiceName
        {
            get => _serviceName;
        }

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
            UniqueId = ext.AppInfo.PackageFamilyName + Consts.ArraySeparator + ext.Id;
        }

        #region Properties
        public string UniqueId { get; }
        public AppExtension AppExtension { get; protected set; }
        #endregion

        public abstract Task<object> ExecuteAsync(ValueSet parameters);

        public void Unload()
        {

        }

        public void New(AppExtension ext)
        {
            if (UniqueId == ext.AppInfo.PackageFamilyName + Consts.ArraySeparator + ext.Id)
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

        public static async Task<T> Load<T>(string lyricExtensionID) where T : Extension
        {
            if (lyricExtensionID.IsNullorEmpty())
            {
                lyricExtensionID = Consts.PackageFamilyName + "$|$BuiltIn";
            }
            var catalog = AppExtensionCatalog.Open(Consts.ExtensionContract);
            var extesions = await catalog.FindAllAsync();
            bool b = false;
            find: foreach (var ext in extesions)
            {
                if (lyricExtensionID == ext.AppInfo.PackageFamilyName + Consts.ArraySeparator + ext.Id)
                {
                    try
                    {
                        var properties = await ext.GetExtensionPropertiesAsync();

                        var categoryProperty = properties["Category"] as PropertySet;

                        // parse Category
                        var categories = (categoryProperty["#text"] as string).Split(';');

                        if (typeof(T) == typeof(LyricExtension) && categories.Contains("Lyric"))
                        {
                            return new LyricExtension(ext, properties) as T;
                        }
                        if (typeof(T) == typeof(OnlineMetaExtension) && categories.Contains("OnlineMeta"))
                        {
                            return new OnlineMetaExtension(ext, properties) as T;
                        }
                        if (typeof(T) == typeof(OnlineMusicExtension) && categories.Contains("OnlineMusic"))
                        {
                            return new OnlineMusicExtension(ext, properties) as T;
                        }

                        // we have the exact extension, but it don't provide this kind of extension, return null
                        return null;
                    }
                    catch (Exception)
                    {
                        // if extension didn't write the correct manifest, may throw exception
                        continue;
                    }
                }
            }
            if (!b)
            {
                b = true;
                lyricExtensionID = Consts.PackageFamilyName + "$|$BuiltIn";
                goto find;
            }
            else
            {
                throw new InvalidOperationException("Can't find specific Extension");
            }
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
