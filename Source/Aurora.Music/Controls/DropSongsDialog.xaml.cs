using Aurora.Music.Core.Models;
using Aurora.Music.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public sealed partial class DropSongsDialog : ContentDialog
    {
        internal ObservableCollection<SongViewModel> DropList { get; set; } = new ObservableCollection<SongViewModel>();

        public DropSongsDialog()
        {
            this.InitializeComponent();
        }

        public DropSongsDialog(IList<Song> songs)
        {
            this.InitializeComponent();
            foreach (var item in songs)
            {
                DropList.Add(new SongViewModel(item));
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
