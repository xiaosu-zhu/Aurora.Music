// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using AudioVisualizer;
using Aurora.Music.Core;
using Aurora.Music.ViewModels;
using Aurora.Shared.Helpers;
using Microsoft.Graphics.Canvas;
using System;
using System.Numerics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Aurora.Music.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    [UriActivate("douban", Usage = ActivateUsage.Navigation)]
    public sealed partial class DoubanPage : Page
    {
        private float canvasHeight;
        private float canvasWidth;

        public DoubanPage()
        {
            this.InitializeComponent();
            MainPageViewModel.Current.Title = Consts.Localizer.GetString("DouText");
            MainPageViewModel.Current.NeedShowTitle = Window.Current.Bounds.Width >= 1008;
            MainPageViewModel.Current.NeedShowPanel = false;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
        }

        private void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            Context.Switch(e.ClickedItem as ChannelViewModel);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            Context.Detach();
            SizeChanged -= Visualizer_SizeChanged;
            MainPageViewModel.Current.IsVisualizing = false;
            Visualizer.Draw -= CustomVisualizer_Draw;
        }
        private void CustomVisualizer_Loaded(object sender, RoutedEventArgs e)
        {
            lock (drawingLock)
            {
                canvasHeight = 0.25f * (float)ActualHeight;
                canvasWidth = (float)Visualizer.ActualWidth;
                Context.Visualizer = Visualizer;
                SizeChanged += Visualizer_SizeChanged;
                Visualizer.Height = 0.25 * ActualHeight;
                barCount = Convert.ToUInt32(Math.Round(Visualizer.ActualWidth / 32));
                _emptySpectrum = SpectrumData.CreateEmpty(2, barCount, ScaleType.Linear, ScaleType.Linear, 0, 20000);

            }
        }

        private void Visualizer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            lock (drawingLock)
            {
                canvasHeight = 0.25f * (float)ActualHeight;
                canvasWidth = (float)Visualizer.ActualWidth;
                Visualizer.Height = 0.25 * ActualHeight;
                barCount = Convert.ToUInt32(Math.Round(Visualizer.ActualWidth / 32));
                _emptySpectrum = SpectrumData.CreateEmpty(2, barCount, ScaleType.Linear, ScaleType.Linear, 0, 20000);
                _previousSpectrum = null;
                _previousPeakSpectrum = null;
            }
        }

        private void CustomVisualizer_CreateResources(object sender, CreateResourcesEventArgs args)
        {

        }



        SpectrumData _emptySpectrum = SpectrumData.CreateEmpty(2, 32, ScaleType.Linear, ScaleType.Linear, 0, 20000);
        SpectrumData _previousSpectrum;
        SpectrumData _previousPeakSpectrum;
        private uint barCount = 32;
        private object drawingLock = new object();
        const double fps = 1000d / 60d;

        static readonly TimeSpan _rmsRiseTime = TimeSpan.FromMilliseconds(12 * fps);
        static readonly TimeSpan _rmsFallTime = TimeSpan.FromMilliseconds(12 * fps);
        static readonly TimeSpan _peakRiseTime = TimeSpan.FromMilliseconds(12 * fps);
        static readonly TimeSpan _peakFallTime = TimeSpan.FromMilliseconds(120 * fps);
        static readonly TimeSpan _frameDuration = TimeSpan.FromMilliseconds(fps);

        private void CustomVisualizer_Draw(IVisualizer sender, VisualizerDrawEventArgs args)
        {
            if (!MainPageViewModel.Current.IsVisualizing || Context.Palette == null)
                return;

            lock (drawingLock)
            {
                var drawingSession = (CanvasDrawingSession)args.DrawingSession;
                float barWidth = canvasWidth / barCount;
                // Calculate spectum metrics
                var barSize = new Vector2(barWidth, canvasHeight - 2 * barWidth);

                // Get the data if data exists and source is in play state, else use empty
                var spectrumData = args.Data != null && Visualizer.Source?.PlaybackState == SourcePlaybackState.Playing ?
                                                args.Data.Spectrum.LogarithmicTransform(barCount, 20f, 20000f) : _emptySpectrum;

                _previousSpectrum = spectrumData.ApplyRiseAndFall(_previousSpectrum, _rmsRiseTime, _rmsFallTime, _frameDuration);
                _previousPeakSpectrum = spectrumData.ApplyRiseAndFall(_previousPeakSpectrum, _peakRiseTime, _peakFallTime, _frameDuration);

                var logSpectrum = _previousSpectrum.ConvertToDecibels(-50, 0);
                var logPeakSpectrum = _previousPeakSpectrum.ConvertToDecibels(-50, 0);

                var step = canvasWidth / barCount;
                var flaw = (step - barSize.X) / 2;
                // Draw spectrum bars
                for (int index = 0, j = 0; index < barCount; index++, j++)
                {
                    float barX = step * index + flaw;

                    // use average of 2 channel and add a offset to pretty
                    float spectrumBarHeight = barSize.Y * (1.0f - (logSpectrum[0][index] + logSpectrum[1][index]) / -100.0f) + barSize.X;

                    if (j >= Context.Palette.Count)
                    {
                        j = 0;
                    }

                    drawingSession.FillRoundedRectangle(barX, 0, barSize.X, spectrumBarHeight, barSize.X / 2, barSize.X / 2, Context.Palette[j]);
                    drawingSession.FillRectangle(barX, 0, barSize.X, spectrumBarHeight - barSize.X / 2, Context.Palette[j]);
                }
            }
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            Visualizer.SizeChanged -= Visualizer_SizeChanged;
            MainPageViewModel.Current.IsVisualizing = false;
            Visualizer.Draw -= CustomVisualizer_Draw;
        }

        private void Adaptive_CurrentStateChanged(object sender, VisualStateChangedEventArgs e)
        {
            if (e.NewState.Name == "Full")
            {
                MainPageViewModel.Current.NeedShowTitle = true;
            }
            else
            {
                MainPageViewModel.Current.NeedShowTitle = false;
            }
        }

        private void Visualizer_Draw(object sender, VisualizerDrawEventArgs args)
        {

        }
    }
}
