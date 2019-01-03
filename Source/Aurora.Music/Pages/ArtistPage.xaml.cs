// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

using Aurora.Music.Controls;
using Aurora.Music.Controls.ListItems;
using Aurora.Music.Core;
using Aurora.Music.ViewModels;
using Aurora.Shared.Extensions;
using Aurora.Shared.Helpers;

using ExpressionBuilder;

using Windows.System;
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
    [UriActivate("artist", Usage = ActivateUsage.SubNavigation)]
    public sealed partial class ArtistPage : Page, IRequestGoBack
    {
        private AlbumViewModel _clickedAlbum;
        private string _lastParameter;

        public ArtistPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Disabled;
        }

        public void RequestGoBack()
        {
            _lastParameter = Guid.NewGuid().ToString();
            ConnectedAnimationService.GetForCurrentView().PrepareToAnimate(Consts.ArtistPageInAnimation + "_1", TitleText);
            ConnectedAnimationService.GetForCurrentView().PrepareToAnimate(Consts.ArtistPageInAnimation + "_2", Image);
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

            try
            {
                if (!Context.AlbumList.IsNullorEmpty() && _clickedAlbum != null && ((ArtistViewModel)e.Parameter).RawName == _lastParameter)
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
                }
                else if (_clickedAlbum != null && ((ArtistViewModel)e.Parameter).RawName == _lastParameter)
                {
                    Context.Artist = (ArtistViewModel)e.Parameter;
                    await Context.Init((ArtistViewModel)e.Parameter);
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
                }
            }
            catch (Exception)
            {
            }
            if (e.Parameter is ArtistViewModel s && _lastParameter != s.RawName)
            {
                var t = Task.Run(async () =>
                {
                    await Context.Init(s);

                });
                _lastParameter = s.RawName;
                Context.Artist = s;
            }
        }

        private void AlbumList_ItemClick(object sender, ItemClickEventArgs e)
        {
            var container = (AlbumList.ContainerFromItem(e.ClickedItem) as SelectorItem).ContentTemplateRoot as AlbumItem;
            container.PrepareConnectedAnimation();
            LibraryPage.Current.Navigate(typeof(AlbumDetailPage), e.ClickedItem);
            _clickedAlbum = e.ClickedItem as AlbumViewModel;
        }

        private void StackPanel_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Panel s)
            {
                (s.Resources["PointerPressed"] as Storyboard).Begin();
            }
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

        private async void Descriptions_LinkClicked(object sender, Microsoft.Toolkit.Uwp.UI.Controls.LinkClickedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri(e.Link));
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

        private async void SongItem_Play(object sender, RoutedEventArgs e)
        {
            await Context.PlayAt(sender as SongViewModel);
        }

        private void SongItem_RequestMultiSelect(object sender, RoutedEventArgs e)
        {
            SongsList.SelectionMode = ListViewSelectionMode.Multiple;
            SongsList.IsItemClickEnabled = false;
            foreach (var item in Context.SongsList)
            {
                foreach (var song in item)
                {
                    song.ListMultiSelecting = true;
                }
            }
        }

        private async void SongsList_ItemClick(object sender, ItemClickEventArgs e)
        {
            await Context.PlayAt(e.ClickedItem as SongViewModel);
        }

        private void SongsList_ContextRequested(UIElement sender, ContextRequestedEventArgs args)
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

        private void SongsList_ContextCanceled(UIElement sender, RoutedEventArgs args)
        {
            MainPage.Current.SongFlyout.Hide();
        }

        private async void HeaderGroup_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            await Root.ChangeViewAsync(null, 0, false);
        }

        private void SongsList_Loaded(object sender, RoutedEventArgs e)
        {
            var scrollviewer = Root;
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
            horiMovingAni = EF.Lerp(0, (float)(80f - ImageGrid.Height), horiMovingAni);

            var scaleAnimation = EF.Clamp(-scrollingProperties.Translation.Y / moving, 0, 1);
            var textScaleAnimation = EF.Lerp(1, (float)(ToolbarTitle.ActualHeight / TitleText.ActualHeight), scaleAnimation);

            var titleVisual = ElementCompositionPreview.GetElementVisual(Title);
            titleVisual.StartAnimation("Offset.Y", movingAnimation);
            titleVisual.StartAnimation("Offset.X", horiMovingAni);


            var titleTextVisual = ElementCompositionPreview.GetElementVisual(TitleText);
            titleTextVisual.CenterPoint = new Vector3(32f, (float)TitleText.ActualHeight / 2, 0);
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

            Task.Run(async () =>
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
                {
                    var ani = ConnectedAnimationService.GetForCurrentView().GetAnimation(Consts.ArtistPageInAnimation + "_1");
                    if (ani != null)
                    {
                        ani.TryStart(TitleText, new UIElement[] { Details });
                    }
                    ani = ConnectedAnimationService.GetForCurrentView().GetAnimation(Consts.ArtistPageInAnimation + "_2");
                    if (ani != null)
                    {
                        ani.TryStart(Image);
                    }
                });
            });
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Title.Width = ActualWidth - 240 - 32;
            ToolbarTitle.Width = Title.Width * ToolbarTitle.ActualHeight / TitleText.ActualHeight;
        }
    }
}
