using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppExtensions;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;

namespace Aurora.Music.Core.Models
{
    public abstract class Extension
    {
        protected PropertySet _properties;
        protected bool _loaded;
        protected string _serviceName;
        protected object lockable = new object();


        public Extension(AppExtension ext, PropertySet properties)
        {
            AppExtension = ext;
            _properties = properties;
            Enabled = false;
            _loaded = false;
            Offline = false;

            #region Properties
            _serviceName = null;
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
        public bool Enabled { get; protected set; }
        public bool Offline { get; protected set; }
        public AppExtension AppExtension { get; protected set; }
        #endregion

        public abstract void Execute(string str);
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
