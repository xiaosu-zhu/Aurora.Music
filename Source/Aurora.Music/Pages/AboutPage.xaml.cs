// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Aurora.Music.Controls;
using Aurora.Music.Core;
using Aurora.Music.ViewModels;
using Aurora.Shared.Helpers;
using System;
using Windows.System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Aurora.Music.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    [UriActivate("about", Usage = ActivateUsage.Navigation)]
    public sealed partial class AboutPage : Page
    {
        public AboutPage()
        {
            BuildText = SystemInfoHelper.GetPackageVer();
            this.InitializeComponent();
            MainPageViewModel.Current.Title = Consts.Localizer.GetString("AboutText");
            MainPageViewModel.Current.NeedShowTitle = true;
            MainPageViewModel.Current.LeftTopColor = Resources["SystemControlForegroundBaseHighBrush"] as SolidColorBrush;
        }

        public string BuildText { get; set; }

        private async void OpenSource(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            OpenSource o = new OpenSource();
            await o.ShowAsync();
        }

        private async void Github(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("https://github.com/pkzxs/Aurora.Music"));
        }

        private async void Tranlate(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("https://aurorastudio.oneskyapp.com/collaboration/project?id=141901"));
        }

        private async void UnSplash(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("https://unsplash.com/"));
        }

        private async void MarkdownTextBlock_LinkClicked(object sender, Microsoft.Toolkit.Uwp.UI.Controls.LinkClickedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri(e.Link));
        }

        private async void Comment(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri($"ms-windows-store://review/?ProductId={Consts.ProductID}"));
        }

        private async void Report(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("https://github.com/pkzxs/Aurora.Music/issues"));
        }

        private async void Extension(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri($"ms-windows-store://search/?query={Consts.ExtensionContract}"));
        }

        private async void Privacy(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("http://198.181.41.120/privacypolicy.htm"));
        }

        private async void EaseAccess(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var u = new EaseAccess();
            await u.ShowAsync();
        }

        private async void Update(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var u = new UpdateInfo();
            await u.ShowAsync();
        }
    }
}
