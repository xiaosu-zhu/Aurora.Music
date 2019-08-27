// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using AudioVisualizer;
using Aurora.Music.Controls;
using Aurora.Music.Core;
using Aurora.Music.Core.Models;
using Aurora.Music.ViewModels;
using Aurora.Shared;
using Aurora.Shared.Extensions;
using Aurora.Shared.Helpers;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using System;
using System.Numerics;
using System.Threading.Tasks;
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
    [UriActivate("now", Usage = ActivateUsage.Query)]
    public sealed partial class NowPlayingPage : Page, IRequestGoBack
    {
        internal static NowPlayingPage Current;
        private float canvasWidth;
        private float canvasHeight;

        public bool NewSpec { get; private set; }

        public NowPlayingPage()
        {
            InitializeComponent();
            Current = this;
            Context.SongChanged += Context_SongChanged;
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
                    if (!s.Song.IsPodcast)
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

        private async void ClickArtistViewDialog(object sender, RoutedEventArgs e)
        {
            var artist = Context.Song.GetFormattedArtists();
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
            MainPageViewModel.Current.NeedShowTitle = false;

            MainPageViewModel.Current.NeedShowBack = true;


            if (e.Parameter is SongViewModel s)
            {
                Context.Init(s);
            }
            else
            {
                throw new Exception();
            }
            return;
            Task.Run(async () =>
            {
                await Task.Delay(160);
                await Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
                {
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
                });
            });
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            Visualizer.SizeChanged -= Visualizer_SizeChanged;
            MainPageViewModel.Current.IsVisualizing = false;
            Visualizer.Draw -= CustomVisualizer_Draw;
            SizeChanged -= NowPlayingPage_SizeChanged;
            Context.SongChanged -= Context_SongChanged;
            Context?.Dispose();
            MainPageViewModel.Current.IsVisualizing = false;
            Unload();
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
            Context?.Dispose();
            MainPageViewModel.Current.IsVisualizing = false;
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
            await Context.DeleteCurrentAsync(Windows.Storage.StorageDeleteOption.PermanentDelete);
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
            switch (Settings.Current.Theme)
            {
                case ElementTheme.Default:
                    return Palette.IsDarkColor((Resources["SystemControlBackgroundAltHighBrush"] as SolidColorBrush).Color);
                case ElementTheme.Light:
                    return false;
                case ElementTheme.Dark:
                    return true;
                default:
                    return Palette.IsDarkColor((Resources["SystemControlBackgroundAltHighBrush"] as SolidColorBrush).Color);
            }
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
            if (NowPlayingFlyout.SelectedIndex != -1)
                await NowPlayingFlyout.ScrollToIndex(NowPlayingFlyout.SelectedIndex, ScrollPosition.Center);
        }


        SpectrumData _emptySpectrum = SpectrumData.CreateEmpty(2, Consts.SpectrumBarCount, ScaleType.Linear, ScaleType.Linear, 0, 20000);
        SpectrumData _previousSpectrum;
        SpectrumData _previousPeakSpectrum;

        const double fps = 1000d / 60d;

        static readonly TimeSpan _rmsRiseTime = TimeSpan.FromMilliseconds(12 * fps);
        static readonly TimeSpan _rmsFallTime = TimeSpan.FromMilliseconds(12 * fps);
        static readonly TimeSpan _peakRiseTime = TimeSpan.FromMilliseconds(12 * fps);
        static readonly TimeSpan _peakFallTime = TimeSpan.FromMilliseconds(120 * fps);
        static readonly TimeSpan _frameDuration = TimeSpan.FromMilliseconds(fps);

        private void CustomVisualizer_Loaded(object sender, RoutedEventArgs e)
        {
            canvasHeight = (float)Visualizer.ActualHeight;
            canvasWidth = (float)Visualizer.ActualWidth;
            Context.Visualizer = Visualizer;
            Visualizer.SizeChanged += Visualizer_SizeChanged;
        }

        private void Visualizer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            canvasHeight = (float)Visualizer.ActualHeight;
            canvasWidth = (float)Visualizer.ActualWidth;
        }

        private void CustomVisualizer_CreateResources(object sender, CreateResourcesEventArgs args)
        {

        }

        private void CustomVisualizer_Draw(IVisualizer sender, VisualizerDrawEventArgs args)
        {
            if (!MainPageViewModel.Current.IsVisualizing)
                return;
            var drawingSession = (CanvasDrawingSession)args.DrawingSession;

            float barWidth = canvasWidth / (2 * Consts.SpectrumBarCount);
            // Calculate spectum metrics
            var barSize = new Vector2(barWidth, canvasHeight - 2 * barWidth);

            // Get the data if data exists and source is in play state, else use empty
            var spectrumData = args.Data != null && Visualizer.Source?.PlaybackState == SourcePlaybackState.Playing ?
                                            args.Data.Spectrum.LogarithmicTransform(Consts.SpectrumBarCount, 20f, 20000f) : _emptySpectrum;

            _previousSpectrum = spectrumData.ApplyRiseAndFall(_previousSpectrum, _rmsRiseTime, _rmsFallTime, _frameDuration);
            _previousPeakSpectrum = spectrumData.ApplyRiseAndFall(_previousPeakSpectrum, _peakRiseTime, _peakFallTime, _frameDuration);
            
            var logSpectrum = _previousSpectrum.ConvertToDecibels(-50, 0);
            var logPeakSpectrum = _previousPeakSpectrum.ConvertToDecibels(-50, 0);

            var step = canvasWidth / Consts.SpectrumBarCount;
            var flaw = (step - barSize.X) / 2;

            using (var brush = new CanvasLinearGradientBrush(drawingSession, new CanvasGradientStop[] { new CanvasGradientStop() { Color = Context.CurrentColor[0], Position = 0f }, new CanvasGradientStop() { Color = Context.CurrentColor[1], Position = 1f } })
            {
                StartPoint = new Vector2(canvasWidth, 0),
                EndPoint = new Vector2(0, canvasHeight)
            })
            {
                // Draw spectrum bars
                for (int index = 0; index < Consts.SpectrumBarCount; index++)
                {
                    float barX = step * index + flaw;
                    // use average of 2 channel
                    float spectrumBarHeight = barSize.Y * (1.0f - (logSpectrum[0][index] + logSpectrum[1][index]) / -100.0f);
                    drawingSession.FillRoundedRectangle(barX, canvasHeight - barWidth - spectrumBarHeight, barSize.X, spectrumBarHeight, barSize.X / 2, barSize.X / 2, brush);
                }
            }

            // Spectrum points to draw a slow decay line
            for (int index = 0; index < Consts.SpectrumBarCount; index++)
            {
                float X = (index + 0.5f) * step;

                float spectrumBarHeight = barSize.Y * (1.0f - (logPeakSpectrum[0][index] + logPeakSpectrum[1][index]) / -100.0f);

                var decayPoint = new Vector2(X, canvasHeight - barWidth - spectrumBarHeight);
                drawingSession.FillCircle(decayPoint, barSize.X / 2, Context.CurrentColor[1]);
            }
        }

        private async void Artwork_ImageOpened(object sender, RoutedEventArgs e)
        {
            var service = ConnectedAnimationService.GetForCurrentView();
            var ani = service.GetAnimation(Consts.NowPlayingPageInAnimation);
            if (ani != null)
            {
                ani.TryStart(Artwork, new UIElement[] { Root });
            }
            ani = service.GetAnimation($"{Consts.NowPlayingPageInAnimation}_1");
            if (ani != null)
            {
                ani.TryStart(Title);
            }
            ani = service.GetAnimation($"{Consts.NowPlayingPageInAnimation}_2");
            if (ani != null)
            {
                ani.TryStart(Album);
            }
            await Task.Delay(200);
            ani = service.GetAnimation("DropAni");
            if (ani != null)
                ani.TryStart(SongPanel);
        }

        private void NowPlayingFlyout_ItemClick(object sender, ItemClickEventArgs e)
        {
            MainPageViewModel.Current.SkiptoItem(e.ClickedItem as SongViewModel);
        }
    }
}
