using Aurora.Music.Core;
using Aurora.Music.ViewModels;
using Aurora.Shared.Extensions;
using ExpressionBuilder;
using System.Numerics;
using Windows.System.Threading;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using EF = ExpressionBuilder.ExpressionFunctions;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Media.Animation;

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

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (!Context.ArtistList.IsNullorEmpty() && _clickedArtist != null)
            {
                ArtistList.ScrollIntoView(_clickedArtist);
                var ani = ConnectedAnimationService.GetForCurrentView().GetAnimation(Consts.ArtistPageInAnimation + "_1");
                if (ani != null)
                {
                    ArtistList.TryStartConnectedAnimationAsync(ani, _clickedArtist, "ArtistName");
                }
                ani = ConnectedAnimationService.GetForCurrentView().GetAnimation(Consts.ArtistPageInAnimation + "_2");
                if (ani != null)
                {
                    ArtistList.TryStartConnectedAnimationAsync(ani, _clickedArtist, "ArtistImage");
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

            LibraryPage.Current.Navigate(typeof(ArtistPage), (e.ClickedItem as ArtistViewModel).RawName);
            _clickedArtist = e.ClickedItem as ArtistViewModel;
        }

        private void ArtistList_Loaded(object sender, RoutedEventArgs e)
        {
            var scrollviewer = ArtistList.GetScrollViewer();
            _scrollerPropertySet = ElementCompositionPreview.GetScrollViewerManipulationPropertySet(scrollviewer);
            _compositor = _scrollerPropertySet.Compositor;

            _props = _compositor.CreatePropertySet();
            _props.InsertScalar("progress", 0);
            _props.InsertScalar("clampSize", (float)Title.ActualHeight + 40);
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
            ExpressionNode headerTranslationAnimation = EF.Conditional(progressNode < 1, scrollingProperties.Translation.Y, -(float)Header.Height + (float)Title.ActualHeight + 40);
            headerVisual.StartAnimation("Offset.Y", headerTranslationAnimation);

            //// Create and start an ExpressionAnimation to scale the header during overpan
            //ExpressionNode headerScaleAnimation = EF.Lerp(1, 1.25f, EF.Clamp(scrollingProperties.Translation.Y / 50, 0, 1));
            //headerVisual.StartAnimation("Scale.X", headerScaleAnimation);
            //headerVisual.StartAnimation("Scale.Y", headerScaleAnimation);

            ////Set the header's CenterPoint to ensure the overpan scale looks as desired
            //headerVisual.CenterPoint = new Vector3((float)(Header.ActualWidth / 2), (float)Header.ActualHeight, 0);

            var titleVisual = ElementCompositionPreview.GetElementVisual(Title);
            var titleshrinkVisual = ElementCompositionPreview.GetElementVisual(TitleShrink);
            var fixAnimation = EF.Conditional(progressNode < 1, -scrollingProperties.Translation.Y, (float)Header.Height - ((float)Title.ActualHeight + 40));
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
    }
}
