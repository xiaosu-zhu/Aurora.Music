using Aurora.Music.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System.Threading;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace Aurora.Music.Pages
{
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            this.InitializeComponent();
            LoactionFrame.Navigate(typeof(AddFoldersView));
            MainPageViewModel.Current.Title = "Settings";
            MainPageViewModel.Current.NeedShowTitle = true;
            MainPageViewModel.Current.LeftTopColor = Resources["SystemControlForegroundBaseHighBrush"] as SolidColorBrush;
            var t = ThreadPool.RunAsync(async x =>
            {
                await Context.Init();
            });
        }

        private void Ellipse_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {

        }

        private void Ellipse_PointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {

        }

        private void Ellipse_PointerReleased(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            var el = sender as Ellipse;
            Context.ToggleEffectState((string)el.Tag);
        }

        private void Ellipse_PointerCanceled(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {

        }

        double GetPosition(bool b)
        {
            return b ? 0d : -48d;
        }

        AcrylicBrush GetBrush(bool b)
        {
            return (AcrylicBrush)(b ? Resources["SystemControlAccentAcrylicWindowAccentMediumHighBrush"] : Resources["SystemControlAltLowAcrylicWindowBrush"]);
        }

        SolidColorBrush GetForeground(bool b)
        {
            return (SolidColorBrush)(b ? Resources["SystemControlForegroundBaseHighBrush"] : Resources["SystemControlForegroundChromeGrayBrush"]);
        }
    }
}
