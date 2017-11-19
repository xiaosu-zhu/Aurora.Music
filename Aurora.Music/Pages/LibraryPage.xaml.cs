using Aurora.Music.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Aurora.Music.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class LibraryPage : Page
    {
        public static LibraryPage Current;

        public LibraryPage()
        {
            this.InitializeComponent();
            Current = this;
            SubPanelFrame.Navigate(typeof(CategoryListView));
            MainPageViewModel.Current.Title = "Library";
            MainPageViewModel.Current.NeedShowTitle = true;
            MainPageViewModel.Current.IsLeftTopForeWhite = true;
        }

        internal void LefPanelNavigate(Type t)
        {
            SubPanelFrame.Navigate(t);
        }

        internal void LefPanelNavigate(Type t, object parameter)
        {
            SubPanelFrame.Navigate(t, parameter);
        }

        internal void Navigate(Type type, object parameter)
        {
            MainFrame.Navigate(type, parameter);
        }

        internal void Navigate(Type type)
        {
            MainFrame.Navigate(type);
        }

        internal void GoBack()
        {
            if (MainPage.Current.CanGoBack)
            {
                MainPage.Current.GoBack();
            }
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            SubPanelFrame.Content = null;
            MainFrame.Content = null;
        }
    }
}
