// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System;
using System.Threading.Tasks;

using Aurora.Music.Core.Models;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace Aurora.Music.Controls
{
    public sealed partial class LimiterSettings : ContentDialog
    {
        public LimiterSettings()
        {
            InitializeComponent();
            RequestedTheme = Settings.Current.Theme;
            Attack.Value = Settings.Current.CompressorAttack;
            Release.Value = Settings.Current.CompressorRelease;
            Ratio.Value = Settings.Current.CompressorRatio;
            Gain.Value = Settings.Current.CompressorMakeUpGain;
            Threshold.Value = Settings.Current.CompressorThresholddB;
            Attack.ValueChanged += Attack_ValueChanged;
            Release.ValueChanged += Release_ValueChanged;
            Ratio.ValueChanged += Ratio_ValueChanged;
            Gain.ValueChanged += Gain_ValueChanged;
            Threshold.ValueChanged += Threshold_ValueChanged;
        }

        private void Threshold_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            Settings.Current.CompressorThresholddB = Convert.ToSingle(e.NewValue);
            if (Effects.Threshold.Current != null)
                Effects.Threshold.Current.ThresholddB = Settings.Current.CompressorThresholddB;

            Task.Run(() => Settings.Current.Save());
        }

        private void Gain_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            Settings.Current.CompressorMakeUpGain = Convert.ToSingle(e.NewValue);
            if (Effects.Threshold.Current != null)
                Effects.Threshold.Current.MakeUpGain = Settings.Current.CompressorMakeUpGain;

            Task.Run(() => Settings.Current.Save());
        }

        private void Ratio_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            Settings.Current.CompressorRatio = Convert.ToSingle(e.NewValue);
            if (Effects.Threshold.Current != null)
                Effects.Threshold.Current.Ratio = Settings.Current.CompressorRatio;

            Task.Run(() => Settings.Current.Save());
        }

        private void Release_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            Settings.Current.CompressorRelease = Convert.ToSingle(e.NewValue);
            if (Effects.Threshold.Current != null)
                Effects.Threshold.Current.Release = Settings.Current.CompressorRelease;

            Task.Run(() => Settings.Current.Save());
        }

        private void Attack_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            Settings.Current.CompressorAttack = Convert.ToSingle(e.NewValue);
            if (Effects.Threshold.Current != null)
                Effects.Threshold.Current.Attack = Settings.Current.CompressorAttack;

            Task.Run(() => Settings.Current.Save());
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            Attack.Value = 10;
            Release.Value = 10;
            Gain.Value = 0;
            Threshold.Value = 0;
            Ratio.Value = 1;
        }
    }
}
