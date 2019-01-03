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
    [UriActivate("podcast", Usage = ActivateUsage.SubNavigation)]
    public sealed partial class PodcastPage : Page, Controls.IRequestGoBack
    {
        public PodcastPage()
        {
            this.InitializeComponent();
        }
        public void RequestGoBack()
        {
            try
            {
                ConnectedAnimationService.GetForCurrentView().PrepareToAnimate(Consts.ArtistPageInAnimation + "_1", Title);
                ConnectedAnimationService.GetForCurrentView().PrepareToAnimate(Consts.ArtistPageInAnimation + "_2", Details);
            }
            catch (Exception)
            {
            }
            LibraryPage.Current.GoBack();
            UnloadObject(this);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            MainPageViewModel.Current.NeedShowBack = true;

            if (!(e.Parameter is int))
            {
                return;
            }
            await Context.Init((int)e.Parameter);
        }

        private async void AlbumList_ItemClick(object sender, ItemClickEventArgs e)
        {
            await Context.PlayAt(e.ClickedItem as SongViewModel);
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
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
            var model = (sender as ListViewBase).ItemFromContainer(requestedElement) as SongViewModel;
            if (requestedElement != sender)
            {
                var albumMenu = MainPage.Current.SongFlyout.Items.First(x => x.Name == "AlbumMenu") as MenuFlyoutItem;
                albumMenu.Text = model.Album;
                albumMenu.Visibility = Visibility.Visible;

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

        private void AlbumList_ContextCanceled(UIElement sender, RoutedEventArgs args)
        {
            MainPage.Current.SongFlyout.Hide();
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

            var headerHeight = (float)(HeaderGroup.ActualHeight);
            var finalHeight = (float)TitleBG.ActualHeight;


            var progressAnimation = EF.Conditional(-scrollingProperties.Translation.Y > headerHeight, EF.Conditional(-scrollingProperties.Translation.Y > headerHeight + finalHeight, 0, -scrollingProperties.Translation.Y - headerHeight - finalHeight), -finalHeight);

            // 0~1
            progressAnimation = (progressAnimation + finalHeight) / finalHeight;

            var toolbarVisual = ElementCompositionPreview.GetElementVisual(Toolbar);


            toolbarVisual.StartAnimation("Offset.Y", progressAnimation * 16 - 16);

            var offset = toolbarVisual.GetReference().GetScalarProperty("Offset.Y");

            toolbarVisual.StartAnimation("Opacity", progressAnimation);

            var moving = 80f;

            var movingAnimation = EF.Conditional(-scrollingProperties.Translation.Y > moving, 0f, moving + scrollingProperties.Translation.Y);

            var horiMovingAni = EF.Clamp(-scrollingProperties.Translation.Y / moving, 0, 1);
            horiMovingAni = EF.Lerp(0, (float)(80f - ImageGrid.Width), horiMovingAni);

            var scaleAnimation = EF.Clamp(-scrollingProperties.Translation.Y / moving, 0, 1);
            var textScaleAnimation = EF.Lerp(1, (float)(ToolbarTitle.ActualHeight / TitleText.Height), scaleAnimation);

            var titleVisual = ElementCompositionPreview.GetElementVisual(Title);
            titleVisual.StartAnimation("Offset.Y", movingAnimation);
            titleVisual.StartAnimation("Offset.X", horiMovingAni);


            var titleTextVisual = ElementCompositionPreview.GetElementVisual(TitleText);
            titleTextVisual.CenterPoint = new Vector3(32f, (float)TitleText.Height / 2, 0);
            titleTextVisual.StartAnimation("Scale.X", textScaleAnimation);
            titleTextVisual.StartAnimation("Scale.Y", textScaleAnimation);

            var bgVisual = ElementCompositionPreview.GetElementVisual(TitleBG);
            bgVisual.StartAnimation("Opacity", scaleAnimation);

            var marginTop = (float)((TitleBG.ActualHeight - 80f) / 2);

            var imgMovingAnimation = EF.Conditional(-scrollingProperties.Translation.Y > (moving - marginTop), marginTop, moving + scrollingProperties.Translation.Y);

            var imageScaleAnimation = EF.Lerp(1, (float)(80f / ImageGrid.Height), scaleAnimation);

            var imageVisual = ElementCompositionPreview.GetElementVisual(ImageGrid);

            imageVisual.CenterPoint = new Vector3(32f, 0, 0);
            imageVisual.StartAnimation("Scale.X", imageScaleAnimation);
            imageVisual.StartAnimation("Scale.Y", imageScaleAnimation);

            imageVisual.StartAnimation("Offset.Y", imgMovingAnimation);
        }
    }
}
