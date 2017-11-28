using Aurora.Music.Core.Models;
using Aurora.Music.PlaybackEngine;
using Aurora.Shared.Helpers;
using Aurora.Shared.MVVM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Devices.Enumeration;
using Windows.Media.Devices;

namespace Aurora.Music.ViewModels
{
    class SettingsPageViewModel : ViewModelBase
    {
        private int audioSelectedIndex = -1;
        public int AudioSelectedIndex
        {
            get { return audioSelectedIndex; }
            set
            {
                if (value == -1)
                    return;
                SetProperty(ref audioSelectedIndex, value);

                settings.OutputDeviceID = DevicList[value].ID;
                settings.Save();
                Player.Current.ChangeAudioEndPoint(settings.OutputDeviceID);
            }
        }

        private double playerVolume;
        public double PlayerVolume
        {
            get { return playerVolume; }
            set
            {
                if (!value.AlmostEqualTo(playerVolume))
                {
                    Player.Current.ChangeVolume(value);
                    settings.PlayerVolume = value;
                    settings.Save();
                }

                SetProperty(ref playerVolume, value);
            }
        }

        internal void ToggleEffectState(string tag)
        {
            switch (tag)
            {
                case "Threshold":
                    settings.AudioGraphEffects ^= Effects.Limiter;
                    break;
                case "Equalizer":
                    settings.AudioGraphEffects ^= Effects.Equalizer;
                    break;
                case "Reverb":
                    settings.AudioGraphEffects ^= Effects.Reverb;
                    break;
                default:
                    break;
            }

            settings.Save();

            EqualizerEnabled = settings.AudioGraphEffects.HasFlag(Effects.Equalizer);
            ThresholdEnabled = settings.AudioGraphEffects.HasFlag(Effects.Limiter);
            ReverbEnabled = settings.AudioGraphEffects.HasFlag(Effects.Reverb);
        }

        private bool equalizerEnabled;
        public bool EqualizerEnabled
        {
            get { return equalizerEnabled; }
            set { SetProperty(ref equalizerEnabled, value); }
        }

        private bool threshold;
        public bool ThresholdEnabled
        {
            get { return threshold; }
            set { SetProperty(ref threshold, value); }
        }

        private bool reverb;
        public bool ReverbEnabled
        {
            get { return reverb; }
            set { SetProperty(ref reverb, value); }
        }

        public SettingsPageViewModel()
        {
            settings = Settings.Load();
            PlayerVolume = settings.PlayerVolume;
            EqualizerEnabled = settings.AudioGraphEffects.HasFlag(Effects.Equalizer);
            ThresholdEnabled = settings.AudioGraphEffects.HasFlag(Effects.Limiter);
            ReverbEnabled = settings.AudioGraphEffects.HasFlag(Effects.Reverb);
        }

        public ObservableCollection<DeviceInformationViewModel> DevicList = new ObservableCollection<DeviceInformationViewModel>();
        private Settings settings;

        public async Task Init()
        {
            string audioSelector = MediaDevice.GetAudioRenderSelector();
            var outputDevices = await DeviceInformation.FindAllAsync(audioSelector);
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                DevicList.Add(new DeviceInformationViewModel()
                {
                    Name = "System default",
                    ID = null,
                    Tag = null
                });

                foreach (var device in outputDevices)
                {
                    //var deviceItem = new ComboBoxItem();
                    //deviceItem.Content = device.Name;
                    //deviceItem.Tag = device;
                    //_audioDeviceComboBox.Items.Add(deviceItem);
                    DevicList.Add(new DeviceInformationViewModel()
                    {
                        Name = device.Name,
                        ID = device.Id,
                        Tag = device
                    });
                }

            });

            await Task.Delay(100);
            var index = DevicList.IndexOf(DevicList.First(x => x.ID == settings.OutputDeviceID));
            if (index == -1)
                index = 0;
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, () =>
            {
                AudioSelectedIndex = index;
            });
        }
    }

    class DeviceInformationViewModel : ViewModelBase
    {
        public string Name { get; set; }
        public string ID { get; set; }
        public DeviceInformation Tag { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
