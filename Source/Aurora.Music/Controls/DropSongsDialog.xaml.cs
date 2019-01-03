// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Aurora.Music.Core.Models;

using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace Aurora.Music.Controls
{
    public sealed partial class DropSongsDialog : ContentDialog
    {
        internal ObservableCollection<StorageFile> DropList { get; set; } = new ObservableCollection<StorageFile>();

        public DropSongsDialog()
        {
            InitializeComponent();
            RequestedTheme = Settings.Current.Theme;
        }

        public DropSongsDialog(IList<StorageFile> files)
        {
            this.InitializeComponent();
            foreach (var item in files)
            {
                DropList.Add(item);
            }
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Settings.Current.CopyFileWhenActivated = true;
            Settings.Current.Save();
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Settings.Current.CopyFileWhenActivated = false;
            Settings.Current.Save();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Settings.Current.RememberFileActivatedAction = (sender as CheckBox).IsChecked ?? false;
            Settings.Current.Save();
        }
    }
}
