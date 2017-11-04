using System;
using Aurora.Music.Pages;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Aurora.Music.ViewModels;
using Aurora.Shared.Controls;
using Windows.UI.Input;
using Windows.UI.Xaml.Media.Animation;
using Aurora.Music.Core;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace Aurora.Music
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page, IChangeTheme
    {
        public static MainPage Current;

        public MainPage()
        {
            this.InitializeComponent();
            Current = this;
            MainFrame.Navigate(typeof(HomePage));
            GestureRecognizer g = new GestureRecognizer
            {
                GestureSettings = GestureSettings.HoldWithMouse
            };
        }

        private Type[] navigateOptions = { typeof(HomePage), typeof(LibraryPage) };

        private void Main_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            MainFrame.Navigate(navigateOptions[sender.MenuItems.IndexOf(args.SelectedItem)]);
        }

        Symbol NullableBoolToSymbol(bool? b)
        {
            if (b is bool bb)
            {
                return bb ? Symbol.Pause : Symbol.Play;
            }
            return Symbol.Play;
        }

        double PositionToValue(TimeSpan t1, TimeSpan total)
        {
            if (total == null || total == default(TimeSpan))
            {
                return 0;
            }
            return 100 * (t1.TotalMilliseconds / total.TotalMilliseconds);
        }

        string PositionToString(TimeSpan t1, TimeSpan total)
        {
            if (total == null || total == default(TimeSpan))
            {
                return "0:00/0:00";
            }
            return (t1.ToString(@"m\:ss") + '/' + total.ToString(@"m\:ss"));
        }

        public void Navigate(Type type)
        {
            MainFrame.Navigate(type);
        }

        public void Navigate(Type type, object parameter)
        {
            MainFrame.Navigate(type, parameter);
        }

        public Visibility BooltoVisibility(bool b)
        {
            return b ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Toggle_PaneOpened(object sender, RoutedEventArgs e)
        {
            Root.IsPaneOpen = !Root.IsPaneOpen;
        }

        private void Pane_CurrentChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (var item in Context.HamList)
            {
                item.IsCurrent = false;
            }
            ((sender as ListView).SelectedItem as HamPanelItem).IsCurrent = true;
            MainFrame.Navigate(((sender as ListView).SelectedItem as HamPanelItem).TargetType);
        }

        public void ChangeTheme()
        {
            if (MainFrame.Content is IChangeTheme iT)
            {
                iT.ChangeTheme();
            }
        }

        private void FastFoward_Holding(object sender, Windows.UI.Xaml.Input.HoldingRoutedEventArgs e)
        {
            if (e.HoldingState == HoldingState.Canceled)
            {
                Context.FastForward(false);
            }
            else
            {
                Context.FastForward(true);
            }
        }

        private void StackPanel_PointerReleased(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (Context.NowPlayingList.Count > 0 && Context.CurrentIndex >= 0)
            {
                OverlayFrame.Visibility = Visibility.Visible;
                MainFrame.Visibility = Visibility.Collapsed;
                ConnectedAnimationService.GetForCurrentView().PrepareToAnimate(Consts.NowPlayingPageInAnimation, Artwork);
                OverlayFrame.Navigate(typeof(NowPlayingPage), Context.NowPlayingList[Context.CurrentIndex]);
            }
            if (sender is Panel s)
            {
                (s.Resources["Normal"] as Storyboard).Begin();
                e.Handled = true;
            }
        }

        private void StackPanel_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (sender is Panel s)
            {
                (s.Resources["PointerOver"] as Storyboard).Begin();
                e.Handled = true;
            }
        }

        private void StackPanel_PointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (sender is Panel s)
            {
                (s.Resources["Normal"] as Storyboard).Begin();
                e.Handled = true;
            }
        }

        private void NavigatePanel_PointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (sender is Panel s)
            {
                (s.Resources["PointerPressed"] as Storyboard).Begin();
                e.Handled = true;
            }
        }
    }
}
