using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppExtensions;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;

namespace Aurora.Music.Core.Models
{
    public abstract class Extension
    {
        private PropertySet _properties;
        private bool _loaded;
        private string _serviceName;
        private object lockable = new object();


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
        public bool Enabled { get; private set; }
        public bool Offline { get; private set; }
        public AppExtension AppExtension { get; private set; }
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


        public async Task Update(AppExtension ext)
        {
            // ensure this is the same uid
            string identifier = ext.AppInfo.AppUserModelId + "$|$" + ext.Id;
            if (identifier != this.UniqueId)
            {
                return;
            }

            // get extension properties
            var properties = await ext.GetExtensionPropertiesAsync() as PropertySet;

            // get logo 
            //var filestream = await (ext.AppInfo.DisplayInfo.GetLogo(new Windows.Foundation.Size(1, 1))).OpenReadAsync();

            // update the extension
            this.AppExtension = ext;
            this._properties = properties;

            #region Update Properties
            // update app service information
            this._serviceName = null;
            if (this._properties != null)
            {
                if (this._properties.ContainsKey("Service"))
                {
                    PropertySet serviceProperty = this._properties["Service"] as PropertySet;
                    this._serviceName = serviceProperty["#text"].ToString();
                }
            }
            #endregion

            // load it
            Load();
        }

        // this controls loading of the extension
        public void Load()
        {
            #region Error Checking
            // if it's not enabled or already loaded, don't load it
            if (!Enabled || _loaded)
            {
                return;
            }

            // make sure package is OK to load
            if (!AppExtension.Package.Status.VerifyIsOK())
            {
                return;
            }
            #endregion

        }

        // This enables the extension
        public void Enable()
        {
            // indicate desired state is enabled
            Enabled = true;

            // load the extension
            Load();
        }

        // this is different from Disable, example: during updates where we only want to unload -> reload
        // Enable is user intention. Load respects enable, but unload doesn't care
        public void Unload()
        {
            // unload it
            lock (lockable)
            {
                if (_loaded)
                {
                    // see if package is offline
                    if (!AppExtension.Package.Status.VerifyIsOK() && !AppExtension.Package.Status.PackageOffline)
                    {
                        Offline = true;
                    }

                    _loaded = false;
                }
            }
        }

        // user-facing action to disable the extension
        public void Disable()
        {
            // only disable if it is enabled
            if (Enabled)
            {
                // set desired state to disabled
                Enabled = false;
                // unload the app
                Unload();
            }
        }
    }
}
