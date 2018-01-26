// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Aurora.Music.Core;
using Aurora.Music.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System.Threading;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

namespace Aurora.Music.Pages
{
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            this.InitializeComponent();
            LoactionFrame.Navigate(typeof(AddFoldersView));
            MainPageViewModel.Current.Title = Consts.Localizer.GetString("SettingsText");
            MainPageViewModel.Current.NeedShowTitle = true;
            MainPageViewModel.Current.LeftTopColor = Resources["SystemControlForegroundBaseHighBrush"] as SolidColorBrush;
            var t = ThreadPool.RunAsync(async x =>
            {
                await Context.Init();
            });
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            SizeChanged -= Page_SizeChanged;
            OnlineCombo.SelectionChanged -= OnlineCombo_SelectionChanged;
            LrcCombo.SelectionChanged -= LyricCombo_SelectionChanged;
            MetaCombo.SelectionChanged -= MetaCombo_SelectionChanged;
        }

        private void Ellipse_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {

        }

        private void Ellipse_PointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {

        }

        private void Ellipse_PointerReleased(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            var el = sender as Ellipse;
            Context.ToggleEffectState((string)el.Tag);
        }

        private void Ellipse_PointerCanceled(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {

        }

        double GetPosition(bool b)
        {
            return b ? 0d : -48d;
        }

        AcrylicBrush GetBrush(bool b)
        {
            return (AcrylicBrush)(b ? Resources["SystemControlAccentAcrylicWindowAccentMediumHighBrush"] : Resources["SystemControlAltLowAcrylicWindowBrush"]);
        }

        SolidColorBrush GetForeground(bool b)
        {
            return (SolidColorBrush)(b ? Resources["SystemControlForegroundBaseHighBrush"] : Resources["SystemControlForegroundChromeGrayBrush"]);
        }

        private async void Hyperlink_Click(Windows.UI.Xaml.Documents.Hyperlink sender, Windows.UI.Xaml.Documents.HyperlinkClickEventArgs args)
        {
            await Context.PurchaseOnlineExtension();
        }

        private async void OnlineCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Context.ChangeOnlineExt((sender as ComboBox).SelectedItem);

            await MainPageViewModel.Current.ReloadExtensions();
        }

        private async void LyricCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Context.ChangeLyricExt((sender as ComboBox).SelectedItem);

            await MainPageViewModel.Current.ReloadExtensions();
        }

        private async void MetaCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Context.ChangeMetaExt((sender as ComboBox).SelectedItem);

            await MainPageViewModel.Current.ReloadExtensions();
        }

        private void Page_SizeChanged(object sender, Windows.UI.Xaml.SizeChangedEventArgs e)
        {
            Main.Width = Window.Current.Bounds.Width;
        }

        private void Main_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Main.Width = Window.Current.Bounds.Width;
            SizeChanged += Page_SizeChanged;
            OnlineCombo.SelectionChanged += OnlineCombo_SelectionChanged;
            LrcCombo.SelectionChanged += LyricCombo_SelectionChanged;
            MetaCombo.SelectionChanged += MetaCombo_SelectionChanged;
        }

        private void Page_Unloaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            SizeChanged -= Page_SizeChanged;
        }
    }
}
