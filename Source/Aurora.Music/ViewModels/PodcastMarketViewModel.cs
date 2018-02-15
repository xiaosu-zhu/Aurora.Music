// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Aurora.Shared.MVVM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Music.ViewModels
{
    class PodcastMarketViewModel : ViewModelBase
    {
        public ObservableCollection<GenericMusicItemViewModel> TopList { get; set; }

        public ObservableCollection<PodcastGroup> Genres { get; set; }

        public PodcastMarketViewModel()
        {
            TopList = new ObservableCollection<GenericMusicItemViewModel>();
            Genres = new ObservableCollection<PodcastGroup>();
        }
    }

    internal class PodcastGroup: ViewModelBase
    {
        public ObservableCollection<GenericMusicItemViewModel> Items { get; set; }

        public string Title { get; set; }
    }
}
