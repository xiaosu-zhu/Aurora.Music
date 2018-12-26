// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Aurora.Music.Controls;
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
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

using EF = ExpressionBuilder.ExpressionFunctions;


namespace Aurora.Music.Pages
{
    [UriActivate("album", Usage = ActivateUsage.SubNavigation)]
    public sealed partial class AlbumDetailPage : Page, IRequestGoBack
    {


        public AlbumDetailPage()
        {
            InitializeComponent();

            MainPageViewModel.Current.NeedShowBack = true;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            UnloadObject(this);
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
            // await Task.Delay(300);
            if (e.Parameter is AlbumViewModel s)
            {
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
                foreach (var song in item)
                {
                    song.ListMultiSelecting = true;
                }
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
                foreach (var song in item)
                {
                    song.ListMultiSelecting = false;
                }
            }
        }

        private async void PlayAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            await MainPageViewModel.Current.InstantPlayAsync(SongList.SelectedItems.Select(a => (a as SongViewModel).Song).ToList());
        }

        private async void PlayNextAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            await MainPageViewModel.Current.PlayNextAsync(SongList.SelectedItems.Select(a => (a as SongViewModel).Song).ToList());
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

        private async void HeaderGroup_PointerReleased(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            await SongList.GetScrollViewer().ChangeViewAsync(null, 0, false);
        }

        private void Image_ImageOpened(object sender, RoutedEventArgs e)
        {
            TitleGridMargin.X = Image.ActualWidth + 48;
            Col1.Width = new GridLength(Image.ActualWidth);
            ToolbarTitle.Margin = new Thickness(Image.ActualWidth * 80 / 200 + 32, 8, 0, 8);

            var scrollviewer = SongList.GetScrollViewer();
            var _scrollerPropertySet = ElementCompositionPreview.GetScrollViewerManipulationPropertySet(scrollviewer);
            var _compositor = _scrollerPropertySet.Compositor;
            var moving = 80f;

            // Get references to our property sets for use with ExpressionNodes
            var scrollingProperties = _scrollerPropertySet.GetSpecializedReference<ManipulationPropertySetReferenceNode>();

            var horiMovingAni = EF.Clamp(-scrollingProperties.Translation.Y / moving, 0, 1);
            horiMovingAni = EF.Lerp(0, (float)((80f / ImageGrid.ActualHeight * ImageGrid.ActualWidth) - ImageGrid.ActualWidth), horiMovingAni);

            var scaleAnimation = EF.Clamp(-scrollingProperties.Translation.Y / moving, 0, 1);
            var textScaleAnimation = EF.Lerp(1, (float)(ToolbarTitle.ActualHeight / TitleText.ActualHeight), scaleAnimation);

            var titleVisual = ElementCompositionPreview.GetElementVisual(Title);
            
            titleVisual.StopAnimation("offset.X");
            titleVisual.StartAnimation("Offset.X", horiMovingAni);

            Task.Run(async () =>
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
                {
                    var ani = ConnectedAnimationService.GetForCurrentView().GetAnimation(Consts.AlbumItemConnectedAnimation + "_1");
                    if (ani != null)
                    {
                        ani.TryStart(Title, new UIElement[] { Details });
                    }
                    ani = ConnectedAnimationService.GetForCurrentView().GetAnimation(Consts.AlbumItemConnectedAnimation + "_2");
                    if (ani != null)
                    {
                        ani.TryStart(Image);
                    }
                });
            });
        }

        private void SongList_Loaded(object sender, RoutedEventArgs e)
        {
            var scrollviewer = SongList.GetScrollViewer();
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
            horiMovingAni = EF.Lerp(0, (float)((80f / ImageGrid.ActualHeight * ImageGrid.ActualWidth) - ImageGrid.ActualWidth), horiMovingAni);

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

            var imageScaleAnimation = EF.Lerp(1, (float)(80f / ImageGrid.ActualHeight), scaleAnimation);

            var imageVisual = ElementCompositionPreview.GetElementVisual(ImageGrid);

            imageVisual.CenterPoint = new Vector3(32f, 0, 0);
            imageVisual.StartAnimation("Scale.X", imageScaleAnimation);
            imageVisual.StartAnimation("Scale.Y", imageScaleAnimation);

            imageVisual.StartAnimation("Offset.Y", imgMovingAnimation);
        }
    }
}
