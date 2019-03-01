// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Aurora.Music.Controls;
using Aurora.Music.Core;
using Aurora.Music.Core.Models;
using Aurora.Music.ViewModels;
using Aurora.Shared.Extensions;
using Aurora.Shared.Helpers;
using ExpressionBuilder;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using Windows.System.Threading;
using Windows.UI.Composition;
using Windows.UI.Core;
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
    [UriActivate("home", Usage = ActivateUsage.Navigation)]
    public sealed partial class HomePage : Page
    {
        private CompositionPropertySet _scrollerPropertySet;
        private Compositor _compositor;
        private CompositionPropertySet _props;

        public HomePage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            MainPageViewModel.Current.NeedShowTitle = false;
            MainPageViewModel.Current.NeedShowBack = false;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MainPage.Current.Navigate(typeof(LibraryPage));
        }

        private void Grid_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            ((sender as Grid).Resources["PointerOver"] as Storyboard).Begin();
        }

        private void Grid_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            ((sender as Grid).Resources["Normal"] as Storyboard).Begin();
        }

        private void Grid_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            ((sender as Grid).Resources["Pressed"] as Storyboard).Begin();
        }

        private void Grid_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            ((sender as Grid).Resources["PointerOver"] as Storyboard).Begin();
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ContentPanel.Width = ActualWidth;
        }

        private void Header_Loaded(object sender, RoutedEventArgs e)
        {
            var scrollviewer = MainScroller;
            _scrollerPropertySet = ElementCompositionPreview.GetScrollViewerManipulationPropertySet(scrollviewer);
            _compositor = _scrollerPropertySet.Compositor;

            _props = _compositor.CreatePropertySet();
            _props.InsertScalar("progress", 0);
            _props.InsertScalar("clampSize", (float)HeaderBG.Height);
            _props.InsertScalar("scaleFactor", 0.7f);

            // Get references to our property sets for use with ExpressionNodes
            var scrollingProperties = _scrollerPropertySet.GetSpecializedReference<ManipulationPropertySetReferenceNode>();
            var props = _props.GetReference();
            var progressNode = props.GetScalarProperty("progress");
            var clampSizeNode = props.GetScalarProperty("clampSize");
            var scaleFactorNode = props.GetScalarProperty("scaleFactor");

            // Create and start an ExpressionAnimation to track scroll progress over the desired distance
            var progressAnimation = EF.Clamp(-scrollingProperties.Translation.Y / clampSizeNode, 0, 1);
            _props.StartAnimation("progress", progressAnimation);

            var headerbgVisual = ElementCompositionPreview.GetElementVisual(HeaderBG);
            var bgblurOpacityAnimation = EF.Clamp(progressNode, 0, 1);
            headerbgVisual.StartAnimation("Opacity", bgblurOpacityAnimation);

            var headerVisual = ElementCompositionPreview.GetElementVisual(HeroTitle);
            var scaleAnimation = EF.Lerp(1, scaleFactorNode, progressNode);
            headerVisual.StartAnimation("Scale.X", scaleAnimation);
            headerVisual.StartAnimation("Scale.Y", scaleAnimation);

            var offsetAnimation = EF.Lerp(160f, 32f, progressNode);
            var opacityAnimation = EF.Lerp(1f, 0.6f, progressNode);

            var containerVisual = ElementCompositionPreview.GetElementVisual(TextContainer);
            containerVisual.StartAnimation("Offset.Y", offsetAnimation);
            containerVisual.StartAnimation("Opacity", opacityAnimation);
        }

        private async void FavList_ItemClick(object sender, ItemClickEventArgs e)
        {
            var collection = (sender as ListView).ItemsSource as ObservableCollection<GenericMusicItemViewModel>;
            var songs = new List<Core.Models.Song>();
            foreach (var item in collection)
            {
                songs.AddRange(await item.GetSongsAsync());
            }
            var current = (await (e.ClickedItem as GenericMusicItemViewModel).GetSongsAsync())[0];
            var start = songs.FindIndex(a => a.ID == current.ID);
            await MainPageViewModel.Current.InstantPlayAsync(songs, start);
        }

        private void HeroGrid_ContextRequested(UIElement sender, ContextRequestedEventArgs e)
        {
            // Walk up the tree to find the ListViewItem.
            // There may not be one if the click wasn't on an item.
            var requestedElement = (FrameworkElement)e.OriginalSource;
            while ((requestedElement != sender) && !(requestedElement is SelectorItem))
            {
                requestedElement = (FrameworkElement)VisualTreeHelper.GetParent(requestedElement);
            }
            var model = (sender as ListViewBase).ItemFromContainer(requestedElement) as GenericMusicItemViewModel;
            if (requestedElement != sender)
            {
                var albumMenu = MainPage.Current.SongFlyout.Items.First(x => x.Name == "AlbumMenu") as MenuFlyoutItem;

                switch (model.InnerType)
                {
                    case Core.Models.MediaType.Song:
                        albumMenu.Text = model.Description;
                        albumMenu.Visibility = Visibility.Visible;
                        break;
                    case Core.Models.MediaType.Album:
                        albumMenu.Text = model.Title;
                        albumMenu.Visibility = Visibility.Visible;
                        break;
                    case Core.Models.MediaType.PlayList:
                        albumMenu.Visibility = Visibility.Collapsed;
                        break;
                    case Core.Models.MediaType.Artist:
                        albumMenu.Text = model.Description;
                        albumMenu.Visibility = Visibility.Visible;
                        break;
                    default:
                        break;
                }

                // remove performers in flyout
                var index = MainPage.Current.SongFlyout.Items.IndexOf(albumMenu);
                while (!(MainPage.Current.SongFlyout.Items[index + 1] is MenuFlyoutSeparator))
                {
                    MainPage.Current.SongFlyout.Items.RemoveAt(index + 1);
                }

                if (!model.Addtional.IsNullorEmpty())
                {
                    var artists = model.Addtional.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries);

                    // add song's performers to flyout
                    if (!artists.IsNullorEmpty())
                    {
                        if (artists.Length == 1)
                        {
                            var menuItem = new MenuFlyoutItem()
                            {
                                Text = $"{artists[0]}",
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
                            foreach (var item in artists)
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
                }


                if (e.TryGetPosition(requestedElement, out var point))
                {
                    MainPage.Current.SongFlyout.ShowAt(requestedElement, point);
                }
                else
                {
                    MainPage.Current.SongFlyout.ShowAt(requestedElement);
                }

                e.Handled = true;
            }
        }

        private void HeroGrid_ContextCanceled(UIElement sender, RoutedEventArgs args)
        {
            MainPage.Current.SongFlyout.Hide();
        }

        private async void HeroGrid_ItemClick(object sender, ItemClickEventArgs e)
        {
            if ((e.ClickedItem as GenericMusicItemViewModel).IDs == null)
            {
                await Context.RestorePlayerStatus();
            }
            else
            {
                await MainPageViewModel.Current.InstantPlayAsync(await (e.ClickedItem as GenericMusicItemViewModel).GetSongsAsync());
            }
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            Context.Unload();
        }

        private void HeroPanel_PointerReleased(object sender, PointerRoutedEventArgs e)
        {

        }

        private async void OnlineList_ItemClick(object sender, ItemClickEventArgs e)
        {
            if ((e.ClickedItem as GenericMusicItemViewModel).InnerType == MediaType.Placeholder)
            {
                await Context.ShowMoreOnlineList();
                return;
            }
            MainPage.Current.ShowModalUI(true, Consts.Localizer.GetString("WaitingResultText"));
            var result = await Context.GetPlaylistAsync(e.ClickedItem as GenericMusicItemViewModel);
            if (result != null)
            {
                var dialog = new AlbumViewDialog(result);
                MainPage.Current.ShowModalUI(false);
                await dialog.ShowAsync();
            }
            else
            {
                MainPage.Current.ShowModalUI(false);
            }
        }

        private void ToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            Context.OnlineList.Clear();
            Settings.Current.ShowFeatured = false;
            Settings.Current.Save();
            MainPage.Current.PopMessage("You can re-enable featured list in settings");
            (sender as ToggleSwitch).Toggled -= ToggleSwitch_Toggled;
        }

        private void ToggleSwitch_Loaded(object sender, RoutedEventArgs e)
        {
            (sender as ToggleSwitch).IsOn = Settings.Current.ShowFeatured;
            (sender as ToggleSwitch).Toggled += ToggleSwitch_Toggled;
        }
    }
}
