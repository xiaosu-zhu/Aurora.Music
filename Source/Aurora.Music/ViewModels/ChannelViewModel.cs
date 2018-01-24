using Aurora.Music.Core.Models;
using Aurora.Music.Core.Models.Json;
using Aurora.Shared.Extensions;
using Aurora.Shared.Helpers;
using Aurora.Shared.MVVM;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Windows.Foundation.Collections;

namespace Aurora.Music.ViewModels
{
    class ChannelGroup : List<ChannelViewModel>, IGrouping<string, ChannelViewModel>, INotifyPropertyChanged
    {
        private string name;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Key"));
            }
        }

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
