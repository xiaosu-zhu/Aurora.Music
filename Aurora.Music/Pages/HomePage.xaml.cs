using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System.Threading;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Aurora.Music.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class HomePage : Page
    {
        public HomePage()
        {
            this.InitializeComponent();
            var t = ThreadPool.RunAsync(async x =>
            {
                await Context.Load();
            });
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
            ContentPanel.Width = this.ActualWidth;
        }
    }
}
