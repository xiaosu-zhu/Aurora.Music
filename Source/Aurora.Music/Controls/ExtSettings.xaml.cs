// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System;

using Aurora.Music.Core.Models;

using Windows.System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Aurora.Music.Controls
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class ExtSettings : Page
    {
        public ExtSettings()
        {
            InitializeComponent();
            RequestedTheme = Settings.Current.Theme;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            switch (Settings.Current.PreferredBitRate)
            {
                case Bitrate._128:
                    a_128.IsChecked = true;
                    break;
                case Bitrate._192:
                    a_192.IsChecked = true;
                    break;
                case Bitrate._256:
                    a_256.IsChecked = true;
                    break;
                case Bitrate._320:
                    a_320.IsChecked = true;
                    break;
                default:
                    break;
            }
            a_128.Checked += RadioButton_Checked;
            a_192.Checked += RadioButton_Checked;
            a_256.Checked += RadioButton_Checked;
            a_320.Checked += RadioButton_Checked;

            count.Text = Settings.Current.PreferredSearchCount.ToString();
            count.TextChanged += Count_TextChanged;
        }

        private void Count_TextChanged(object sender, TextChangedEventArgs e)
        {
            count.TextChanged -= Count_TextChanged;
            if (uint.TryParse(count.Text, out uint i))
            {
                if (i == 0u)
                {
                    i = 1u;
                }
                Settings.Current.PreferredSearchCount = i;
                Settings.Current.Save();
            }
            if (!string.IsNullOrEmpty(count.Text))
            {
                count.Text = Settings.Current.PreferredSearchCount.ToString();
            }
            count.TextChanged += Count_TextChanged;
        }

        private void RadioButton_Checked(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if ((bool)a_128.IsChecked)
            {
                Settings.Current.PreferredBitRate = Bitrate._128;
            }
            if ((bool)a_192.IsChecked)
            {
                Settings.Current.PreferredBitRate = Bitrate._192;
            }
            if ((bool)a_256.IsChecked)
            {
                Settings.Current.PreferredBitRate = Bitrate._256;
            }
            if ((bool)a_320.IsChecked)
            {
                Settings.Current.PreferredBitRate = Bitrate._320;
            }
            Settings.Current.Save();
        }

        private async void Button_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("http://gecimi.com/"));
        }

        private async void Button_Click_1(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("https://www.last.fm/"));
        }
    }
}
