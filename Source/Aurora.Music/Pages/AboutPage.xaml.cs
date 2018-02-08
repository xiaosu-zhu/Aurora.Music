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
    public sealed partial class AboutPage : Page
    {
        public AboutPage()
        {
            this.InitializeComponent();
            MainPageViewModel.Current.Title = Consts.Localizer.GetString("AboutText");
            MainPageViewModel.Current.NeedShowTitle = true;
            MainPageViewModel.Current.LeftTopColor = Resources["SystemControlForegroundBaseHighBrush"] as SolidColorBrush;

            BuildText = string.Format(Consts.Localizer.GetString("BuildText"), SystemInfoHelper.GetPackageVer());
        }

        public string BuildText { get; set; }

        private async void Hyperlink_Click(Windows.UI.Xaml.Documents.Hyperlink sender, Windows.UI.Xaml.Documents.HyperlinkClickEventArgs args)
        {
            await Launcher.LaunchUriAsync(new Uri("https://unsplash.com/"));
        }

        private async void Hyperlink_Click_1(Windows.UI.Xaml.Documents.Hyperlink sender, Windows.UI.Xaml.Documents.HyperlinkClickEventArgs args)
        {
            await Launcher.LaunchUriAsync(new Uri("https://aurorastudio.oneskyapp.com/collaboration/project?id=141901"));
        }

        private async void Button_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            OpenSource o = new OpenSource();
            await o.ShowAsync();
        }
    }
}
