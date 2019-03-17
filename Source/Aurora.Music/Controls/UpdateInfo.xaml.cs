// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System;

using Aurora.Music.Core;
using Aurora.Music.Core.Models;
using Aurora.Shared.Helpers;

using Windows.System;
using Windows.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace Aurora.Music.Controls
{
    public sealed partial class UpdateInfo : ContentDialog
    {
        public UpdateInfo()
        {
            InitializeComponent();
            RequestedTheme = Settings.Current.Theme;
            Title = string.Format(Consts.UpdateNoteTitle, SystemInfoHelper.GetPackageVer().ToVersionString());
            Note.Text = string.IsNullOrEmpty(Consts.UpdateNote) ? "We are continuously providing new features and bug fixes for Aurora Music" : Consts.UpdateNote;
        }

        private void ContentDialog_Unloaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            this.SizeChanged -= UpdateInfo_SizeChanged;
        }

        private void Root_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Note.Width = Root.ActualWidth;
            this.SizeChanged += UpdateInfo_SizeChanged;
        }

        private void UpdateInfo_SizeChanged(object sender, Windows.UI.Xaml.SizeChangedEventArgs e)
        {
            Note.Width = Root.ActualWidth;
        }

        private async void Note_LinkClicked(object sender, Microsoft.Toolkit.Uwp.UI.Controls.LinkClickedEventArgs e)
        {
            if (e.Link == "http://as0")
            {
                await Launcher.LaunchUriAsync(new Uri("as-music:"));
            }
            else if (e.Link == "http://as1")
            {
                await Launcher.LaunchUriAsync(new Uri("as-music:///settings/extension"));
            }
            else
            {
                await Launcher.LaunchUriAsync(new Uri(e.Link));
            }
        }
    }
}
