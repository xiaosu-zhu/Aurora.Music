using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aurora.Music.Core.Storage;
using Aurora.Shared.Helpers;
using Aurora.Shared.MVVM;

namespace Aurora.Music.ViewModels
{
    class AlbumsPageViewModel : ViewModelBase
    {
        public ObservableCollection<AlbumViewModel> AlbumList { get; set; } = new ObservableCollection<AlbumViewModel>();

        public AlbumsPageViewModel()
        {

        }

        public async Task GetAlbums()
        {
            var albums = await FileReader.ReadAlbumsAsync();
            foreach (var item in albums)
            {
                AlbumList.Add(new AlbumViewModel(item));
            }
        }
    }
}
