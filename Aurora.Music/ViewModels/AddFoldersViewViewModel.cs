using Aurora.Music.Core.Models;
using Aurora.Music.Core.Storage;
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

        private bool includeMusicLibrary;
        public bool IncludeMusicLibrary
        {
            get { return includeMusicLibrary; }
            set
            {
                SetProperty(ref includeMusicLibrary, value);
                settings.IncludeMusicLibrary = value;
            }
        }

        public ObservableCollection<FolderViewModel> Folders { get; set; }

        public AddFoldersViewViewModel()
        {
            Folders = new ObservableCollection<FolderViewModel>();
            settings = Settings.Load();
            IncludeMusicLibrary = settings.IncludeMusicLibrary;
        }

        public async Task Init()
        {
            var opr = SQLOperator.Current();
            var folders = await opr.GetAllAsync<Folder>();
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                foreach (var item in folders)
                {
                    Folders.Add(new FolderViewModel(item));
                }
            });
        }
    }

    public class FolderViewModel
    {
        public FolderViewModel(Folder item)
        {
            Disk = item.Path.Split(':').FirstOrDefault();
            Path = item.Path;
            Folder = AsyncHelper.RunSync(async () => await StorageFolder.GetFolderFromPathAsync(item.Path));
            FolderName = Folder.DisplayName;
            SongsCount = item.SongsCount;
        }

        public FolderViewModel() { }

        public string Disk { get; set; }

        public string FolderName { get; set; }

        public string Path { get; set; }

        public int SongsCount { get; set; }

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
