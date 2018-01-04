using Aurora.Music.Core;
using Aurora.Music.ViewModels;
using Aurora.Shared.Extensions;
using ExpressionBuilder;
using System.Numerics;
using System;
using System.Threading.Tasks;
using Windows.System.Threading;
using Windows.UI.Composition;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using EF = ExpressionBuilder.ExpressionFunctions;
using Windows.System;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Aurora.Music.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class ArtistPage : Page
    {
        private CompositionPropertySet _scrollerPropertySet;
        private Compositor _compositor;
        private CompositionPropertySet _props;
        private AlbumViewModel _clickedAlbum;
        private string _lastParameter;

        public ArtistPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        private void ArtistPage_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if (e.Handled)
            {
                return;
            }
            e.Handled = true;
            ConnectedAnimationService.GetForCurrentView().PrepareToAnimate(Consts.ArtistPageInAnimation + "_1", Title);
            ConnectedAnimationService.GetForCurrentView().PrepareToAnimate(Consts.ArtistPageInAnimation + "_2", AvatarImage);
            LibraryPage.Current.GoBack();
            UnloadObject(this);
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
            AppViewBackButtonVisibility.Visible;
            SystemNavigationManager.GetForCurrentView().BackRequested -= ArtistPage_BackRequested;
            SystemNavigationManager.GetForCurrentView().BackRequested += ArtistPage_BackRequested;

            if (!Context.AlbumList.IsNullorEmpty() && _clickedAlbum != null && ((ArtistViewModel)e.Parameter).RawName == _lastParameter)
            {
                AlbumList.ScrollIntoView(_clickedAlbum);
                var ani = ConnectedAnimationService.GetForCurrentView().GetAnimation(Consts.AlbumDetailPageInAnimation + "_1");
                if (ani != null)
                {
                    await AlbumList.TryStartConnectedAnimationAsync(ani, _clickedAlbum, "AlbumName");
                }
                ani = ConnectedAnimationService.GetForCurrentView().GetAnimation(Consts.AlbumDetailPageInAnimation + "_2");
                if (ani != null)
                {
                    await AlbumList.TryStartConnectedAnimationAsync(ani, _clickedAlbum, "Shadow");
                }
                return;
            }
            else if (_clickedAlbum != null && ((ArtistViewModel)e.Parameter).RawName == _lastParameter)
            {
                Context.Artist = (ArtistViewModel)e.Parameter;
                await Context.GetAlbums((ArtistViewModel)e.Parameter);
                var ani = ConnectedAnimationService.GetForCurrentView().GetAnimation(Consts.AlbumDetailPageInAnimation + "_1");
                if (ani != null)
                {
                    await AlbumList.TryStartConnectedAnimationAsync(ani, _clickedAlbum, "AlbumName");
                }
                ani = ConnectedAnimationService.GetForCurrentView().GetAnimation(Consts.AlbumDetailPageInAnimation + "_2");
                if (ani != null)
                {
                    await AlbumList.TryStartConnectedAnimationAsync(ani, _clickedAlbum, "Shadow");
                }
                return;
            }
            else if (e.Parameter is ArtistViewModel s)
            {
                _lastParameter = s.RawName;
                Context.Artist = s;

                await Context.GetAlbums(s);
            }
            else
            {
                LibraryPage.Current.GoBack();
            }
        }

        private void StackPanel_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Panel s)
            {
                (s.Resources["PointerOver"] as Storyboard).Begin();
            }
        }

        private void StackPanel_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Panel s)
            {
                (s.Resources["Normal"] as Storyboard).Begin();
            }
        }

        private void AlbumList_ItemClick(object sender, ItemClickEventArgs e)
        {
            AlbumList.PrepareConnectedAnimation(Consts.AlbumDetailPageInAnimation + "_1", e.ClickedItem, "AlbumName");
            AlbumList.PrepareConnectedAnimation(Consts.AlbumDetailPageInAnimation + "_2", e.ClickedItem, "Shadow");
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

        private void StackPanel_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Panel s)
            {
                (s.Resources["PointerOver"] as Storyboard).Begin();
            }
        }

        private void AlbumList_Loaded(object sender, RoutedEventArgs e)
        {
            var ani = ConnectedAnimationService.GetForCurrentView().GetAnimation(Consts.ArtistPageInAnimation);
            if (ani != null)
            {
                ani.TryStart(AvatarImage, new UIElement[] { Title, HeaderBG, Details });
            }

            var scrollviewer = AlbumList.GetScrollViewer();
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
            SystemNavigationManager.GetForCurrentView().BackRequested -= ArtistPage_BackRequested;
        }

        private async void PlayAlbum_Click(object sender, RoutedEventArgs e)
        {
            await Context.PlayAlbumAsync((sender as Button).DataContext as AlbumViewModel);
        }

        private void Button_Holding(object sender, HoldingRoutedEventArgs e)
        {
            if (e.HoldingState == Windows.UI.Input.HoldingState.Completed)
            {
                PlayAlbum_Click(sender, null);
                e.Handled = true;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (DescriIndicator.Glyph == "\uE018")
            {
                DescriIndicator.Glyph = "\uE09D";
                Descriptions.Height = 75;
            }
            else
            {
                DescriIndicator.Glyph = "\uE018";
                Descriptions.Height = double.NaN;
            }
        }

        private async void Descriptions_LinkClicked(object sender, Microsoft.Toolkit.Uwp.UI.Controls.LinkClickedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri(e.Link));
        }
    }
}
