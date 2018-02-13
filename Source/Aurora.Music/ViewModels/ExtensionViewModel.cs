// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Aurora.Music.Core;
using Aurora.Shared.MVVM;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppExtensions;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Xaml.Media.Imaging;

namespace Aurora.Music.ViewModels
{
    class ExtensionViewModel : ViewModelBase
    {
        public AppExtension AppExtension { get; private set; }

        private string uniqueId;
        public string UniqueId
        {
            get { return uniqueId; }
            set { SetProperty(ref uniqueId, value); }
        }

        private bool isCurrent;
        public bool IsCurrent
        {
            get { return isCurrent; }
            set { SetProperty(ref isCurrent, value); }
        }

        private bool canLaunch;
        public bool CanLaunch
        {
            get { return canLaunch; }
            set { SetProperty(ref canLaunch, value); }
        }

        private bool canUninstall = true;
        public bool CanUninstall
        {
            get { return canUninstall; }
            set { SetProperty(ref canUninstall, value); }
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

        private string name;
        public string Name
        {
            get { return Avaliable ? name : Consts.Localizer.GetString("NotAvaliableText"); }
            set { SetProperty(ref name, value); }
        }

        private string service;
        public string Service
        {
            get { return service; }
            set { SetProperty(ref service, value); }
        }

        private string _launchUri;
        private string descri;
        public string Description
        {
            get { return descri; }
            set { SetProperty(ref descri, value); }
        }

        private ExtType type;
        public ExtType ExtType
        {
            get { return type; }
            set { SetProperty(ref type, value); }
        }

        public override string ToString()
        {
            return $"{Name} - {Description}";
        }

        public bool LyricEnabled(ExtType type)
        {
            return type.HasFlag(ExtType.Lyric);
        }
        public bool MusicEnabled(ExtType type)
        {
            return type.HasFlag(ExtType.OnlineMusic);
        }
        public bool MetaEnabled(ExtType type)
        {
            return type.HasFlag(ExtType.OnlieMetaData);
        }

        public DelegateCommand LaunchUri
        {
            get => new DelegateCommand(async () =>
            {
                await Launcher.LaunchUriAsync(new Uri(_launchUri));
            });
        }

        public ExtensionViewModel(AppExtension ext, PropertySet properties)
        {
            UniqueId = ext.AppInfo.PackageFamilyName + Consts.ArraySeparator + ext.Id;
            if (ext.AppInfo.PackageFamilyName == Consts.PackageFamilyName)
            {
                CanUninstall = false;
            }

            AppExtension = ext;
            Avaliable = ext.Package.Status.VerifyIsOK();

            var cates = ((properties["Category"] as PropertySet)["#text"] as string).Split(';');
            if (cates != null && cates.Length > 0)
            {
                foreach (var item in cates)
                {
                    switch (item)
                    {
                        case "Lyric":
                            ExtType |= ExtType.Lyric; break;
                        case "OnlineMusic":
                            ExtType |= ExtType.OnlineMusic; break;
                        case "OnlineMeta":
                            ExtType |= ExtType.OnlieMetaData; break;
                        default:
                            break;
                    }
                }
            }
            else
            {
                ExtType = ExtType.NotSpecific;
            }

            Name = ext.DisplayName;
            Description = ext.Description;
            Service = (properties["Service"] as PropertySet)["#text"] as string;

            if (properties.TryGetValue("LaunchUri", out object uri))
            {
                if (uri is PropertySet p && p["#text"] is string s)
                {
                    if (Uri.TryCreate(s, UriKind.Absolute, out var u))
                    {
                        CanLaunch = true;
                        _launchUri = s;
                    }
                    else
                    {
                        CanLaunch = false;
                    }
                }
                else
                {
                    CanLaunch = false;
                }

            }
            else
            {
                CanLaunch = false;
            }
        }

        internal async Task Load()
        {
            // get logo 
            var filestream = await (AppExtension.AppInfo.DisplayInfo.GetLogo(new Windows.Foundation.Size(1, 1))).OpenReadAsync();
            Logo = new BitmapImage();
            logo.DecodePixelHeight = 48;
            logo.DecodePixelType = DecodePixelType.Logical;
            logo.SetSource(filestream);
        }

        internal void Unload()
        {
            Avaliable = false;
        }

        internal async Task Update(AppExtension ext)
        {

            var properties = await ext.GetExtensionPropertiesAsync() as PropertySet;
            UniqueId = ext.AppInfo.PackageFamilyName + Consts.ArraySeparator + ext.Id;


            if (ext.AppInfo.PackageFamilyName == Consts.PackageFamilyName)
            {
                CanUninstall = false;
            }

            AppExtension = ext;
            Avaliable = ext.Package.Status.VerifyIsOK();

            var cates = ((properties["Category"] as PropertySet)["#text"] as string).Split(';');
            if (cates != null && cates.Length > 0)
            {
                foreach (var item in cates)
                {
                    switch (item)
                    {
                        case "Lyric":
                            ExtType |= ExtType.Lyric; break;
                        case "OnlineMusic":
                            ExtType |= ExtType.OnlineMusic; break;
                        case "OnlineMeta":
                            ExtType |= ExtType.OnlieMetaData; break;
                        default:
                            break;
                    }
                }
            }
            else
            {
                ExtType = ExtType.NotSpecific;
            }

            Name = ext.DisplayName;
            Description = ext.Description;
            Service = (properties["Service"] as PropertySet)["#text"] as string;

            _launchUri = (properties["LaunchUri"] as PropertySet)["#text"] as string;
            if (string.IsNullOrEmpty(_launchUri))
            {
                CanLaunch = false;
            }
            else
            {
                if (Uri.TryCreate(_launchUri, UriKind.Absolute, out var u))
                {
                    CanLaunch = true;
                }
                else
                {
                    CanLaunch = false;
                }
            }
        }
    }

    [Flags]
    public enum ExtType { NotSpecific = 1, Lyric = 2, OnlineMusic = 4, OnlieMetaData = 8 }
}
