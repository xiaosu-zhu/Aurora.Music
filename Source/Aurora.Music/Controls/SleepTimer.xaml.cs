// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System;

using Aurora.Music.Core;
using Aurora.Music.Core.Models;
using Windows.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace Aurora.Music.Controls
{
    public sealed partial class SleepTimer : ContentDialog
    {
        public SleepTimer()
        {
            InitializeComponent();
            RequestedTheme = Settings.Current.Theme;
            Time.Time = DateTime.Now.TimeOfDay + TimeSpan.FromMinutes(10);
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            var t = DateTime.Today;
            if (Time.Time < DateTime.Now.TimeOfDay)
            {
                t = t.AddDays(1) + Time.Time;
            }
            else
            {
                t += Time.Time;
            }

            SleepAction a;

            if ((bool)PlayPause.IsChecked)
            {
                a = SleepAction.Pause;
            }
            else if ((bool)Stop.IsChecked)
            {
                a = SleepAction.Stop;
            }
            else
            {
                a = SleepAction.Shutdown;
            }

            MainPage.Current.SetSleepTimer(t, a);
        }
    }
}
