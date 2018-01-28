// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Aurora.Music.Core;
using Aurora.Music.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace Aurora.Music.Controls
{
    public sealed partial class ArtistViewDialog : ContentDialog
    {
        public ArtistViewDialog()
        {
            this.InitializeComponent();
        }

        internal ArtistViewDialog(ArtistViewModel artist)
        {
            MainPage.Current.ShowModalUI(true, "Prepare to Play");
            this.InitializeComponent();
            Context.Artist = artist;
            Title = artist.Name;

            Task.Run(async () =>
            {
                await Context.GetAlbums(artist);
                MainPage.Current.ShowModalUI(false);
            });
        }

        public string AlbumCount(int count)
        {
            return SmartFormat.Smart.Format(Consts.Localizer.GetString("SmartAlbums"), count);
        }

        private async void AlbumList_ItemClick(object sender, ItemClickEventArgs e)
        {
            this.Hide();
            var dialog = new AlbumViewDialog((e.ClickedItem) as AlbumViewModel);
            await dialog.ShowAsync();
        }

        private async void PlayAlbum_Click(object sender, RoutedEventArgs e)
        {
            await Context.PlayAlbumAsync(sender as AlbumViewModel);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (DescriIndicator.Glyph == "\uE018")
            {
                DescriIndicator.Glyph = "\uE09D";
                Descriptions.Height = 75;
            }
            else
            {
                DescriIndicator.Glyph = "\uE018";
                Descriptions.Height = double.NaN;
            }
        }

        private async void Descriptions_LinkClicked(object sender, Microsoft.Toolkit.Uwp.UI.Controls.LinkClickedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri(e.Link));
        }

        private void Flyout_Click(object sender, RoutedEventArgs e)
        {
            // Walk up the tree to find the ListViewItem.
            // There may not be one if the click wasn't on an item.
            var requestedElement = (FrameworkElement)e.OriginalSource;
            while ((requestedElement != AlbumList) && !(requestedElement is SelectorItem))
            {
                requestedElement = (FrameworkElement)VisualTreeHelper.GetParent(requestedElement);
            }
            var model = AlbumList.ItemFromContainer(requestedElement) as AlbumViewModel;
            if (requestedElement != AlbumList)
            {
                var albumMenu = MainPage.Current.SongFlyout.Items.First(x => x.Name == "AlbumMenu") as MenuFlyoutItem;
                albumMenu.Text = model.Name;
                albumMenu.Visibility = Visibility.Collapsed;

                // remove performers in flyout
                var index = MainPage.Current.SongFlyout.Items.IndexOf(albumMenu);
                while (!(MainPage.Current.SongFlyout.Items[index + 1] is MenuFlyoutSeparator))
                {
                    MainPage.Current.SongFlyout.Items.RemoveAt(index + 1);
                }

                MainPage.Current.SongFlyout.ShowAt(requestedElement);
            }
        }

        private void AlbumList_ContextRequested(UIElement sender, ContextRequestedEventArgs args)
        {
            // Walk up the tree to find the ListViewItem.
            // There may not be one if the click wasn't on an item.
            var requestedElement = (FrameworkElement)args.OriginalSource;
            while ((requestedElement != sender) && !(requestedElement is SelectorItem))
            {
                requestedElement = (FrameworkElement)VisualTreeHelper.GetParent(requestedElement);
            }
            var model = (sender as ListViewBase).ItemFromContainer(requestedElement) as AlbumViewModel;
            if (requestedElement != sender)
            {
                var albumMenu = MainPage.Current.SongFlyout.Items.First(x => x.Name == "AlbumMenu") as MenuFlyoutItem;
                albumMenu.Text = model.Name;
                albumMenu.Visibility = Visibility.Collapsed;

                // remove performers in flyout
                var index = MainPage.Current.SongFlyout.Items.IndexOf(albumMenu);
                while (!(MainPage.Current.SongFlyout.Items[index + 1] is MenuFlyoutSeparator))
                {
                    MainPage.Current.SongFlyout.Items.RemoveAt(index + 1);
                }

                if (args.TryGetPosition(requestedElement, out var point))
                {
                    MainPage.Current.SongFlyout.ShowAt(requestedElement, point);
                }
                else
                {
                    MainPage.Current.SongFlyout.ShowAt(requestedElement);
                }

                args.Handled = true;
            }
        }

        private void AlbumList_ContextCanceled(UIElement sender, RoutedEventArgs args)
        {
            MainPage.Current.SongFlyout.Hide();
        }
    }
}
