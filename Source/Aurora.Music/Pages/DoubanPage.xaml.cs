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
    public sealed partial class DoubanPage : Page
    {
        private float canvasHeight;
        private float canvasWidth;

        public DoubanPage()
        {
            this.InitializeComponent();
            MainPageViewModel.Current.Title = Consts.Localizer.GetString("DouText");
            MainPageViewModel.Current.NeedShowTitle = true;
            MainPageViewModel.Current.LeftTopColor = Resources["SystemControlForegroundBaseHighBrush"] as SolidColorBrush;
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
            Visualizer.SizeChanged -= Visualizer_SizeChanged;
            MainPageViewModel.Current.IsVisualizing = false;
            Visualizer.Draw -= CustomVisualizer_Draw;
        }
        private void CustomVisualizer_Loaded(object sender, RoutedEventArgs e)
        {
            canvasHeight = (float)Visualizer.ActualHeight;
            canvasWidth = (float)Visualizer.ActualWidth;
            Context.Visualizer = Visualizer;
            Visualizer.SizeChanged += Visualizer_SizeChanged;
            Visualizer.Height = 0.25 * ActualHeight;
            MainPageViewModel.Current.IsVisualizing = true;
        }

        private void Visualizer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            canvasHeight = 0.25f * (float)ActualHeight;
            canvasWidth = (float)Visualizer.ActualWidth;
            Visualizer.Height = 0.25 * ActualHeight;
        }

        private void CustomVisualizer_CreateResources(object sender, CreateResourcesEventArgs args)
        {

        }



        SpectrumData _emptySpectrum = SpectrumData.CreateEmpty(2, 32, ScaleType.Linear, ScaleType.Linear, 0, 20000);
        SpectrumData _previousSpectrum;
        SpectrumData _previousPeakSpectrum;

        const double fps = 1000d / 60d;

        static readonly TimeSpan _rmsRiseTime = TimeSpan.FromMilliseconds(12 * fps);
        static readonly TimeSpan _rmsFallTime = TimeSpan.FromMilliseconds(12 * fps);
        static readonly TimeSpan _peakRiseTime = TimeSpan.FromMilliseconds(12 * fps);
        static readonly TimeSpan _peakFallTime = TimeSpan.FromMilliseconds(120 * fps);
        static readonly TimeSpan _frameDuration = TimeSpan.FromMilliseconds(fps);

        private void CustomVisualizer_Draw(IVisualizer sender, VisualizerDrawEventArgs args)
        {
            if (!MainPageViewModel.Current.IsVisualizing)
                return;
            var drawingSession = (CanvasDrawingSession)args.DrawingSession;


            float barWidth = canvasWidth / 32;
            // Calculate spectum metrics
            Vector2 barSize = new Vector2(barWidth, canvasHeight - 2 * barWidth);

            // Get the data if data exists and source is in play state, else use empty
            var spectrumData = args.Data != null && Visualizer.Source?.PlaybackState == SourcePlaybackState.Playing ?
                                            args.Data.Spectrum.LogarithmicTransform(32, 20f, 20000f) : _emptySpectrum;

            _previousSpectrum = spectrumData.ApplyRiseAndFall(_previousSpectrum, _rmsRiseTime, _rmsFallTime, _frameDuration);
            _previousPeakSpectrum = spectrumData.ApplyRiseAndFall(_previousPeakSpectrum, _peakRiseTime, _peakFallTime, _frameDuration);

            var logSpectrum = _previousSpectrum.ConvertToDecibels(-50, 0);
            var logPeakSpectrum = _previousPeakSpectrum.ConvertToDecibels(-50, 0);

            var step = canvasWidth / 32;
            var flaw = (step - barSize.X) / 2;
            // Draw spectrum bars
            for (int index = 0; index < 32; index++)
            {
                float barX = step * index + flaw;

                // use average of 2 channel
                float spectrumBarHeight = barSize.Y * (1.0f - (logSpectrum[0][index] + logSpectrum[1][index]) / -100.0f);

                drawingSession.FillRoundedRectangle(barX, 0, barSize.X, spectrumBarHeight, barSize.X / 2, barSize.X / 2, Context.Palette[index]);
                drawingSession.FillRectangle(barX, 0, barSize.X, spectrumBarHeight - barSize.X / 2, Context.Palette[index]);
            }
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            Visualizer.SizeChanged -= Visualizer_SizeChanged;
            MainPageViewModel.Current.IsVisualizing = false;
            Visualizer.Draw -= CustomVisualizer_Draw;
        }
    }
}
