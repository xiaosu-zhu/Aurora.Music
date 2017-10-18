using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aurora.Shared.MVVM;

namespace Aurora.Music.ViewModels
{
    class AlbumViewModel : ViewModelBase
    {
        public ObservableCollection<SongViewModel> Songs { get; set; } = new ObservableCollection<SongViewModel>();
    }
}
