// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System;

using Aurora.Music.Core;
using Aurora.Music.Core.Models;
using Aurora.Music.ViewModels;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace Aurora.Music.Controls
{
    sealed partial class SearchResultDialog : ContentDialog
    {
        private GenericMusicItemViewModel currentItem;

        public SearchResultDialog()
        {
            InitializeComponent();
            RequestedTheme = Settings.Current.Theme;
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
            MainPage.Current.ShowModalUI(true, Consts.Localizer.GetString("PrepareToPlay"));
            await MainPageViewModel.Current.InstantPlayAsync(await currentItem.GetSongsAsync());
            MainPage.Current.ShowModalUI(false);
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {

        }
    }
}
