using Aurora.Music.Core.Models;
using Aurora.Music.Core.Storage;
using Aurora.Shared.MVVM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                        // Application now has read/write access to all contents in the picked folder
                        // (including other sub-folder contents)
                        Windows.Storage.AccessCache.StorageApplicationPermissions.
                        FutureAccessList.AddOrReplace("PickedFolderToken", folder);

                        var opr = SQLOperator.Current();
                        if (await opr.AddFolderAsync(folder))
                        {
                            Folders.Add(new FolderViewModel(new Folder(folder)));
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

        public ObservableCollection<FolderViewModel> Folders { get; set; } = new ObservableCollection<FolderViewModel>();

        public AddFoldersViewViewModel()
        {
            settings = Settings.Load();
            IncludeMusicLibrary = settings.IncludeMusicLibrary;
        }

        public async Task Init()
        {
            var opr = SQLOperator.Current();
            var folders = await opr.GetAllAsync<Folder>();
            foreach (var item in folders)
            {
                Folders.Add(new FolderViewModel
                {
                    Disk = item.Path.Split(':').FirstOrDefault(),
                    Path = item.Path,
                    FolderName = item.Name,
                    SongsCount = item.SongsCount
                });
            }
        }
    }

    public class FolderViewModel
    {
        public FolderViewModel(Folder item)
        {
            Disk = item.Path.Split(':').FirstOrDefault();
            Path = item.Path;
            FolderName = item.Name;
            SongsCount = item.SongsCount;
        }

        public FolderViewModel() { }

        public string Disk { get; set; }

        public string FolderName { get; set; }

        public string Path { get; set; }

        public int SongsCount { get; set; }

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
