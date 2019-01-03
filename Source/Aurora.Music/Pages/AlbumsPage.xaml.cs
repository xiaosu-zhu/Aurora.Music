// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System;
using System.Linq;
using System.Numerics;

using Aurora.Music.Controls.ListItems;
using Aurora.Music.Core;
using Aurora.Music.ViewModels;
using Aurora.Shared.Extensions;
using Aurora.Shared.Helpers;

using ExpressionBuilder;

using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

using EF = ExpressionBuilder.ExpressionFunctions;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Aurora.Music.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    [UriActivate("albums", Usage = ActivateUsage.SubNavigation)]
    public sealed partial class AlbumsPage : Page, Controls.IRequestGoBack
    {
        private AlbumViewModel _clickedAlbum;

        public AlbumsPage()
        {
            InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Enabled;
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

            MainPageViewModel.Current.NeedShowBack = true;

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
            SortBox.SelectedIndex = Context.SortIndex;
            SortBox.SelectionChanged += ComboBox_SelectionChanged;
        }

        private void AlbumList_ItemClick(object sender, ItemClickEventArgs e)
        {
            var container = (AlbumList.ContainerFromItem(e.ClickedItem) as SelectorItem).ContentTemplateRoot as AlbumItem;
            container.PrepareConnectedAnimation();
            LibraryPage.Current.Navigate(typeof(AlbumDetailPage), e.ClickedItem);
            _clickedAlbum = e.ClickedItem as AlbumViewModel;
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

        private void AlbumItem_PlayAlbum(object sender, EventArgs e)
        {

        }

        private void AlbumList_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
        {
            MainPage.Current.IsInAppDrag = true;
            e.Data.RequestedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Link;
        }

        private void AlbumList_DragItemsCompleted(ListViewBase sender, DragItemsCompletedEventArgs args)
        {
            MainPage.Current.IsInAppDrag = false;
        }

        private async void HeaderGroup_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            await AlbumList.GetScrollViewer().ChangeViewAsync(null, 0, false);
        }

        private void AlbumList_Loaded(object sender, RoutedEventArgs e)
        {
            var scrollviewer = AlbumList.GetScrollViewer();
            var _scrollerPropertySet = ElementCompositionPreview.GetScrollViewerManipulationPropertySet(scrollviewer);
            var _compositor = _scrollerPropertySet.Compositor;

            // Get references to our property sets for use with ExpressionNodes
            var scrollingProperties = _scrollerPropertySet.GetSpecializedReference<ManipulationPropertySetReferenceNode>();

            var headerHeight = (float)(HeaderGroup.ActualHeight - (48f + HeaderGroup.Margin.Bottom));
            var toolbarHeight = (float)Toolbar.ActualHeight;


            var progressAnimation = EF.Conditional(-scrollingProperties.Translation.Y > headerHeight, EF.Conditional(-scrollingProperties.Translation.Y > headerHeight + toolbarHeight, 0, -scrollingProperties.Translation.Y - headerHeight - toolbarHeight), -toolbarHeight);

            // 0~1
            progressAnimation = (progressAnimation + toolbarHeight) / toolbarHeight;

            var toolbarVisual = ElementCompositionPreview.GetElementVisual(Toolbar);


            toolbarVisual.StartAnimation("Offset.Y", progressAnimation * 16 - 16);

            var bgVisual = ElementCompositionPreview.GetElementVisual(TitleBG);
            bgVisual.StartAnimation("Opacity", progressAnimation);
            toolbarVisual.StartAnimation("Opacity", progressAnimation);


            var moving = 80f;

            var movingAnimation = EF.Conditional(-scrollingProperties.Translation.Y > moving, 0f, moving + scrollingProperties.Translation.Y);

            var scaleAnimation = EF.Clamp(-scrollingProperties.Translation.Y / moving, 0, 1);
            scaleAnimation = EF.Lerp(1, (float)(ToolbarTitle.ActualHeight / TitleText.ActualHeight), scaleAnimation);

            var titleVisual = ElementCompositionPreview.GetElementVisual(Title);
            titleVisual.StartAnimation("Offset.Y", movingAnimation);

            var titleTextVisual = ElementCompositionPreview.GetElementVisual(TitleText);

            titleTextVisual.CenterPoint = new Vector3(0, (float)TitleText.ActualHeight / 2, 0);
            titleTextVisual.StartAnimation("Scale.X", scaleAnimation);
            titleTextVisual.StartAnimation("Scale.Y", scaleAnimation);
        }
    }
}
