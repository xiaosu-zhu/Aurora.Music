// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

using Aurora.Music.Core;
using Aurora.Music.Core.Models;
using Aurora.Music.ViewModels;
using Windows.ApplicationModel;
using Windows.ApplicationModel.AppExtensions;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace Aurora.Music.Controls
{
    public sealed partial class ExtensionsManager : ContentDialog
    {
        private AppExtensionCatalog _catalog;
        ObservableCollection<ExtensionViewModel> ExtensionList = new ObservableCollection<ExtensionViewModel>();

        public ExtensionsManager()
        {
            InitializeComponent();
            RequestedTheme = Settings.Current.Theme;
            var t = Task.Run(async () =>
            {
                await Init();
            });
        }

        public async Task Init()
        {
            _catalog = AppExtensionCatalog.Open(Consts.ExtensionContract);
            // set up extension management events
            _catalog.PackageInstalled += _catalog_PackageInstalled;
            _catalog.PackageUpdated += _catalog_PackageUpdated;
            _catalog.PackageUninstalling += _catalog_PackageUninstalling;
            _catalog.PackageUpdating += _catalog_PackageUpdating;
            _catalog.PackageStatusChanged += _catalog_PackageStatusChanged;

            // Scan all extensions
            await FindAllExtensions();
        }

        public async Task FindAllExtensions()
        {
            // load all the extensions currently installed
            IReadOnlyList<AppExtension> extensions = await _catalog.FindAllAsync();
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, async () =>
            {
                foreach (AppExtension ext in extensions)
                {
                    // load this extension
                    await LoadExtension(ext);
                }
                ExtensionList.ToList().ForEach(async (x) => { await x.Load(); });
            });
        }

        // loads an extension
        public async Task LoadExtension(AppExtension ext)
        {
            // get unique identifier for this extension
            string identifier = ext.AppInfo.PackageFamilyName + Consts.ArraySeparator + ext.Id;

            // load the extension if the package is OK
            if (!(ext.Package.Status.VerifyIsOK()
#if !DEBUG
                    && Settings.Current.DebugModeEnabled ? true : ext.Package.SignatureKind == PackageSignatureKind.Store
#endif
                    ))
            {
                // if this package doesn't meet our requirements
                // ignore it and return
                return;
            }
            var properties = await ext.GetExtensionPropertiesAsync() as PropertySet;
            var cates = ((properties["Category"] as PropertySet)["#text"] as string).Split(';');

            // if its already existing then this is an update
            var exting = ExtensionList.Where(e => e.UniqueId == identifier).FirstOrDefault();
            // new extension
            if (exting == null)
            {
                // get extension properties
                ExtensionList.Add(new ExtensionViewModel(ext, properties));
            }
            // update
            else
            {
                // update the extension
                await exting.Update(ext);
            }

        }

        private async void _catalog_PackageStatusChanged(AppExtensionCatalog sender, AppExtensionPackageStatusChangedEventArgs args)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                ExtensionList.Clear();
            });
            await FindAllExtensions();
        }

        private async void _catalog_PackageUpdating(AppExtensionCatalog sender, AppExtensionPackageUpdatingEventArgs args)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                ExtensionList.Clear();
            });
            await FindAllExtensions();
        }

        private async void _catalog_PackageUninstalling(AppExtensionCatalog sender, AppExtensionPackageUninstallingEventArgs args)
        {
            await Task.Delay(1000);
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                ExtensionList.Clear();
            });
            await FindAllExtensions();
        }

        private async void _catalog_PackageUpdated(AppExtensionCatalog sender, AppExtensionPackageUpdatedEventArgs args)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                ExtensionList.Clear();
            });
            await FindAllExtensions();
        }

        private async void _catalog_PackageInstalled(AppExtensionCatalog sender, AppExtensionPackageInstalledEventArgs args)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                ExtensionList.Clear();
            });
            await FindAllExtensions();
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var p = (e.ClickedItem as ExtensionViewModel).IsCurrent;
            foreach (var item in ExtensionList)
            {
                item.IsCurrent = false;
            }
            (e.ClickedItem as ExtensionViewModel).IsCurrent = !p;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            await _catalog.RequestRemovePackageAsync(((sender as Button).DataContext as ExtensionViewModel).AppExtension.Package.Id.FullName);
        }
    }
}
