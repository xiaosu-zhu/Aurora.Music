using Aurora.Music.Core;
using Aurora.Music.Core.Models;
using Aurora.Music.Pages;
using Aurora.Music.PlaybackEngine;
using Aurora.Shared.Extensions;
using Aurora.Shared.Helpers;
using Aurora.Shared.MVVM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.AppExtensions;
using Windows.ApplicationModel.Core;
using Windows.Devices.Enumeration;
using Windows.Foundation.Collections;
using Windows.Media.Devices;
using Windows.Services.Store;
using Windows.System;

namespace Aurora.Music.ViewModels
{
    class SettingsPageViewModel : ViewModelBase
    {
        private int audioSelectedIndex = -1;
        public int AudioSelectedIndex
        {
            get { return audioSelectedIndex; }
            set
            {
                if (value == -1)
                    return;
                SetProperty(ref audioSelectedIndex, value);

                settings.OutputDeviceID = DevicList[value].ID;
                settings.Save();
                Player.Current.ChangeAudioEndPoint(settings.OutputDeviceID);
            }
        }

        public DelegateCommand NavigateToPrivacy
        {
            get => new DelegateCommand(async () =>
            {
                await Launcher.LaunchUriAsync(new Uri("http://198.181.41.120/privacypolicy.htm"));
            });
        }

        public DelegateCommand CommentInStore
        {
            get => new DelegateCommand(async () =>
            {
                await Launcher.LaunchUriAsync(new Uri($"ms-windows-store://review/?ProductId={Consts.ProductID}"));
            });
        }

        public DelegateCommand Github
        {
            get => new DelegateCommand(async () =>
            {
                await Launcher.LaunchUriAsync(new Uri(Consts.Github));
            });
        }

        public DelegateCommand ReportABug
        {
            get => new DelegateCommand(async () =>
            {
                await Launcher.LaunchUriAsync(new Uri($"feedback-hub:"));
            });
        }

        public DelegateCommand GetExtensions
        {
            get => new DelegateCommand(async () =>
            {
                await Launcher.LaunchUriAsync(new Uri($"ms-windows-store://search/?query={Consts.ExtensionContract}"));
            });
        }

        public DelegateCommand About
        {
            get => new DelegateCommand(() =>
            {
                MainPage.Current.Navigate(typeof(AboutPage));
            });
        }

        private bool onlinePurchase;
        public bool OnlinePurchase
        {
            get { return onlinePurchase; }
            set { SetProperty(ref onlinePurchase, value); }
        }

        private double playerVolume;
        public double PlayerVolume
        {
            get { return playerVolume; }
            set
            {
                if (!value.AlmostEqualTo(playerVolume))
                {
                    Player.Current.ChangeVolume(value);
                    settings.PlayerVolume = value;
                    settings.Save();
                }

                SetProperty(ref playerVolume, value);
            }
        }

        private int crrentLyricIndex = -1;
        public int CurrentLyricIndex
        {
            get { return crrentLyricIndex; }
            set
            {
                SetProperty(ref crrentLyricIndex, value);
            }
        }

        private int currentOnlineIndex = -1;
        public int CurrentOnlineIndex
        {
            get { return currentOnlineIndex; }
            set
            {
                SetProperty(ref currentOnlineIndex, value);
            }
        }

        private bool debugModeEnabled;
        public bool DebugModeEnabled
        {
            get { return debugModeEnabled; }
            set
            {
                settings.DebugModeEnabled = value;
                settings.Save();
                SetProperty(ref debugModeEnabled, value);
            }
        }

        internal void ToggleEffectState(string tag)
        {
            switch (tag)
            {
                case "Threshold":
                    settings.AudioGraphEffects ^= Effects.Limiter;
                    break;
                case "Equalizer":
                    settings.AudioGraphEffects ^= Effects.Equalizer;
                    break;
                case "Reverb":
                    settings.AudioGraphEffects ^= Effects.Reverb;
                    break;
                default:
                    break;
            }

            settings.Save();

            EqualizerEnabled = settings.AudioGraphEffects.HasFlag(Effects.Equalizer);
            ThresholdEnabled = settings.AudioGraphEffects.HasFlag(Effects.Limiter);
            ReverbEnabled = settings.AudioGraphEffects.HasFlag(Effects.Reverb);
        }

        internal void ChangeLyricExt(object selectedItem)
        {
            if (selectedItem is ExtensionViewModel v)
            {
                settings.LyricExtensionID = v.UniqueId;
                settings.Save();
            }
        }

