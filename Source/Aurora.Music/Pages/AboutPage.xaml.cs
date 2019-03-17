// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Aurora.Music.Controls;
using Aurora.Music.Core;
using Aurora.Music.ViewModels;
using Aurora.Shared.Helpers;
using System;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Services.Store;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Aurora.Music.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    [UriActivate("about", Usage = ActivateUsage.Navigation)]
    public sealed partial class AboutPage : Page
    {
        private StoreContext context;

        public AboutPage()
        {
            BuildText = SystemInfoHelper.GetPackageVer().ToVersionString();
            this.InitializeComponent();
            MainPageViewModel.Current.Title = Consts.Localizer.GetString("AboutText");
            MainPageViewModel.Current.NeedShowTitle = true;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is bool b && b)
            {
                Task.Run(async () =>
                {
                    await Task.Delay(200);
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
                    {
                        Root.ChangeView(0, Root.ScrollableHeight, 1);
                    });
                });
            }
        }

        public string BuildText { get; set; }

        private async void OpenSource(object sender, RoutedEventArgs e)
        {
            var o = new OpenSource();
            await o.ShowAsync();
        }

        private async void Github(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("https://github.com/xiaosu-zhu/Aurora.Music"));
        }

        private async void Tranlate(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("https://aurorastudio.oneskyapp.com/collaboration/project?id=141901"));
        }

        private async void UnSplash(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("https://unsplash.com/"));
        }

        private async void MarkdownTextBlock_LinkClicked(object sender, Microsoft.Toolkit.Uwp.UI.Controls.LinkClickedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri(e.Link));
        }

        private async void Comment(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri($"ms-windows-store://review/?ProductId={Consts.ProductID}"));
        }

        private async void Report(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("https://github.com/xiaosu-zhu/Aurora.Music/issues"));
        }

        private async void Extension(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri($"ms-windows-store://search/?query=Aurora Music Extension"));
        }

        private async void Privacy(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("https://github.com/xiaosu-zhu/Aurora.Music/blob/master/Documentation/Privacy%20Policy.md"));
        }

        private async void EaseAccess(object sender, RoutedEventArgs e)
        {
            var u = new EaseAccess();
            await u.ShowAsync();
        }

        private async void Update(object sender, RoutedEventArgs e)
        {
            var u = new UpdateInfo();
            await u.ShowAsync();
        }

        private async Task ReportPurchased()
        {
            if (context == null)
            {
                context = StoreContext.GetDefault();
                // If your app is a desktop app that uses the Desktop Bridge, you
                // may need additional code to configure the StoreContext object.
                // For more info, see https://aka.ms/storecontext-for-desktop.
            }

            // This is an example for a Store-managed consumable, where you specify the actual number
            // of units that you want to report as consumed so the Store can update the remaining
            // balance. For a developer-managed consumable where you maintain the balance, specify 1
            // to just report the add-on as fulfilled to the Store.
            var trackingId = Guid.NewGuid();


            var result = await context.ReportConsumableFulfillmentAsync(Consts.DonationStoreID, 1, trackingId);

            switch (result.Status)
            {
                case StoreConsumableStatus.Succeeded:
                case StoreConsumableStatus.InsufficentQuantity:
                    break;

                case StoreConsumableStatus.NetworkError:
                case StoreConsumableStatus.ServerError:
                    MainPage.Current.PopMessage(result.ExtendedError.Message);
                    break;

                default:
                    break;
            }
        }

        private async void Donate(object sender, RoutedEventArgs e)
        {
            MainPage.Current.ShowModalUI(true, Consts.Localizer.GetString("WaitingResultText"));

            if (context == null)
            {
                context = StoreContext.GetDefault();
                // If your app is a desktop app that uses the Desktop Bridge, you
                // may need additional code to configure the StoreContext object.
                // For more info, see https://aka.ms/storecontext-for-desktop.
            }

            var result = await context.RequestPurchaseAsync(Consts.DonationStoreID);
            switch (result.Status)
            {
                case StorePurchaseStatus.Succeeded:
                case StorePurchaseStatus.AlreadyPurchased:
                    await ReportPurchased();
#pragma warning disable CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
                    Task.Run(async () => { await Task.Delay(2000); MainPage.Current.PopMessage("A huge thanks ❤"); });
#pragma warning restore CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
                    break;
                case StorePurchaseStatus.NotPurchased:
#pragma warning disable CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
                    Task.Run(async () => { await Task.Delay(2000); MainPage.Current.PopMessage("Please retry"); });
#pragma warning restore CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
                    break;
                case StorePurchaseStatus.NetworkError:
                case StorePurchaseStatus.ServerError:
#pragma warning disable CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
                    Task.Run(async () => { await Task.Delay(2000); MainPage.Current.PopMessage("Please retry:\r\n" + result.ExtendedError.Message); });
#pragma warning restore CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
                    break;
                default:
#pragma warning disable CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
                    Task.Run(async () => { await Task.Delay(2000); MainPage.Current.PopMessage("Please retry"); });
#pragma warning restore CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
                    break;
            }
            MainPage.Current.ShowModalUI(false);
        }
    }
}
