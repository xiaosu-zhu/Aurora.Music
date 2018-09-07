// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System;
using System.Linq;
using System.Numerics;
using Aurora.Music.Core;
using Aurora.Music.ViewModels;
using Aurora.Shared.Extensions;
using Aurora.Shared.Helpers;

using ExpressionBuilder;

using Windows.System.Threading;
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
    [UriActivate("artists", Usage = ActivateUsage.SubNavigation)]
    public sealed partial class ArtistsPage : Page
    {
        private ArtistViewModel _clickedArtist;

        public ArtistsPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;

            var t = ThreadPool.RunAsync(async x =>
            {
                await Context.GetArtists();
            });
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (!Context.ArtistList.IsNullorEmpty() && _clickedArtist != null)
            {
                ArtistList.ScrollIntoView(_clickedArtist);
                var ani = ConnectedAnimationService.GetForCurrentView().GetAnimation(Consts.ArtistPageInAnimation + "_1");
                if (ani != null)
                {
                    await ArtistList.TryStartConnectedAnimationAsync(ani, _clickedArtist, "ArtistName");
                }
                ani = ConnectedAnimationService.GetForCurrentView().GetAnimation(Consts.ArtistPageInAnimation + "_2");
                if (ani != null)
                {
                    await ArtistList.TryStartConnectedAnimationAsync(ani, _clickedArtist, "ArtistImage");
                }
                return;
            }
        }

        private void ArtistCell_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            //if (sender is FrameworkElement s)
            //{
            //    (s.Resources["PointerOver"] as Storyboard).Begin();
            //}
        }

        private void ArtistCell_PointerExited(object sender, PointerRoutedEventArgs e)
        {
        }

        private void ArtistCell_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
        }

        private void ArtistCell_PointerReleased(object sender, PointerRoutedEventArgs e)
        {

        }

        private void AlbumList_ItemClick(object sender, ItemClickEventArgs e)
        {

        }

        private void ArtistList_ItemClick(object sender, ItemClickEventArgs e)
        {
            ArtistList.PrepareConnectedAnimation(Consts.ArtistPageInAnimation + "_1", e.ClickedItem, "ArtistName");
            ArtistList.PrepareConnectedAnimation(Consts.ArtistPageInAnimation + "_2", e.ClickedItem, "ArtistImage");

            LibraryPage.Current.Navigate(typeof(ArtistPage), (e.ClickedItem as ArtistViewModel));
            _clickedArtist = e.ClickedItem as ArtistViewModel;
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
        }

        private void ArtistList_ContextRequested(UIElement sender, ContextRequestedEventArgs args)
        {
            // Walk up the tree to find the ListViewItem.
            // There may not be one if the click wasn't on an item.
            var requestedElement = (FrameworkElement)args.OriginalSource;
            while ((requestedElement != sender) && !(requestedElement is SelectorItem))
            {
                requestedElement = (FrameworkElement)VisualTreeHelper.GetParent(requestedElement);
            }
            var model = (sender as ListViewBase).ItemFromContainer(requestedElement) as ArtistViewModel;
            if (requestedElement != sender)
            {
                var albumMenu = MainPage.Current.SongFlyout.Items.First(x => x.Name == "AlbumMenu") as MenuFlyoutItem;
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

        private void ArtistList_ContextCanceled(UIElement sender, RoutedEventArgs args)
        {
            MainPage.Current.SongFlyout.Hide();
        }

        private async void HeaderGroup_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            await ArtistList.GetScrollViewer().ChangeViewAsync(null, 0, false);
        }

        private void ArtistList_Loaded(object sender, RoutedEventArgs e)
        {
            var scrollviewer = ArtistList.GetScrollViewer();
            var _scrollerPropertySet = ElementCompositionPreview.GetScrollViewerManipulationPropertySet(scrollviewer);
            var _compositor = _scrollerPropertySet.Compositor;

            // Get references to our property sets for use with ExpressionNodes
            var scrollingProperties = _scrollerPropertySet.GetSpecializedReference<ManipulationPropertySetReferenceNode>();

            var headerHeight = (float)(HeaderGroup.ActualHeight - (0f + HeaderGroup.Margin.Bottom));
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
