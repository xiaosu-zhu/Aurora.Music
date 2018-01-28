// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Aurora.Music.Core;
using Aurora.Music.ViewModels;
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
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace Aurora.Music.Controls
{
    sealed partial class SearchResultDialog : ContentDialog
    {
        private GenericMusicItemViewModel currentItem;

        public SearchResultDialog()
        {
            this.InitializeComponent();
            Title = Consts.Localizer.GetString("OopsText");
            TitleText.Text = Consts.Localizer.GetString("SearchFailedText");
            IsSecondaryButtonEnabled = false;
            IsPrimaryButtonEnabled = false;
            OnlineIndicator.Visibility = Visibility.Collapsed;
        }

        public SearchResultDialog(GenericMusicItemViewModel param)
        {
            this.InitializeComponent();
            Artwork.Source = param.Artwork == null ? new BitmapImage(new Uri(Consts.NowPlaceholder)) : new BitmapImage(param.Artwork);
            TitleText.Text = param.Title;
            Description.Text = param.Description;
            Addtional.Text = param.Addtional;
            OnlineIndicator.Visibility = param.IsOnline ? Visibility.Visible : Visibility.Collapsed;
            currentItem = param;
        }

        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            MainPage.Current.ShowModalUI(true, "Prepare to Play");
            await MainPageViewModel.Current.InstantPlay(await currentItem.GetSongsAsync());
            MainPage.Current.ShowModalUI(false);
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {

        }
    }
}
