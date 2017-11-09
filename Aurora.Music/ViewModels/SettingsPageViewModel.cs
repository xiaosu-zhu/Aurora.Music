using Aurora.Music.Core.Models;
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
                MainPageViewModel.Current.GetPlayer().ChangeAudioEndPoint(settings.OutputDeviceID);
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
                    MainPageViewModel.Current.GetPlayer().ChangeVolume(value);
                    settings.PlayerVolume = value;
                    settings.Save();
                }

                SetProperty(ref playerVolume, value);
            }
        }

        private int lyricSource;
        public int LyricSource
        {
            get { return lyricSource; }
            set
            {
                SetProperty(ref lyricSource, value);
                settings.LyricSource = value;
            }
        }

        public SettingsPageViewModel()
        {
            settings = Settings.Load();
            PlayerVolume = settings.PlayerVolume;
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