        internal void ChangeOnlineExt(object selectedItem)
        {
            if (selectedItem is ExtensionViewModel v)
            {
                settings.OnlineMusicExtensionID = v.UniqueId;
                settings.Save();
            }
        }

        internal async Task PurchaseOnlineExtension()
        {
            if (context == null)
            {
                context = StoreContext.GetDefault();
            }

            MainPage.Current.ShowModalUI(true, "Wating for Result");
            StorePurchaseResult result = await context.RequestPurchaseAsync(Consts.OnlineAddOnStoreID);


            // Capture the error message for the operation, if any.
            string extendedError = string.Empty;
            if (result.ExtendedError != null)
            {
                extendedError = result.ExtendedError.Message;
            }

            switch (result.Status)
            {
                case StorePurchaseStatus.AlreadyPurchased:
                case StorePurchaseStatus.Succeeded:
                    OnlinePurchase = true;
                    settings.OnlinePurchase = true;
                    settings.Save();
                    break;

                case StorePurchaseStatus.NotPurchased:
                    OnlinePurchase = false;
                    settings.OnlinePurchase = false;
                    settings.Save();
                    break;

                case StorePurchaseStatus.NetworkError:
                case StorePurchaseStatus.ServerError:
                default:
                    OnlinePurchase = false;
                    settings.OnlinePurchase = false;
                    settings.Save();
                    MainPage.Current.PopMessage("Purchase Error:\r\n" + extendedError);
                    break;
            }
            MainPage.Current.ShowModalUI(false);
        }

        public ObservableCollection<ExtensionViewModel> LyricExts { get; set; } = new ObservableCollection<ExtensionViewModel>();
        public ObservableCollection<ExtensionViewModel> OnlineExts { get; set; } = new ObservableCollection<ExtensionViewModel>();

        private bool equalizerEnabled;
        public bool EqualizerEnabled
        {
            get { return equalizerEnabled; }
            set { SetProperty(ref equalizerEnabled, value); }
        }

        private bool threshold;
        public bool ThresholdEnabled
        {
            get { return threshold; }
            set { SetProperty(ref threshold, value); }
        }

        private bool reverb;
        public bool ReverbEnabled
        {
            get { return reverb; }
            set { SetProperty(ref reverb, value); }
        }

        public SettingsPageViewModel()
        {
            settings = Settings.Load();
            PlayerVolume = settings.PlayerVolume;
            OnlinePurchase = settings.OnlinePurchase;
            EqualizerEnabled = settings.AudioGraphEffects.HasFlag(Effects.Equalizer);
            ThresholdEnabled = settings.AudioGraphEffects.HasFlag(Effects.Limiter);
            ReverbEnabled = settings.AudioGraphEffects.HasFlag(Effects.Reverb);
            DebugModeEnabled = settings.DebugModeEnabled;
        }

        public ObservableCollection<DeviceInformationViewModel> DevicList = new ObservableCollection<DeviceInformationViewModel>();
        private Settings settings;
        private StoreContext context;
        private AppExtensionCatalog _catalog;

        public async Task FindAllExtensions()
        {
            // load all the extensions currently installed
            IReadOnlyList<AppExtension> extensions = await _catalog.FindAllAsync();
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, async () =>
            {
                foreach (AppExtension ext in extensions)
                {
                    // load this extension
                    await LoadExtension(ext);
                }
                LyricExts.ToList().ForEach(async (x) => { await x.Load(); });
                OnlineExts.ToList().ForEach(async (x) => { await x.Load(); });
                try
                {
                    var f = LyricExts.First(x => x.UniqueId == (settings.LyricExtensionID.IsNullorEmpty() ? Consts.AppUserModelId + "$|$BuiltIn" : settings.LyricExtensionID));
                    if (f != null)
                    {
                        CurrentLyricIndex = LyricExts.IndexOf(f);
                    }
                    else
                    {
                        CurrentLyricIndex = -1;
                    }
                }
                catch (Exception)
                {
                    CurrentLyricIndex = -1;
                }
                try
                {
                    var f = OnlineExts.First(x => x.UniqueId == (settings.OnlineMusicExtensionID.IsNullorEmpty() ? Consts.AppUserModelId + "$|$BuiltIn" : settings.OnlineMusicExtensionID));
                    if (f != null)
                    {
                        CurrentOnlineIndex = OnlineExts.IndexOf(f);
                    }
                    else
                    {
                        CurrentOnlineIndex = -1;
                    }
                }
                catch (Exception)
                {
                    CurrentOnlineIndex = -1;
                }
            });
        }

