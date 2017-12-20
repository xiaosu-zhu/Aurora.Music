using Aurora.Music.Controls;
using Aurora.Music.Core;
using Aurora.Music.Core.Models;
using Aurora.Music.ViewModels;
using Aurora.Shared.Extensions;
using Aurora.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System.Threading;
using Windows.UI;
using Windows.UI.Core;
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
    public sealed partial class NowPlayingPage : Page
    {
        internal static NowPlayingPage Current;

        public NowPlayingPage()
        {
            this.InitializeComponent();
            Current = this;
            Context.SongChanged += Context_SongChanged;
        }

        private async void Context_SongChanged(object sender, EventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            {
                lock (this)
                {
                    var s = sender as SongViewModel;
                    if (MoreMenu.Items[1] is MenuFlyoutSeparator)
                    {

                    }
                    else
                    {
                        MoreMenu.Items.RemoveAt(1);
                    }
                    if (!s.Song.Performers.IsNullorEmpty())
                    {

                        if (s.Song.Performers.Length == 1)
                        {
                            MoreMenu.Items.Insert(1, new MenuFlyoutItem()
                            {
                                Text = $"{s.Song.Performers[0]}",
                                Icon = new FontIcon()
                                {
                                    Glyph = "\uE136"
                                }
                            });
                        }
                        else
                        {
                            var sub = new MenuFlyoutSubItem()
                            {
                                Text = $"Performers:",
                                Icon = new FontIcon()
                                {
                                    Glyph = "\uE136"
                                }
                            };
                            foreach (var item in s.Song.Performers)
                            {
                                sub.Items.Add(new MenuFlyoutItem()
                                {
                                    Text = item
                                });
                            }
                            MoreMenu.Items.Insert(1, sub);
                        }
                    }
                }
            });
        }

        private void NowPlayingPage_BackRequested(object sender, BackRequestedEventArgs e)
        {
            ConnectedAnimationService.GetForCurrentView().PrepareToAnimate(Consts.NowPlayingPageInAnimation, Artwork);
            ConnectedAnimationService.GetForCurrentView().PrepareToAnimate($"{Consts.NowPlayingPageInAnimation}_1", Title);
            ConnectedAnimationService.GetForCurrentView().PrepareToAnimate($"{Consts.NowPlayingPageInAnimation}_2", Album);
            Root.Background = new SolidColorBrush(Colors.Transparent);
            MainPage.Current.GoBackFromNowPlaying();
            e.Handled = true;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            MainPageViewModel.Current.Title = "Now Playing";
            MainPageViewModel.Current.NeedShowTitle = true;
            MainPageViewModel.Current.LeftTopColor = Resources["SystemControlForegroundBaseHighBrush"] as SolidColorBrush;
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
            AppViewBackButtonVisibility.Visible;
            SystemNavigationManager.GetForCurrentView().BackRequested += NowPlayingPage_BackRequested;


            if (e.Parameter is SongViewModel s)
            {
                Context.Init(s);
            }
            else
            {
                throw new Exception();
            }
            var ani = ConnectedAnimationService.GetForCurrentView().GetAnimation(Consts.NowPlayingPageInAnimation);
            if (ani != null)
            {
                ani.TryStart(Artwork, new UIElement[] { Root });
            }
            ani = ConnectedAnimationService.GetForCurrentView().GetAnimation($"{Consts.NowPlayingPageInAnimation}_1");
            if (ani != null)
            {
                ani.TryStart(Title);
            }
            ani = ConnectedAnimationService.GetForCurrentView().GetAnimation($"{Consts.NowPlayingPageInAnimation}_2");
            if (ani != null)
            {
                ani.TryStart(Album);
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            SystemNavigationManager.GetForCurrentView().BackRequested -= NowPlayingPage_BackRequested;
            MainPageViewModel.Current.RestoreLastTitle();
        }

        private async void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Context.Lyric.Contents.Count > 0 && (sender as ListView).SelectedIndex >= 0)
                try
                {
                    await (sender as ListView).ScrollToIndex((sender as ListView).SelectedIndex, (sender as ListView).ActualHeight / 2 - 48);
                }
                catch (Exception)
                {
                }
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().BackRequested -= NowPlayingPage_BackRequested;
            MainPageViewModel.Current.RestoreLastTitle();
            Context?.Dispose();
            Unload();
        }

        private void Slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            Context.PositionChange(Context.TotalDuration * (e.NewValue / 100d));
        }

        private async void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AlbumViewDialog(await Context.GetAlbumAsync());
            await dialog.ShowAsync();
        }

        internal void Unload()
        {
            Context?.Unload();
            Context = null;
        }

        private async void FindFileClick(object sender, RoutedEventArgs e)
        {
            await Context.FindFileAsync();
        }

        private async void DowmloadOrModify(object sender, RoutedEventArgs e)
        {
            await Context.DowmloadOrModifyAsync();
        }

        private void Share_Click(object sender, RoutedEventArgs e)
        {
            Context.ShareCurrentAsync();
        }
    }
}
