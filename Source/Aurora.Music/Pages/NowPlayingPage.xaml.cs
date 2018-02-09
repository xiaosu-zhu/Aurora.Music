// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Aurora.Music.Controls;
using Aurora.Music.Core;
using Aurora.Music.ViewModels;
using Aurora.Shared;
using Aurora.Shared.Extensions;
using Aurora.Shared.Helpers;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Aurora.Music.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class NowPlayingPage : Page, IRequestGoBack
    {
        internal static NowPlayingPage Current;
        private float canvasWidth;
        private float canvasHeight;

        public bool NewSpec { get; private set; }

        public NowPlayingPage()
        {
            this.InitializeComponent();
            Current = this;
            Context.SongChanged += Context_SongChanged;
        }

        private void Grid_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (sender is Panel s)
            {
                (s.Resources["PointerOver"] as Storyboard).Begin();
            }
        }

        private void Grid_PointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (sender is Panel s)
            {
                (s.Resources["Normal"] as Storyboard).Begin();
            }
        }

        private void PlayBtn_Click(object sender, RoutedEventArgs e)
        {
            MainPageViewModel.Current.SkiptoItem((sender as Button).DataContext as SongViewModel);
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
                            var menuItem = new MenuFlyoutItem()
                            {
                                Text = $"{s.Song.Performers[0]}",
                                Icon = new FontIcon()
                                {
                                    Glyph = "\uE136"
                                }
                            };
                            menuItem.Click += OpenArtistViewDialog;
                            MoreMenu.Items.Insert(1, menuItem);
                        }
                        else
                        {
                            var sub = new MenuFlyoutSubItem()
                            {
                                Text = $"{Consts.Localizer.GetString("PerformersText")}",
                                Icon = new FontIcon()
                                {
                                    Glyph = "\uE136"
                                }
                            };
                            foreach (var item in s.Song.Performers)
                            {
                                var menuItem = new MenuFlyoutItem()
                                {
                                    Text = item
                                };
                                menuItem.Click += OpenArtistViewDialog;
                                sub.Items.Add(menuItem);
                            }
                            MoreMenu.Items.Insert(1, sub);
                        }
                    }
                }
            });
        }

        private async void OpenArtistViewDialog(object sender, RoutedEventArgs e)
        {
            var artist = (sender as MenuFlyoutItem).Text;
            var dialog = new ArtistViewDialog(new ArtistViewModel()
            {
                Name = artist,
            });
            await dialog.ShowAsync();
        }

        public void RequestGoBack()
        {
            ConnectedAnimationService.GetForCurrentView().PrepareToAnimate(Consts.NowPlayingPageInAnimation, Artwork);
            ConnectedAnimationService.GetForCurrentView().PrepareToAnimate($"{Consts.NowPlayingPageInAnimation}_1", Title);
            ConnectedAnimationService.GetForCurrentView().PrepareToAnimate($"{Consts.NowPlayingPageInAnimation}_2", Album);
            Root.Background = new SolidColorBrush(Colors.Transparent);
            MainPage.Current.GoBackFromNowPlaying();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            MainPageViewModel.Current.Title = Consts.Localizer.GetString("NowPlayingText");
            MainPageViewModel.Current.NeedShowTitle = true;
            //MainPageViewModel.Current.LeftTopColor = Resources["SystemControlForegroundBaseHighBrush"] as SolidColorBrush;
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
            AppViewBackButtonVisibility.Visible;


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
            MainPageViewModel.Current.RestoreLastTitle();
            SizeChanged -= NowPlayingPage_SizeChanged;
            Context.SongChanged -= Context_SongChanged;
        }

        private async void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Context.Lyric.Contents.Count > 0 && (sender as ListView).SelectedIndex >= 0)
                try
                {
                    await (sender as ListView).ScrollToIndex((sender as ListView).SelectedIndex, ScrollPosition.Center);
                }
                catch (Exception)
                {
                }
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            MainPageViewModel.Current.RestoreLastTitle();
            Context?.Dispose();
            Unload();
        }

        private void Slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            Context.PositionChange(Context.TotalDuration * (e.NewValue / 100d));
        }

        private async void OpenAlbumViewDialog(object sender, RoutedEventArgs e)
        {
            var dialog = new AlbumViewDialog(await Context.GetAlbumAsync());
            await dialog.ShowAsync();
        }

        internal void Unload()
        {
            Context?.Unload();
            Drawer.SizeChanged -= Drawer_SizeChanged;
            Drawer = null;
            Context = null;
        }

        private async void FindFileClick(object sender, RoutedEventArgs e)
        {
            await Context.FindFileAsync();
        }

        private async void DownloadOrModify(object sender, RoutedEventArgs e)
        {
            await Context.DowmloadOrModifyAsync();
        }

        private void Share_Click(object sender, RoutedEventArgs e)
        {
            Context.ShareCurrentAsync();
        }

        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            await Context.DeleteCurrentAsync(Windows.Storage.StorageDeleteOption.Default);
        }

        private async void Delete_Click_1(object sender, RoutedEventArgs e)
        {
            await Context.DeleteCurrentAsync(Windows.Storage.StorageDeleteOption.Default);
        }

        private async void RatingControl_ValueChanged(RatingControl sender, object args)
        {
            await Context.WriteRatingValue(sender.Value);
        }

        private void Cast_Click(object sender, RoutedEventArgs e)
        {
            //Retrieve the location of the casting button
            GeneralTransform transform = (sender as MenuFlyoutItem).TransformToVisual(Window.Current.Content as UIElement);
            Point pt = transform.TransformPoint(new Point(0, 0));

            Context.ShowCastingUI(new Rect(pt.X, pt.Y, (sender as MenuFlyoutItem).ActualWidth, (sender as MenuFlyoutItem).ActualHeight));

        }

        internal bool IsDarkTheme()
        {
            return Palette.IsDarkColor((Resources["SystemControlBackgroundAltHighBrush"] as SolidColorBrush).Color);
        }

        private void VisualStateGroup_CurrentStateChanged(object sender, VisualStateChangedEventArgs e)
        {
            ListView_SelectionChanged(LrcView, null);
            if (e.NewState.Name != "Full")
            {

            }
        }

        private void LrcHeader_Loaded(object sender, RoutedEventArgs e)
        {
            SizeChanged += NowPlayingPage_SizeChanged;
            var h = (Artwork.ActualHeight + LrcHeader.ActualHeight) / 2;
            if (h < 24)
            {
                h = 24;
            }
            LrcHeaderGrid.Height = h;
        }

        private void NowPlayingPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var h = (Artwork.ActualHeight + LrcHeader.ActualHeight) / 2;
            if (h < 24)
            {
                h = 24;
            }
            LrcHeaderGrid.Height = h;
        }


        private async void Flyout_Opened(object sender, object e)
        {
            await NowPlayingFlyout.ScrollToIndex(NowPlayingFlyout.SelectedIndex, ScrollPosition.Center);
        }

        private void Canvas_Draw(ICanvasAnimatedControl sender, CanvasAnimatedDrawEventArgs args)
        {
            float width = canvasWidth;
            width /= 512f;
            for (int i = 0; i < 511; i++)
            {
                args.DrawingSession.DrawLine(i * width, Context.Spectrum[i] * canvasHeight, (i + 1) * width, Context.Spectrum[i + 1] * canvasHeight, Colors.Red);
            }
        }

        private void Canvas_CreateResources(CanvasAnimatedControl sender, Microsoft.Graphics.Canvas.UI.CanvasCreateResourcesEventArgs args)
        {
        }

        private void Canvas_Loaded(object sender, RoutedEventArgs e)
        {
            canvasWidth = (float)Drawer.ActualWidth;
            canvasHeight = (float)Drawer.ActualHeight;
            Drawer.SizeChanged += Drawer_SizeChanged;
        }

        private void Drawer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            canvasWidth = (float)Drawer.ActualWidth;
            canvasHeight = (float)Drawer.ActualHeight;
        }
    }
}
