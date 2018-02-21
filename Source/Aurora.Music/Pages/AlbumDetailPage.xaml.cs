// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Aurora.Music.Controls;
using Aurora.Music.Core;
using Aurora.Music.ViewModels;
using Aurora.Shared.Extensions;
using System;
using System.Linq;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;


namespace Aurora.Music.Pages
{
    public sealed partial class AlbumDetailPage : Page, Controls.IRequestGoBack
    {

        public AlbumDetailPage()
        {
            this.InitializeComponent();

            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
            AppViewBackButtonVisibility.Visible;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            this.UnloadObject(this);
        }

        public void RequestGoBack()
        {
            ConnectedAnimationService.GetForCurrentView().PrepareToAnimate(Consts.AlbumItemConnectedAnimation + "_1", Title);
            ConnectedAnimationService.GetForCurrentView().PrepareToAnimate(Consts.AlbumItemConnectedAnimation + "_2", Image);
            LibraryPage.Current.GoBack();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is AlbumViewModel s)
            {
                var ani = ConnectedAnimationService.GetForCurrentView().GetAnimation(Consts.AlbumItemConnectedAnimation + "_1");
                if (ani != null)
                {
                    ani.TryStart(Title, new UIElement[] { Details });
                }
                ani = ConnectedAnimationService.GetForCurrentView().GetAnimation(Consts.AlbumItemConnectedAnimation + "_2");
                if (ani != null)
                {
                    ani.TryStart(Shadow, new UIElement[] { Image });
                }
                Context.HeroImage = s.ArtworkUri;
                await Context.GetSongsAsync(s);
            }
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            Context = null;
        }

        private async void PlayBtn_Click(object sender, RoutedEventArgs e)
        {
            await Context.PlayAt(sender as SongViewModel);
        }

        private async void Descriptions_LinkClicked(object sender, Microsoft.Toolkit.Uwp.UI.Controls.LinkClickedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri(e.Link));
        }

        private void SongList_ContextRequested(UIElement sender, Windows.UI.Xaml.Input.ContextRequestedEventArgs args)
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

        private void SongItem_RequestMultiSelect(object sender, RoutedEventArgs e)
        {
            SongList.SelectionMode = ListViewSelectionMode.Multiple;
            SongList.IsItemClickEnabled = false;
            foreach (var item in Context.SongList)
            {
                item.ListMultiSelecting = true;
            }
        }

        public Visibility SelectionModeToTitle(ListViewSelectionMode s)
        {
            if (s == ListViewSelectionMode.Multiple)
            {
                return Visibility.Collapsed;
            }
            return Visibility.Visible;
        }

        public Visibility SelectionModeToOther(ListViewSelectionMode s)
        {
            if (s != ListViewSelectionMode.Multiple)
            {
                return Visibility.Collapsed;
            }
            return Visibility.Visible;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            SongList.SelectionMode = ListViewSelectionMode.Single;
            SongList.IsItemClickEnabled = true;
            foreach (var item in Context.SongList)
            {
                item.ListMultiSelecting = false;
            }
        }

        private async void PlayAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            await MainPageViewModel.Current.InstantPlay(SongList.SelectedItems.Select(a => (a as SongViewModel).Song).ToList());
        }

        private async void PlayNextAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            await MainPageViewModel.Current.PlayNext(SongList.SelectedItems.Select(a => (a as SongViewModel).Song).ToList());
        }

        private async void AddCollectionAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            var s = new AddPlayList(SongList.SelectedItems.Select(a => (a as SongViewModel).ID).ToList());
            await s.ShowAsync();
        }

        private void ShareAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            var s = SongList.SelectedItems.Select(a => (a as SongViewModel)).ToList();
            MainPage.Current.Share(s);
        }

        private async void SongList_ItemClick(object sender, ItemClickEventArgs e)
        {
            await Context.PlayAt(e.ClickedItem as SongViewModel);
        }
    }
}
