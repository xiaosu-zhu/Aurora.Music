// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Aurora.Music.Core;
using Aurora.Music.ViewModels;
using Aurora.Shared.Extensions;
using Aurora.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
    public sealed partial class DoubanPage : Page
    {
        public DoubanPage()
        {
            this.InitializeComponent();
            MainPageViewModel.Current.Title = Consts.Localizer.GetString("DouText");
            MainPageViewModel.Current.NeedShowTitle = true;
            MainPageViewModel.Current.LeftTopColor = Resources["SystemControlForegroundBaseHighBrush"] as SolidColorBrush;
            MainPageViewModel.Current.NeedShowPanel = false;
            Context.PropertyChanged += Context_PropertyChanged;
        }

        private void Context_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Palette")
            {
                int n = TopPanel.Children.Count;
                while (n > 1)
                {
                    n--;
                    int k = Tools.Random.Next(n + 1);
                    TopPanel.Children.Move((uint)k, (uint)n);
                    TopPanel.Children.Move((uint)n - 1, (uint)k);
                }

            }
        }

        private void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            Context.Switch(e.ClickedItem as ChannelViewModel);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            Context.Detach();
            Context.PropertyChanged -= Context_PropertyChanged;
        }
    }
}
