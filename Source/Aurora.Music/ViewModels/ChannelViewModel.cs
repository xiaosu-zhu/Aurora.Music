using Aurora.Music.Core.Models;
using Aurora.Shared.MVVM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Music.ViewModels
{
    class ChannelGroup : List<ChannelViewModel>, IGrouping<string, ChannelViewModel>
    {
        public string Name { get; set; }

        public int ID { get; set; }

        public string Key => Name;

        public ChannelGroup()
        {
        }
    }

    class ChannelViewModel : ViewModelBase
    {
        private string name;
        public string Name
        {
            get { return name; }
            set { SetProperty(ref name, value); }
        }

        private string desc;
        public string Description
        {
            get { return desc; }
            set { SetProperty(ref desc, value); }
        }

        public int ID { get; set; }

        private Uri cover;
        public Uri Cover
        {
            get { return cover; }
            set { SetProperty(ref cover, value); }
        }
    }
}
