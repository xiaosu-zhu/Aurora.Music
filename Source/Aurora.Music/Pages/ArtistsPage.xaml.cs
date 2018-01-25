using Aurora.Music.Core;
using Aurora.Music.ViewModels;
using Aurora.Shared.Extensions;
using ExpressionBuilder;
using System.Numerics;
using Windows.System.Threading;
using System;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using EF = ExpressionBuilder.ExpressionFunctions;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using System.Linq;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Aurora.Music.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class ArtistsPage : Page
    {
        private CompositionPropertySet _scrollerPropertySet;
        private Compositor _compositor;
        private CompositionPropertySet _props;
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
            ArtistList.PrepareConnectedAnimation(Consts.ArtistPageInAnimation, e.ClickedItem, "ArtistName");

            LibraryPage.Current.Navigate(typeof(ArtistPage), (e.ClickedItem as ArtistViewModel));
            _clickedArtist = e.ClickedItem as ArtistViewModel;
        }

        private void ArtistList_Loaded(object sender, RoutedEventArgs e)
        {
            var scrollviewer = ArtistList.GetScrollViewer();
            _scrollerPropertySet = ElementCompositionPreview.GetScrollViewerManipulationPropertySet(scrollviewer);
            _compositor = _scrollerPropertySet.Compositor;

            _props = _compositor.CreatePropertySet();
            _props.InsertScalar("progress", 0);
            _props.InsertScalar("clampSize", (float)Title.ActualHeight + 64);
            _props.InsertScalar("scaleFactor", 0.5f);

            // Get references to our property sets for use with ExpressionNodes
            var scrollingProperties = _scrollerPropertySet.GetSpecializedReference<ManipulationPropertySetReferenceNode>();
            var props = _props.GetReference();
            var progressNode = props.GetScalarProperty("progress");
            var clampSizeNode = props.GetScalarProperty("clampSize");
            var scaleFactorNode = props.GetScalarProperty("scaleFactor");

            // Create and start an ExpressionAnimation to track scroll progress over the desired distance
            ExpressionNode progressAnimation = EF.Clamp(-scrollingProperties.Translation.Y / ((float)Header.Height - clampSizeNode), 0, 1);
            _props.StartAnimation("progress", progressAnimation);

            // Get the backing visual for the header so that its properties can be animated
            Visual headerVisual = ElementCompositionPreview.GetElementVisual(Header);

            // Create and start an ExpressionAnimation to clamp the header's offset to keep it onscreen
            ExpressionNode headerTranslationAnimation = EF.Conditional(progressNode < 1, scrollingProperties.Translation.Y, -(float)Header.Height + (float)Title.ActualHeight + 64);
            headerVisual.StartAnimation("Offset.Y", headerTranslationAnimation);

            //// Create and start an ExpressionAnimation to scale the header during overpan
            //ExpressionNode headerScaleAnimation = EF.Lerp(1, 1.25f, EF.Clamp(scrollingProperties.Translation.Y / 50, 0, 1));
            //headerVisual.StartAnimation("Scale.X", headerScaleAnimation);
            //headerVisual.StartAnimation("Scale.Y", headerScaleAnimation);

            ////Set the header's CenterPoint to ensure the overpan scale looks as desired
            //headerVisual.CenterPoint = new Vector3((float)(Header.ActualWidth / 2), (float)Header.ActualHeight, 0);

            var titleVisual = ElementCompositionPreview.GetElementVisual(Title);
            var titleshrinkVisual = ElementCompositionPreview.GetElementVisual(TitleShrink);
            var fixAnimation = EF.Conditional(progressNode < 1, -scrollingProperties.Translation.Y, (float)Header.Height - ((float)Title.ActualHeight + 64));
            titleVisual.StartAnimation("Offset.Y", fixAnimation);
            titleshrinkVisual.StartAnimation("Offset.Y", fixAnimation);
            var detailsVisual = ElementCompositionPreview.GetElementVisual(Details);
            var opacityAnimation = EF.Clamp(1 - (progressNode * 8), 0, 1);
            detailsVisual.StartAnimation("Opacity", opacityAnimation);

            var headerbgVisual = ElementCompositionPreview.GetElementVisual(HeaderBG);
            var headerbgOverlayVisual = ElementCompositionPreview.GetElementVisual(HeaderBGOverlay);
            var bgBlurVisual = ElementCompositionPreview.GetElementVisual(BGBlur);
            var bgOpacityAnimation = EF.Clamp(1 - progressNode, 0, 1);
            var bgblurOpacityAnimation = EF.Clamp(progressNode, 0, 1);
            titleshrinkVisual.StartAnimation("Opacity", bgblurOpacityAnimation);
            titleVisual.StartAnimation("Opacity", bgOpacityAnimation);
            headerbgVisual.StartAnimation("Opacity", bgOpacityAnimation);
            headerbgOverlayVisual.StartAnimation("Opacity", bgOpacityAnimation);
            bgBlurVisual.StartAnimation("Opacity", bgblurOpacityAnimation);
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
        }

        private void SemanticZoom_ViewChangeCompleted(object sender, SemanticZoomViewChangedEventArgs e)
        {
            var zoom = sender as SemanticZoom;
            if (zoom.IsZoomedInViewActive)
            {
                var scroller = ArtistList.GetScrollViewer();
                scroller.ChangeView(null, scroller.VerticalOffset - 120, null);
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var p = (string)((sender as ComboBox).SelectedItem as ComboBoxItem).Tag;
            Context.ChangeSort(p);
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
    }
}
