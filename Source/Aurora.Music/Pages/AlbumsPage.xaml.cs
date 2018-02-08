// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Aurora.Music.Controls.ListItems;
using Aurora.Music.Core;
using Aurora.Music.ViewModels;
using Aurora.Shared.Extensions;
using System;
using System.Linq;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Aurora.Music.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class AlbumsPage : Page, Controls.IRequestGoBack
    {
        private AlbumViewModel _clickedAlbum;

        public AlbumsPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        public void RequestGoBack()
        {
            ConnectedAnimationService.GetForCurrentView().PrepareToAnimate(Consts.ArtistPageInAnimation + "_1", Title);
            ConnectedAnimationService.GetForCurrentView().PrepareToAnimate(Consts.ArtistPageInAnimation + "_2", Details);
            LibraryPage.Current.GoBack();
            UnloadObject(this);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
            AppViewBackButtonVisibility.Visible;

            if (!Context.AlbumList.IsNullorEmpty() && _clickedAlbum != null)
            {
                AlbumList.ScrollIntoView(_clickedAlbum);
                var container = (AlbumList.ContainerFromItem(_clickedAlbum) as SelectorItem).ContentTemplateRoot as AlbumItem;
                var ani = ConnectedAnimationService.GetForCurrentView().GetAnimation(Consts.AlbumItemConnectedAnimation + "_1");
                if (ani != null)
                {
                    container.StartConnectedAnimation(ani, "AlbumName");
                }
                ani = ConnectedAnimationService.GetForCurrentView().GetAnimation(Consts.AlbumItemConnectedAnimation + "_2");
                if (ani != null)
                {
                    container.StartConnectedAnimation(ani, "Arts");
                }
                return;
            }
            else if (_clickedAlbum != null)
            {
                await Context.GetAlbums();
                AlbumList.ScrollIntoView(_clickedAlbum);
                var container = (AlbumList.ContainerFromItem(_clickedAlbum) as SelectorItem).ContentTemplateRoot as AlbumItem;
                var ani = ConnectedAnimationService.GetForCurrentView().GetAnimation(Consts.AlbumItemConnectedAnimation + "_1");
                if (ani != null)
                {
                    container.StartConnectedAnimation(ani, "AlbumName");
                }
                ani = ConnectedAnimationService.GetForCurrentView().GetAnimation(Consts.AlbumItemConnectedAnimation + "_2");
                if (ani != null)
                {
                    container.StartConnectedAnimation(ani, "Arts");
                }
                return;
            }
            else
            {
                await Context.GetAlbums();
            }
            SortBox.SelectionChanged -= ComboBox_SelectionChanged;
            SortBox.SelectionChanged += ComboBox_SelectionChanged;
        }

        private void AlbumList_ItemClick(object sender, ItemClickEventArgs e)
        {
            var container = (AlbumList.ContainerFromItem(e.ClickedItem) as SelectorItem).ContentTemplateRoot as AlbumItem;
            container.PrePareConnectedAnimation();
            LibraryPage.Current.Navigate(typeof(AlbumDetailPage), e.ClickedItem);
            _clickedAlbum = e.ClickedItem as AlbumViewModel;
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
        }

        private async void PlayAlbum_Click(object sender, RoutedEventArgs e)
        {
            await Context.PlayAlbumAsync(sender as AlbumViewModel);
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

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var box = sender as ComboBox;
            Context.ChangeSort(box.SelectedIndex);
        }

        private void SemanticZoom_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            //if (e.Cumulative.Expansion > 20)
            //{
            //    Root.IsZoomedInViewActive = true;

            //}
            //else if (e.Cumulative.Expansion < -20)
            //{
            //    Root.IsZoomedInViewActive = false;
            //}
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
                // add song's performers to flyout
                if (!model.AlbumArtists.IsNullorEmpty())
                {
                    if (model.AlbumArtists.Length == 1)
                    {
                        var menuItem = new MenuFlyoutItem()
                        {
                            Text = $"{model.AlbumArtists[0]}",
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
                        foreach (var item in model.AlbumArtists)
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

        private void AlbumList_ContextCanceled(UIElement sender, RoutedEventArgs args)
        {
            MainPage.Current.SongFlyout.Hide();
        }

        private void AlbumList_DragStarting(UIElement sender, DragStartingEventArgs args)
        {
            var d = args.GetDeferral();
            args.AllowedOperations = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Link;
        }

        private void AlbumItem_PlayAlbum(object sender, EventArgs e)
        {

        }
    }
}
