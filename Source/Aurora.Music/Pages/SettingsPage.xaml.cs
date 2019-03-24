// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Aurora.Music.Core;
using Aurora.Music.Core.Models;
using Aurora.Music.ViewModels;
using Aurora.Shared.Helpers;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Aurora.Music.Pages
{
    [UriActivate("settings", Usage = ActivateUsage.Navigation)]
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            InitializeComponent();
            LoactionFrame.Navigate(typeof(AddFoldersView));
            //MainPageViewModel.Current.Title = Consts.Localizer.GetString("SettingsText");
            MainPageViewModel.Current.NeedShowTitle = false;

            // slider swallowed PointerReleasedEvent
            VolumeSlider.AddHandler(PointerReleasedEvent, new PointerEventHandler(Slider_PointerReleased), true);

            SystemTheme.Checked -= RadioButton_Checked;
            LightTheme.Checked -= RadioButton_Checked;
            DarkTheme.Checked -= RadioButton_Checked;
            AutoTheme.Checked -= RadioButton_Checked;
            SunThemeChecker.Checked -= SunThemeChecker_Checked;
            SunThemeChecker.Unchecked -= SunThemeChecker_Checked;
            SunThemeChecker.IsChecked = Settings.Current.SunTheme;
            if (Settings.Current.AutoTheme)
            {
                AutoTheme.IsChecked = true;
            }
            else
            {
                AutoTheme.IsChecked = false;
                switch (Settings.Current.Theme)
                {
                    case ElementTheme.Default:
                        SystemTheme.IsChecked = true;
                        break;
                    case ElementTheme.Light:
                        LightTheme.IsChecked = true;
                        break;
                    case ElementTheme.Dark:
                        DarkTheme.IsChecked = true;
                        break;
                    default:
                        SystemTheme.IsChecked = true;
                        break;
                }
            }
            SystemTheme.Checked += RadioButton_Checked;
            LightTheme.Checked += RadioButton_Checked;
            DarkTheme.Checked += RadioButton_Checked;
            AutoTheme.Checked += RadioButton_Checked;
            SunThemeChecker.Checked += SunThemeChecker_Checked;
            SunThemeChecker.Unchecked += SunThemeChecker_Checked;

            Task.Run(async () =>
            {
                await Context.Init();
            });
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            OnlineCombo.SelectionChanged -= OnlineCombo_SelectionChanged;
            LrcCombo.SelectionChanged -= LyricCombo_SelectionChanged;
            MetaCombo.SelectionChanged -= MetaCombo_SelectionChanged;
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

            await MainPageViewModel.Current.ReloadExtensionsAsync();
        }

        private async void LyricCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Context.ChangeLyricExt((sender as ComboBox).SelectedItem);

            await MainPageViewModel.Current.ReloadExtensionsAsync();
        }

        private async void MetaCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Context.ChangeMetaExt((sender as ComboBox).SelectedItem);

            await MainPageViewModel.Current.ReloadExtensionsAsync();
        }

        private void Main_Loaded(object sender, RoutedEventArgs e)
        {
            OnlineCombo.SelectionChanged += OnlineCombo_SelectionChanged;
            LrcCombo.SelectionChanged += LyricCombo_SelectionChanged;
            MetaCombo.SelectionChanged += MetaCombo_SelectionChanged;
            DeviceCombo.SelectionChanged += DeviceCombo_SelectionChanged;
            EngineCombo.SelectionChanged += EngineCombo_SelectionChanged;
            MainPivot.Height = Main.ActualHeight - Main.Padding.Top - Main.Padding.Top;
            Main.SizeChanged += SettingsPage_SizeChanged;
        }

        private void EngineCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Context.EngineIndex = EngineCombo.SelectedIndex;
        }

        private void DeviceCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Context.AudioSelectedIndex = DeviceCombo.SelectedIndex;
        }

        private void SettingsPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            MainPivot.Height = Main.ActualHeight - Main.Padding.Top - Main.Padding.Top;
        }

        private void ToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            var el = sender as ToggleSwitch;
            Context.ToggleEffectState((string)el.Tag);
        }

        private void ToggleSwitch_Loaded(object sender, RoutedEventArgs e)
        {
            (sender as ToggleSwitch).Toggled += ToggleSwitch_Toggled;
        }

        private void Slider_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (PlaybackEngine.PlaybackEngine.Current.IsPlaying == null || PlaybackEngine.PlaybackEngine.Current.IsPlaying == false)
            {
                var player = new Windows.Media.Playback.MediaPlayer
                {
                    Source = Windows.Media.Core.MediaSource.CreateFromUri(new Uri("ms-winsoundevent:Notification.Reminder"))
                };
                player.Volume = Settings.Current.PlayerVolume / 100d;
                player.MediaEnded += (a, v) =>
                {
                    player.Dispose();
                };
                player.Play();
            }
        }

        private void Left_Click(object sender, RoutedEventArgs e)
        {
            ShiftSlider.Value -= 0.1;
        }

        private void Right_Click(object sender, RoutedEventArgs e)
        {
            ShiftSlider.Value += 0.1;
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            Main.SizeChanged -= SettingsPage_SizeChanged;
        }

        private void Context_InitComplete(object sender, EventArgs e)
        {
            // PivotItem loaded with latency, only when Pivot move to its neighbor, these controls start to load, and Items become sync with DataContext
#pragma warning disable CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
            Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, async () =>
            {
                while (MetaCombo.Items.Count < 1) await Task.Delay(500);
                OnlineCombo.SelectionChanged -= OnlineCombo_SelectionChanged;
                LrcCombo.SelectionChanged -= LyricCombo_SelectionChanged;
                MetaCombo.SelectionChanged -= MetaCombo_SelectionChanged;

                OnlineCombo.SelectedIndex = Context.CurrentOnlineIndex;
                LrcCombo.SelectedIndex = Context.CurrentLyricIndex;
                MetaCombo.SelectedIndex = Context.CurrentMetaIndex;

                OnlineCombo.SelectionChanged += OnlineCombo_SelectionChanged;
                LrcCombo.SelectionChanged += LyricCombo_SelectionChanged;
                MetaCombo.SelectionChanged += MetaCombo_SelectionChanged;
            });
            Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, async () =>
            {
                while (DeviceCombo.Items.Count < 1) await Task.Delay(500);
                DeviceCombo.SelectionChanged -= DeviceCombo_SelectionChanged;
                DeviceCombo.SelectedIndex = Context.AudioSelectedIndex;
                DeviceCombo.SelectionChanged += DeviceCombo_SelectionChanged;
            });

            Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, async () =>
            {
                while (EngineCombo.Items.Count < 1) await Task.Delay(500);
                EngineCombo.SelectionChanged -= EngineCombo_SelectionChanged;
                EngineCombo.SelectedIndex = Context.EngineIndex;
                EngineCombo.SelectionChanged += EngineCombo_SelectionChanged;
            });
#pragma warning restore CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            var b = sender as RadioButton;
            switch (b.Tag)
            {
                case "System":
                    Context.SetAutoTheme(false);
                    Context.ChangeTheme(ElementTheme.Default);
                    break;
                case "Light":
                    Context.SetAutoTheme(false);
                    Context.ChangeTheme(ElementTheme.Light);
                    break;
                case "Dark":
                    Context.SetAutoTheme(false);
                    Context.ChangeTheme(ElementTheme.Dark);
                    break;
                case "Auto":
                    Context.SetAutoTheme(true);
                    break;
                default:
                    Context.SetAutoTheme(false);
                    Context.ChangeTheme(ElementTheme.Default);
                    break;
            }
        }

        Visibility SunThemeVis(bool? b)
        {
            if (b is bool a && a)
            {
                return Visibility.Collapsed;
            }
            return Visibility.Visible;
        }

        private void SunThemeChecker_Checked(object sender, RoutedEventArgs e)
        {
            Context.SetSunTheme(SunThemeChecker.IsChecked is bool a && a);
        }
    }
}
