using Aurora.Music.Core.Models;
using Aurora.Music.Core.Storage;
using Aurora.Shared.Extensions;
using Aurora.Shared.Helpers;
using Aurora.Shared.MVVM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.UI.Xaml;

namespace Aurora.Music.ViewModels
{
    class AddFoldersViewViewModel : ViewModelBase
    {
        private Settings settings;

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
                    folderPicker.FileTypeFilter.Add(".mp3");
                    folderPicker.FileTypeFilter.Add(".m4a");
                    folderPicker.FileTypeFilter.Add(".wav");
                    folderPicker.FileTypeFilter.Add(".flac");

                    StorageFolder folder = await folderPicker.PickSingleFolderAsync();
                    if (folder != null)
                    {
                        var opr = SQLOperator.Current();
                        if (await opr.AddFolderAsync(folder))
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
                });
            }
        }

        internal async Task RemoveFolder(FolderViewModel folderViewModel)
        {
            var opr = SQLOperator.Current();
            await opr.RemoveFolderAsync(folderViewModel.ID);
            Folders.Remove(folderViewModel);
        }

        private ElementTheme foreground = ElementTheme.Default;
        public ElementTheme Foreground
        {
            get { return foreground; }
            set { SetProperty(ref foreground, value); }
        }


        private bool includeMusicLibrary;
        public bool IncludeMusicLibrary
        {
            get { return includeMusicLibrary; }
            set
            {
                SetProperty(ref includeMusicLibrary, value);
                settings.IncludeMusicLibrary = value;
                settings.Save();
            }
        }

        public ObservableCollection<FolderViewModel> Folders { get; set; }

        public AddFoldersViewViewModel()
        {
            Folders = new ObservableCollection<FolderViewModel>();
            settings = Settings.Load();
            IncludeMusicLibrary = settings.IncludeMusicLibrary;
        }

        public void ChangeForeGround()
        {
            Foreground = ElementTheme.Dark;
        }

        public async Task Init()
        {
            var opr = SQLOperator.Current();
            var folders = await opr.GetAllAsync<FOLDER>();
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                foreach (var item in folders)
                {
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
                FolderName = "Not Avaliable";
                SongsCount = 0;
                return;
            }
            Disk = item.Path.Split(':').FirstOrDefault();
            Path = item.Path;
            FolderName = Folder.DisplayName;
            SongsCount = item.SongsCount;
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

        public string FormatCount(int count)
        {
            if (count < 0)
            {
                return "Need Scan";
            }
            return count == 1 ? count + " Song" : count + " Songs";
        }
    }
}
