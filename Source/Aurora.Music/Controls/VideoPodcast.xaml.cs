using System;

using Aurora.Music.Core.Models;

using Windows.Media.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Aurora.Music.Controls
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class VideoPodcast : Page
    {
        public VideoPodcast()
        {
            InitializeComponent();
            RequestedTheme = Settings.Current.Theme;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is string s && Uri.TryCreate(s, UriKind.Absolute, out var u))
            {
                SetSource(u);
            }
            ApplicationView.GetForCurrentView().Consolidated += ViewConsolidated;
        }

        private void ViewConsolidated(ApplicationView sender, ApplicationViewConsolidatedEventArgs args)
        {
            ApplicationView.GetForCurrentView().Consolidated -= ViewConsolidated;
            mediaPlayer.MediaPlayer.Dispose();
            mediaPlayer = null;
            Window.Current.Close();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }

        public void SetSource(Uri source)
        {
            mediaPlayer.Source = MediaSource.CreateFromUri(source);
        }
    }
}
