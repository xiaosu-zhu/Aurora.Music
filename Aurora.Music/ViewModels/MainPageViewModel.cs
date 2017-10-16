using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aurora.Music.Core.Storage;
using Aurora.Shared.MVVM;
using Windows.Storage;

namespace Aurora.Music.ViewModels
{
    class MainPageViewModel:ViewModelBase
    {
        public async Task Go()
        {
            var picker = new Windows.Storage.Pickers.FolderPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.MusicLibrary;
            picker.FileTypeFilter.Add(".mp3");
            picker.FileTypeFilter.Add(".m4a");
            picker.FileTypeFilter.Add(".flac");

            StorageFolder folder = await picker.PickSingleFolderAsync();
            await FileReader.Read(folder);
        }
    }
}
