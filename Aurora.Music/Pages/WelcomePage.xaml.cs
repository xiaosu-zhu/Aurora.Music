using Aurora.Music.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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
    public sealed partial class WelcomePage : Page
    {
        private bool searchBegined;

        public WelcomePage()
        {
            this.InitializeComponent();
            AddFolderFrame.Navigate(typeof(AddFoldersView), new object());
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Main.SelectedIndex++;
        }


        public double IndexToProgress(int index)
        {
            return (100d / Main.Items.Count) * (index + 1);
        }

        private async Task StartSearching()
        {
            searchBegined = true;
            await Context.StartSearch();
        }

        private async void Main_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Main.SelectedIndex == Main.Items.Count - 1 && !searchBegined)
            {
                await StartSearching();
            }
        }

        private void Finish_Click(object sender, RoutedEventArgs e)
        {
            var s = Settings.Load();
            s.WelcomeFinished = true;
            s.Save();
            Frame rootFrame = Window.Current.Content as Frame;

            rootFrame.Navigate(typeof(MainPage));
        }
    }
}
