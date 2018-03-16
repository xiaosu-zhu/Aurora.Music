// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Aurora.Music.Core.Models;
using Aurora.Shared.Helpers;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Aurora.Music.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    [UriActivate("welcome", Usage = ActivateUsage.Navigation)]
    public sealed partial class WelcomePage : Page
    {
        private bool searchBegined;
        private object parameter;

        public WelcomePage()
        {
            this.InitializeComponent();
            AddFolderFrame.Navigate(typeof(AddFoldersView), new object());
            Ani2.Stop();
            Ani1.Begin();
            Main.SelectionChanged += Main_SelectionChanged;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            parameter = e.Parameter;
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
            if (Main.SelectedIndex == 0)
            {
                Ani2.Stop();
                Ani1.Begin();
                return;
            }
            if (Main.SelectedIndex == 1)
            {
                Ani2.Begin();
                Ani1.Stop();
                return;
            }

            Ani1.Stop();
            Ani2.Stop();

            if (Main.SelectedIndex == Main.Items.Count - 1 && !searchBegined)
            {
                Settings.Current.WelcomeFinished = true;
                Settings.Current.Save();
                await StartSearching();
            }
        }

        private void Finish_Click(object sender, RoutedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            rootFrame.Navigate(typeof(MainPage), parameter);
        }

        private void Skip_Click(object sender, RoutedEventArgs e)
        {
            Settings.Current.WelcomeFinished = true;
            Settings.Current.Save();
            Frame rootFrame = Window.Current.Content as Frame;

            rootFrame.Navigate(typeof(MainPage), parameter);
        }
    }
}
