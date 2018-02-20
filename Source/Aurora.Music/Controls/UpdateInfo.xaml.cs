// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Aurora.Music.Core;
using Aurora.Shared.Helpers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace Aurora.Music.Controls
{
    public sealed partial class UpdateInfo : ContentDialog
    {
        public UpdateInfo()
        {
            this.InitializeComponent();
            Title = string.Format(Consts.UpdateNoteTitle, SystemInfoHelper.GetPackageVer());
            Note.Text = Consts.UpdateNote;
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
    }
}