        // loads an extension
        public async Task LoadExtension(AppExtension ext)
        {
            // get unique identifier for this extension
            string identifier = ext.AppInfo.AppUserModelId + "$|$" + ext.Id;

            // load the extension if the package is OK
            if (!(ext.Package.Status.VerifyIsOK()
#if !DEBUG
                    && settings.DebugModeEnabled ? true : ext.Package.SignatureKind == PackageSignatureKind.Store
#endif
                    ))
            {
                // if this package doesn't meet our requirements
                // ignore it and return
                return;
            }
            var properties = await ext.GetExtensionPropertiesAsync() as PropertySet;
            var cates = ((properties["Category"] as PropertySet)["#text"] as string).Split(';');
            foreach (var category in cates)
            {
                if (category == "Lyric")
                {
                    // if its already existing then this is an update
                    var existingLyricExt = LyricExts.Where(e => e.UniqueId == identifier).FirstOrDefault();
                    // new extension
                    if (existingLyricExt == null)
                    {
                        // get extension properties


                        LyricExts.Add(new ExtensionViewModel(ext, properties));
                    }
                    // update
                    else
                    {
                        // update the extension
                        await existingLyricExt.Update(ext);
                    }
                }

                if (category == "OnlineMusic")
                {
                    // if its already existing then this is an update
                    var existingOnlineExt = OnlineExts.Where(e => e.UniqueId == identifier).FirstOrDefault();
                    // new extension
                    if (existingOnlineExt == null)
                    {
                        // get extension properties


                        OnlineExts.Add(new ExtensionViewModel(ext, properties));
                    }
                    // update
                    else
                    {
                        // update the extension
                        await existingOnlineExt.Update(ext);
                    }
                }
            }

        }

        public async Task Init()
        {
            if (!OnlinePurchase)
            {
                if (context == null)
                {
                    context = StoreContext.GetDefault();
                }

                // Specify the kinds of add-ons to retrieve.
                string[] productKinds = { "Durable" };
                List<String> filterList = new List<string>(productKinds);

                // Specify the Store IDs of the products to retrieve.
                string[] storeIds = new string[] { Consts.OnlineAddOnStoreID };

                StoreProductQueryResult queryResult =
                    await context.GetStoreProductsAsync(filterList, storeIds);

                if (queryResult.ExtendedError != null)
                {
                    // The user may be offline or there might be some other server failure.
                    MainPage.Current.PopMessage($"ExtendedError: {queryResult.ExtendedError.Message}");
                    return;
                }

                foreach (KeyValuePair<string, StoreProduct> item in queryResult.Products)
                {
                    // Access the Store info for the product.
                    StoreProduct product = item.Value;
                    OnlinePurchase = product.IsInUserCollection;
                    settings.OnlinePurchase = product.IsInUserCollection;
                    settings.Save();
                }
            }

            _catalog = AppExtensionCatalog.Open(Consts.ExtensionContract);
            // set up extension management events
            // Scan all extensions
            await FindAllExtensions();

            string audioSelector = MediaDevice.GetAudioRenderSelector();
            var outputDevices = await DeviceInformation.FindAllAsync(audioSelector);
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                DevicList.Add(new DeviceInformationViewModel()
                {
                    Name = "System default",
                    ID = null,
                    Tag = null
                });

                foreach (var device in outputDevices)
                {
                    //var deviceItem = new ComboBoxItem();
                    //deviceItem.Content = device.Name;
                    //deviceItem.Tag = device;
                    //_audioDeviceComboBox.Items.Add(deviceItem);
                    DevicList.Add(new DeviceInformationViewModel()
                    {
                        Name = device.Name,
                        ID = device.Id,
                        Tag = device
                    });
                }

            });

            await Task.Delay(200);
            if (settings.OutputDeviceID.IsNullorEmpty())
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
                {
                    AudioSelectedIndex = 0;
                });
            }
            else
            {
                var index = DevicList.IndexOf(DevicList.First(x => x.ID == settings.OutputDeviceID));
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
                {
                    AudioSelectedIndex = index;
                });
            }
        }
    }

    class DeviceInformationViewModel : ViewModelBase
    {
        public string Name { get; set; }
        public string ID { get; set; }
        public DeviceInformation Tag { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
