using Aurora.Music.Core.Models;
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
                SetProperty(ref audioSelectedIndex, value);
                settings.OutputDeviceID = DevicList[value].ID;
            }
        }

        public SettingsPageViewModel()
        {
            settings = Settings.Load();
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
                    Name = "Auto",
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
