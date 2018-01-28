// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
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

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace Aurora.Music.Controls
{
    public sealed partial class EaseAccess : ContentDialog
    {
        public EaseAccess()
        {
            this.InitializeComponent();
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
