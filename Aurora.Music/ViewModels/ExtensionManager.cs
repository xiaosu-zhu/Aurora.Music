using Aurora.Music.Core.Models;
using Aurora.Shared.MVVM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.AppExtensions;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml.Media.Imaging;

namespace Aurora.Music.ViewModels
{
    class ExtensionManager : ViewModelBase
    {
        private AppExtensionCatalog _catalog;

        public ExtensionManager(string contract)
        {
            // extensions list   
            Extensions = new ObservableCollection<ExtensionViewModel>();

            // catalog & contract
            Contract = contract;
            _catalog = AppExtensionCatalog.Open(Contract);
            // set up extension management events
            _catalog.PackageInstalled += Catalog_PackageInstalled;
            _catalog.PackageUpdated += Catalog_PackageUpdated;
            _catalog.PackageUninstalling += Catalog_PackageUninstalling;
            _catalog.PackageUpdating += Catalog_PackageUpdating;
            _catalog.PackageStatusChanged += Catalog_PackageStatusChanged;

            // Scan all extensions
            FindAllExtensions();
        }

        public ObservableCollection<ExtensionViewModel> Extensions { get; }

        public string Contract { get; }

        public async void FindAllExtensions()
        {
            // load all the extensions currently installed
            IReadOnlyList<AppExtension> extensions = await _catalog.FindAllAsync();
            foreach (AppExtension ext in extensions)
            {
                // load this extension
                await LoadExtension(ext);
            }
        }

        private async void Catalog_PackageInstalled(AppExtensionCatalog sender, AppExtensionPackageInstalledEventArgs args)
        {
            foreach (AppExtension ext in args.Extensions)
            {
                await LoadExtension(ext);
            }
        }

        // package has been updated, so reload the extensions
        private async void Catalog_PackageUpdated(AppExtensionCatalog sender, AppExtensionPackageUpdatedEventArgs args)
        {
            foreach (AppExtension ext in args.Extensions)
            {
                await LoadExtension(ext);
            }
        }

        // package is updating, so just unload the extensions
        private void Catalog_PackageUpdating(AppExtensionCatalog sender, AppExtensionPackageUpdatingEventArgs args)
        {
            UnloadExtensions(args.Package);
        }

        // package is removed, so unload all the extensions in the package and remove it
        private void Catalog_PackageUninstalling(AppExtensionCatalog sender, AppExtensionPackageUninstallingEventArgs args)
        {
            RemoveExtensions(args.Package);
        }

        // package status has changed, could be invalid, licensing issue, app was on USB and removed, etc
        private void Catalog_PackageStatusChanged(AppExtensionCatalog sender, AppExtensionPackageStatusChangedEventArgs args)
        {
            // get package status
            if (!(args.Package.Status.VerifyIsOK()))
            {
                // if it's offline unload only
                if (args.Package.Status.PackageOffline)
                {
                    UnloadExtensions(args.Package);
                }

                // package is being serviced or deployed
                else if (args.Package.Status.Servicing || args.Package.Status.DeploymentInProgress)
                {
                    // ignore these package status events
                }

                // package is tampered or invalid or some other issue
                // glyphing the extensions would be a good user experience
                else
                {
                    RemoveExtensions(args.Package);
                }

            }
            // if package is now OK, attempt to load the extensions
            else
            {
                // try to load any extensions associated with this package
                LoadExtensions(args.Package);
            }
        }

        // loads an extension
        public async Task LoadExtension(AppExtension ext)
        {
            // get unique identifier for this extension
            string identifier = ext.AppInfo.AppUserModelId + "$|$" + ext.Id;

            // load the extension if the package is OK
            if (!(ext.Package.Status.VerifyIsOK()
                    // This is where we'd normally do signature verfication, but for demo purposes it isn't important
                    // Below is an example of how you'd ensure that you only load store-signed extensions :)

#if !DEBUG
                    && ext.Package.SignatureKind == PackageSignatureKind.Store
#endif

                    ))
            {
                // if this package doesn't meet our requirements
                // ignore it and return
                return;
            }

            // if its already existing then this is an update
            var existingExt = Extensions.Where(e => e.UniqueId == identifier).FirstOrDefault();

            // new extension
            if (existingExt == null)
            {
                // get extension properties
                var properties = await ext.GetExtensionPropertiesAsync() as PropertySet;
            }
            // update
            else
            {
                // update the extension
                existingExt.Update(ext);
            }
        }

        // loads all extensions associated with a package - used for when package status comes back
        public void LoadExtensions(Package package)
        {
            Extensions.Where(ext => ext.AppExtension.Package.Id.FamilyName == package.Id.FamilyName).ToList().ForEach(async e =>
            { await e.Load(); });
        }

        // unloads all extensions associated with a package - used for updating and when package status goes away
        public void UnloadExtensions(Package package)
        {
            Extensions.Where(ext => ext.AppExtension.Package.Id.FamilyName == package.Id.FamilyName).ToList().ForEach(e => { e.Unload(); });

        }

        // removes all extensions associated with a package - used when removing a package or it becomes invalid
        public void RemoveExtensions(Package package)
        {
            Extensions.Where(ext => ext.AppExtension.Package.Id.FamilyName == package.Id.FamilyName).ToList().ForEach(e => { e.Unload(); Extensions.Remove(e); });
        }


        public async void RemoveExtension(Extension ext)
        {
            await _catalog.RequestRemovePackageAsync(ext.AppExtension.Package.Id.FullName);
        }

        #region Extra exceptions
        // For exceptions using the Extension Manager
        public class ExtensionManagerException : Exception
        {
            public ExtensionManagerException() { }

            public ExtensionManagerException(string message) : base(message) { }

            public ExtensionManagerException(string message, Exception inner) : base(message, inner) { }
        }
        #endregion
    }

    class ExtensionViewModel : ViewModelBase
    {
        private Extension extension;

        public AppExtension AppExtension { get => extension.AppExtension; }

        private string uniqueId;
        public string UniqueId
        {
            get { return uniqueId; }
            set { SetProperty(ref uniqueId, value); }
        }

        private bool avaliable;
        public bool Avaliable
        {
            get { return avaliable; }
            set { SetProperty(ref avaliable, value); }
        }

        private BitmapImage logo;
        public BitmapImage Logo
        {
            get { return logo; }
            set { SetProperty(ref logo, value); }
        }

        public ExtensionViewModel(AppExtension ext, PropertySet properties)
        {
            UniqueId = ext.AppInfo.AppUserModelId + "$|$" + ext.Id;
            switch (properties["Category"])
            {
                case "Lyric":
                    // create new extension
                    extension = new LyricExtension(ext, properties);
                    break;
                default:
                    break;
            }
        }

        internal async Task Load()
        {
            // get logo 
            var filestream = await(AppExtension.AppInfo.DisplayInfo.GetLogo(new Windows.Foundation.Size(1, 1))).OpenReadAsync();
            Logo = new BitmapImage();
            logo.SetSource(filestream);
        }

        internal void Unload()
        {
            Avaliable = false;
        }

        internal void Update(AppExtension ext)
        {
            extension.Unload();
            extension.New(ext);
        }
    }
}
