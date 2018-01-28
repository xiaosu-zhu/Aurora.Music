// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Aurora.Music.Core;
using Aurora.Music.Core.Storage;
using Aurora.Music.ViewModels;
using Aurora.Shared.Extensions;
using Aurora.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.System.Threading;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace Aurora.Music.Controls
{
    public sealed partial class AlbumViewDialog : ContentDialog
    {
        internal ObservableCollection<SongViewModel> SongList = new ObservableCollection<SongViewModel>();
        private AlbumViewModel album;

        public AlbumViewDialog()
        {
            this.InitializeComponent();
        }

        public event EventHandler<Uri> UpdateArtwork;

        internal AlbumViewDialog(AlbumViewModel album)
        {
            this.InitializeComponent();
            if (album == null)
            {
                Title = Consts.Localizer.GetString("OopsText");
                IsPrimaryButtonEnabled = false;
                Album.Text = Consts.Localizer.GetString("SearchFailedText");
                Artist.Visibility = Visibility.Collapsed;
                Brief.Visibility = Visibility.Collapsed;
                Descriptions.Visibility = Visibility.Collapsed;

            }
            this.album = album;
            var songs = AsyncHelper.RunSync(async () => { return await album.GetSongsAsync(); });
            uint i = 0;
            foreach (var item in songs)
            {
                SongList.Add(new SongViewModel(item)
                {
                    Index = i++,
                });
            }
            foreach (var item in SongList)
            {
                item.RefreshFav();
            }
            Album.Text = album.Name;
            Artwork.Source = new BitmapImage(album.ArtworkUri ?? new Uri(Consts.NowPlaceholder));
            Artist.Text = album.GetFormattedArtists();
            Brief.Text = album.GetBrief();
            if (album.Description.IsNullorEmpty())
            {
                var t = ThreadPool.RunAsync(async x =>
                {
                    var info = await MainPageViewModel.Current.GetAlbumInfoAsync(album.Name, album.AlbumArtists.FirstOrDefault());
                    await CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
                    {
                        if (info != null)
                        {
                            if (album.Artwork == null && info.AltArtwork != null)
                            {
                                Artwork.Source = new BitmapImage(info.AltArtwork);
                                UpdateArtwork?.Invoke(this, info.AltArtwork);
                                var task = ThreadPool.RunAsync(async k =>
                                {
                                    if (!album.IsOnline)
                                    {
                                        await SQLOperator.Current().UpdateAlbumArtworkAsync(album.ID, info.AltArtwork.OriginalString);
                                    }
                                });
                            }
                            Descriptions.Text = info.Description;
                        }
                        else
                        {
                            Descriptions.Text = $"# {Consts.Localizer.GetString("LocaAlbumTitle")}";
                        }
                    });
                });
            }
            else
            {
                Descriptions.Text = album.Description;
            }
        }

        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            MainPage.Current.ShowModalUI(true, "Prepare to Play");
            await MainPageViewModel.Current.InstantPlay(await album.GetSongsAsync());
            MainPage.Current.ShowModalUI(false);
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void DetailPanel_Click(object sender, RoutedEventArgs e)
        {
            if (DescriIndicator.Glyph == "\uE018")
            {
                DetailPanel.SetValue(Grid.ColumnProperty, 1);
                DetailPanel.SetValue(Grid.ColumnSpanProperty, 1);
                DescriIndicator.Glyph = "\uE09D";
                Descriptions.Height = 75;
            }
            else
            {
                DetailPanel.SetValue(Grid.ColumnProperty, 0);
                DetailPanel.SetValue(Grid.ColumnSpanProperty, 2);
                DescriIndicator.Glyph = "\uE018";
                Descriptions.Height = double.NaN;
            }
        }

        private async void Descriptions_LinkClicked(object sender, Microsoft.Toolkit.Uwp.UI.Controls.LinkClickedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri(e.Link));
        }


        private void SongList_ContextRequested(UIElement sender, ContextRequestedEventArgs args)
        {
            // Walk up the tree to find the ListViewItem.
            // There may not be one if the click wasn't on an item.
            var requestedElement = (FrameworkElement)args.OriginalSource;
            while ((requestedElement != sender) && !(requestedElement is SelectorItem))
            {
                requestedElement = (FrameworkElement)VisualTreeHelper.GetParent(requestedElement);
            }
            var model = (sender as ListViewBase).ItemFromContainer(requestedElement) as SongViewModel;
            if (requestedElement != sender)
            {
                // set album name of flyout
                var albumMenu = MainPage.Current.SongFlyout.Items.First(x => x.Name == "AlbumMenu") as MenuFlyoutItem;
                albumMenu.Text = model.Album;
                albumMenu.Visibility = Visibility.Collapsed;

                // remove performers in flyout
                var index = MainPage.Current.SongFlyout.Items.IndexOf(albumMenu);
                while (!(MainPage.Current.SongFlyout.Items[index + 1] is MenuFlyoutSeparator))
                {
                    MainPage.Current.SongFlyout.Items.RemoveAt(index + 1);
                }
                // add song's performers to flyout
                if (!model.Song.Performers.IsNullorEmpty())
                {
                    if (model.Song.Performers.Length == 1)
                    {
                        var menuItem = new MenuFlyoutItem()
                        {
                            Text = $"{model.Song.Performers[0]}",
                            Icon = new FontIcon()
                            {
                                Glyph = "\uE136"
                            }
                        };
                        menuItem.Click += MainPage.Current.MenuFlyoutArtist_Click;
                        MainPage.Current.SongFlyout.Items.Insert(index + 1, menuItem);
                    }
                    else
                    {
                        var sub = new MenuFlyoutSubItem()
                        {
                            Text = $"{Consts.Localizer.GetString("PerformersText")}:",
                            Icon = new FontIcon()
                            {
                                Glyph = "\uE136"
                            }
                        };
                        foreach (var item in model.Song.Performers)
                        {
                            var menuItem = new MenuFlyoutItem()
                            {
                                Text = item
                            };
                            menuItem.Click += MainPage.Current.MenuFlyoutArtist_Click;
                            sub.Items.Add(menuItem);
                        }
                        MainPage.Current.SongFlyout.Items.Insert(index + 1, sub);
                    }
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

        private void SongList_ContextCanceled(UIElement sender, RoutedEventArgs args)
        {
            MainPage.Current.SongFlyout.Hide();
        }
    }
}
