// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Aurora.Music.Controls;
using Aurora.Music.Core.Extension;
using Aurora.Music.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Aurora.Music.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class PodcastMarket : Page
    {
        public PodcastMarket()
        {
            this.InitializeComponent();
            MainPageViewModel.Current.NeedShowTitle = true;
            MainPageViewModel.Current.Title = string.Empty;
            MainPageViewModel.Current.LeftTopColor = Resources["SystemControlForegroundBaseHighBrush"] as SolidColorBrush;
            Task.Run(() => { Context.Fetch(); });
        }

        private void ScrollViewer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Main.Width = ActualWidth;
        }

        private async void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var res = await ITunesSearcher.LookUp((e.ClickedItem as GenericMusicItemViewModel).OnlineAlbumID);
            var dialog = new PodcastDialog(new GenericMusicItemViewModel(res.First()));
            await dialog.ShowAsync();
        }
    }
}
