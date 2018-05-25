// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Aurora.Music.Core.Models;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace Aurora.Music.Controls
{
    public sealed partial class EqualizerSettings : ContentDialog
    {
        private bool presetsDo;

        public EqualizerSettings()
        {
            InitializeComponent();
            RequestedTheme = Settings.Current.Theme;

            Slider0.Tag = 0;
            Slider1.Tag = 1;
            Slider2.Tag = 2;
            Slider3.Tag = 3;
            Slider4.Tag = 4;
            Slider5.Tag = 5;
            Slider6.Tag = 6;
            Slider7.Tag = 7;
            Slider8.Tag = 8;
            Slider9.Tag = 9;
            Slider0.Value = Settings.Current.Gain[0];
            Slider1.Value = Settings.Current.Gain[1];
            Slider2.Value = Settings.Current.Gain[2];
            Slider3.Value = Settings.Current.Gain[3];
            Slider4.Value = Settings.Current.Gain[4];
            Slider5.Value = Settings.Current.Gain[5];
            Slider6.Value = Settings.Current.Gain[6];
            Slider7.Value = Settings.Current.Gain[7];
            Slider8.Value = Settings.Current.Gain[8];
            Slider9.Value = Settings.Current.Gain[9];
            Presets.SelectionChanged += Presets_SelectionChanged;

            if (Settings.Current.AudioGraphEffects.HasFlag(Core.Models.Effects.Equalizer))
            {

            }
            else
            {
                Slider0.IsEnabled = false;
                Slider1.IsEnabled = false;
                Slider2.IsEnabled = false;
                Slider3.IsEnabled = false;
                Slider4.IsEnabled = false;
                Slider5.IsEnabled = false;
                Slider6.IsEnabled = false;
                Slider7.IsEnabled = false;
                Slider8.IsEnabled = false;
                Slider9.IsEnabled = false;
                Presets.IsEnabled = false;
                Reset.IsEnabled = false;
            }
        }

        private void Slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (presetsDo)
            {
                return;
            }
            Presets.SelectionChanged -= Presets_SelectionChanged;
            Presets.SelectedIndex = 1;
            Settings.Current.Gain[(int)(sender as Slider).Tag] = (float)e.NewValue;
            Presets.SelectionChanged += Presets_SelectionChanged;

            PlaybackEngine.PlaybackEngine.Current.ChangeEQ(Settings.Current.Gain);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Presets.SelectedIndex = 0;
        }

        private void Presets_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((Presets.SelectedItem as ComboBoxItem).Content as string == "Custom")
            {
                return;
            }
            var set = Core.Models.Presets.Instance[(Presets.SelectedItem as ComboBoxItem).Content as string];
            presetsDo = true;
            Slider0.Value = set[0];
            Slider1.Value = set[1];
            Slider2.Value = set[2];
            Slider3.Value = set[3];
            Slider4.Value = set[4];
            Slider5.Value = set[5];
            Slider6.Value = set[6];
            Slider7.Value = set[7];
            Slider8.Value = set[8];
            Slider9.Value = set[9];
            presetsDo = false;

            Settings.Current.Gain = set;

            PlaybackEngine.PlaybackEngine.Current.ChangeEQ(set);
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Settings.Current.Save();
        }
    }
}
