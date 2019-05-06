// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Aurora.Music.Core;
using Aurora.Music.Core.Models;
using Aurora.Music.Core.Storage;
using Aurora.Shared.Extensions;
using Aurora.Shared.Helpers;
using Aurora.Shared.MVVM;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace Aurora.Music.ViewModels
{
    class AddFoldersViewViewModel : ViewModelBase
    {
        public DelegateCommand AddFolderCommand
        {
            get
            {
                return new DelegateCommand(async () =>
                {
                    var folderPicker = new Windows.Storage.Pickers.FolderPicker
                    {
                        SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.ComputerFolder
                    };
                    foreach (var ext in Consts.PlaylistType)
                    {
                        folderPicker.FileTypeFilter.Add(ext);
                    }
                    foreach (var ext in Consts.FileTypes)
                    {
                        folderPicker.FileTypeFilter.Add(ext);
                    }

                    var folder = await folderPicker.PickSingleFolderAsync();
                    if (folder != null)
                    {
                        var opr = SQLOperator.Current();
                        if (await opr.AddFolderAsync(folder, false))
                        {
                            var l = await opr.GetFolderAsync(folder.Path);
                            foreach (var item in l)
                            {
                                Folders.Add(new FolderViewModel(item));
                            }
                        }
                    }
                    else
                    {
                        return;
                    }

                    if (MainPageViewModel.Current != null)
                    {
                        var t = Task.Run(async () =>
                        {
                            await MainPageViewModel.Current.FilesChangedAsync();
                        });
                    }
                });
            }
        }

        public DelegateCommand FilterFolderCommand
        {
            get
            {
                return new DelegateCommand(async () =>
                {
                    var folderPicker = new Windows.Storage.Pickers.FolderPicker
                    {
                        SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.ComputerFolder
                    };
                    foreach (var ext in Consts.PlaylistType)
                    {
                        folderPicker.FileTypeFilter.Add(ext);
                    }
                    foreach (var ext in Consts.FileTypes)
                    {
                        folderPicker.FileTypeFilter.Add(ext);
                    }

                    var folder = await folderPicker.PickSingleFolderAsync();
                    if (folder != null)
                    {
                        var opr = SQLOperator.Current();
                        if (await opr.AddFolderAsync(folder, true))
                        {
                            var l = await opr.GetFolderAsync(folder.Path);
                            foreach (var item in l)
                            {
                                Folders.Add(new FolderViewModel(item));
                            }
                        }
                    }
                    else
                    {
                        return;
                    }

                    if (MainPageViewModel.Current != null)
                    {
                        var t = Task.Run(async () =>
                        {
                            await MainPageViewModel.Current.FilesChangedAsync();
                        });
                    }
                });
            }
        }

        private bool durationFilterEnabled = Settings.Current.FileDurationFilterEnabled;
        public bool DurationFilterEnabled
        {
            get { return durationFilterEnabled; }
            set
            {
                Settings.Current.FileDurationFilterEnabled = value;
                Settings.Current.Save();
                SetProperty(ref durationFilterEnabled, value);
            }
        }

        private bool sizeFilterEnabled = Settings.Current.FileSizeFilterEnabled;
        public bool SizeFilterEnabled
        {
            get { return sizeFilterEnabled; }
            set
            {
                Settings.Current.FileSizeFilterEnabled = value;
                Settings.Current.Save();
                SetProperty(ref sizeFilterEnabled, value);
            }
        }

        private double duration = Settings.Current.FileDurationFilter;
        public double Duration
        {
            get { return duration; }
            set
            {
                Settings.Current.FileDurationFilter = Convert.ToUInt32(value);
                Settings.Current.Save();
                SetProperty(ref duration, value);
            }
        }

        public string DurationString(double d)
        {
            var t = TimeSpan.FromMilliseconds(d);
            return t.ToString(@"mm\:ss\.fff");
        }

        private double size = Math.Round(Settings.Current.FileSizeFilter / 1024d);
        public double Size
        {
            get { return size; }
            set
            {
                Settings.Current.FileSizeFilter = Convert.ToUInt32(value * 1024);
                Settings.Current.Save();
                SetProperty(ref size, value);
            }
        }

        public string SizeString(double s1)
        {
            s1 *= 1024;
            if (s1 < 1024ul)
            {
                return $"{s1}B";
            }
            if (s1 < 1024ul * 1024ul)
            {
                return $"{(s1 / 1024d).ToString("0.#")}KiB";
            }
            else
            {
                return $"{(s1 / (1024d * 1024d)).ToString("0.#")}MiB";
            }
        }

        internal async Task RemoveFolder(FolderViewModel folderViewModel)
        {
            var opr = SQLOperator.Current();
            await opr.RemoveFolderAsync(folderViewModel.ID);
            Folders.Remove(folderViewModel);
            if (MainPageViewModel.Current != null)
            {
                var t = Task.Run(async () =>
                {
                    await MainPageViewModel.Current.FilesChangedAsync();
                });
            }
        }

        private ElementTheme foreground = ElementTheme.Default;
        public ElementTheme Foreground
        {
            get { return foreground; }
            set { SetProperty(ref foreground, value); }
        }


        private bool includeMusicLibrary = Settings.Current.IncludeMusicLibrary;
        public bool IncludeMusicLibrary
        {
            get { return includeMusicLibrary; }
            set
            {
                SetProperty(ref includeMusicLibrary, value);
                Settings.Current.IncludeMusicLibrary = value;
                Settings.Current.Save();
            }
        }

        public ObservableCollection<FolderViewModel> Folders { get; set; }

        public AddFoldersViewViewModel()
        {
            Folders = new ObservableCollection<FolderViewModel>();
        }

        public void ChangeForeGround()
        {
            Foreground = ElementTheme.Dark;
        }

        public async Task Init()
        {
            var opr = SQLOperator.Current();
            var folders = await opr.GetAllAsync<FOLDER>();
            var p = from g in folders orderby g.IsFiltered select g;
            await CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                foreach (var item in p)
                {
                    if (item.Path == KnownFolders.MusicLibrary.Path || item.Path.Contains(ApplicationData.Current.LocalFolder.Path))
                    {
                        continue;
                    }
                    if (!item.Path.IsNullorEmpty())
                        Folders.Add(new FolderViewModel(item));
                }
            });
        }
    }

    class FolderViewModel : ViewModelBase
    {
        public FolderViewModel(FOLDER item)
        {
            ID = item.ID;
            Folder = AsyncHelper.RunSync(async () => await item.GetFolderAsync());
            if (Folder == null)
            {
                NotAvaliable = true;
                Disk = "N/A";
                Path = item.Path;
                FolderName = Consts.Localizer.GetString("NotAvaliableText");
                SongsCount = 0;
                return;
            }
            Disk = item.Path.Split(':').FirstOrDefault();
            Path = item.Path;
            FolderName = Folder.DisplayName;
            SongsCount = item.SongsCount;
            IsFiltered = item.IsFiltered;
        }

        private bool isFiltered;
        public bool IsFiltered
        {
            get { return isFiltered; }
            set { SetProperty(ref isFiltered, value); }
        }

        private bool notAva;
        public bool NotAvaliable
        {
            get { return notAva; }
            set { SetProperty(ref notAva, value); }
        }

        private bool isOpened;

        public bool IsOpened
        {
            get { return isOpened; }
            set { SetProperty(ref isOpened, value); }
        }

        public FolderViewModel() { }

        public string Disk { get; set; }

        public string FolderName { get; set; }

        public string Path { get; set; }

        public int SongsCount { get; set; }
        public int ID { get; }
        public StorageFolder Folder { get; set; }

        public Brush FilteredBackground(bool b)
        {
            return b ? new SolidColorBrush(Color.FromArgb(0x33, 0xff, 0x43, 0x43)) : new SolidColorBrush();
        }

        public string FormatCount(bool b, int count)
        {
            if (b)
            {
                return Consts.Localizer.GetString("FilteredfolderText");
            }
            if (count < 0)
            {
                return Consts.Localizer.GetString("NeedScanText");
            }
            return SmartFormat.Smart.Format(Consts.Localizer.GetString("SmartSongs"), count);
        }
    }
}
