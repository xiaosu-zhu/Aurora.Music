// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Aurora.Music.Core.Models;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace Aurora.Music.Controls
{
    public sealed partial class EaseAccess : ContentDialog
    {
        public EaseAccess()
        {
            InitializeComponent();
            RequestedTheme = Settings.Current.Theme;
        }

        private void Root_Loaded(object sender, RoutedEventArgs e)
        {
            Main.Width = Root.ActualWidth;
            SizeChanged += EaseAccess_SizeChanged;
        }

        private void EaseAccess_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Main.Width = Root.ActualWidth;
        }

        private void ContentDialog_Unloaded(object sender, RoutedEventArgs e)
        {
            SizeChanged -= EaseAccess_SizeChanged;
        }
    }
}
