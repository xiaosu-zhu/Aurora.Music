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
using Windows.UI.Xaml.Media;
using System.Drawing;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Aurora.Music.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class HomePage : Page
    {
        private CompositionPropertySet _scrollerPropertySet;
        private Compositor _compositor;
        private CompositionPropertySet _props;

        public HomePage()
        {
            this.InitializeComponent();
            var t = ThreadPool.RunAsync(async x =>
            {
                await Context.Load();
            });
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            MainPageViewModel.Current.NeedShowTitle = false;
            MainPageViewModel.Current.LeftTopColor = Resources["SystemControlForegroundBaseHighBrush"] as SolidColorBrush;
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
            AppViewBackButtonVisibility.Collapsed;
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

        private async void Grid_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            ((sender as Grid).Resources["PointerOver"] as Storyboard).Begin();
            await MainPageViewModel.Current.InstantPlay(await ((sender as FrameworkElement).DataContext as GenericMusicItemViewModel).GetSongsAsync());
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ContentPanel.Width = this.ActualWidth;
        }

        private void Header_Loaded(object sender, RoutedEventArgs e)
        {
            var scrollviewer = MainScroller;
            _scrollerPropertySet = ElementCompositionPreview.GetScrollViewerManipulationPropertySet(scrollviewer);
            _compositor = _scrollerPropertySet.Compositor;

            _props = _compositor.CreatePropertySet();
            _props.InsertScalar("progress", 0);

            // Get references to our property sets for use with ExpressionNodes
            var scrollingProperties = _scrollerPropertySet.GetSpecializedReference<ManipulationPropertySetReferenceNode>();
            var props = _props.GetReference();
            var progressNode = props.GetScalarProperty("progress");

            // Create and start an ExpressionAnimation to track scroll progress over the desired distance
            ExpressionNode progressAnimation = EF.Clamp(-scrollingProperties.Translation.Y / ((float)Header.Height), 0, 1);
            _props.StartAnimation("progress", progressAnimation);

            var headerbgVisual = ElementCompositionPreview.GetElementVisual(HeaderBG);
            var bgblurOpacityAnimation = EF.Clamp(progressNode, 0, 1);
            headerbgVisual.StartAnimation("Opacity", bgblurOpacityAnimation);
        }

        private async void FavList_ItemClick(object sender, ItemClickEventArgs e)
        {
            await MainPageViewModel.Current.InstantPlay(await (e.ClickedItem as GenericMusicItemViewModel).GetSongsAsync());
        }

        private void HeroGrid_ContextRequested(UIElement sender, ContextRequestedEventArgs e)
        {
            // Walk up the tree to find the ListViewItem.
            // There may not be one if the click wasn't on an item.
            var requestedElement = (FrameworkElement)e.OriginalSource;
            while ((requestedElement != sender) && !(requestedElement is ListViewItem))
            {
                requestedElement = (FrameworkElement)VisualTreeHelper.GetParent(requestedElement);
            }
            if (requestedElement != sender)
            {
                if (e.TryGetPosition(requestedElement, out var point))
                {
                    (Resources["SongFlyout"] as MenuFlyout).ShowAt(requestedElement, point);
                }
                else
                {
                    (Resources["SongFlyout"] as MenuFlyout).ShowAt(requestedElement);
                }
                e.Handled = true;
            }
        }

        private void HeroGrid_ContextCanceled(UIElement sender, RoutedEventArgs args)
        {
            (Resources["SongFlyout"] as MenuFlyout).Hide();
        }
    }
}
