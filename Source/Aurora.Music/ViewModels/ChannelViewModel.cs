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

        internal async Task<playlist> RequestPlayListAsync()
        {
            var args = new Dictionary<string, string>()
            {
                ["channel"] = ID.ToString(),
                ["from"] = "mainsite",
                ["pt"] = "0.0",
                ["kbps"] = "128",
                ["formats"] = "aac",
                ["alt"] = "json",
                ["app_name"] = "radio_iphone",
                ["apikey"] = "02646d3fb69a52ff072d47bf23cef8fd",
                ["client"] = "s:mobile|y:iOS 10.2|f:115|d:b88146214e19b8a8244c9bc0e2789da68955234d|e:iPhone7,1|m:appstore",
                ["client_id"] = "02646d3fb69a52ff072d47bf23cef8fd",
                ["icon_cate"] = "xlarge",
                ["udid"] = "b88146214e19b8a8244c9bc0e2789da68955234d",
                ["douban_udid"] = "b635779c65b816b13b330b68921c0f8edc049590",
                ["version"] = "115",
                ["type"] = "n"
            };

            Dictionary<string, string> addHeader = null;

            if (Settings.Current.VerifyDoubanLogin())
            {
                addHeader = new Dictionary<string, string>()
                {
                    ["Authorization"] = $"Bearer {Settings.Current.DoubanToken}"
                };
            }

            var json = await ApiRequestHelper.HttpGet("https://api.douban.com/v2/fm/playlist", args, addHeader);
            if (!json.IsNullorEmpty())
            {
                return JsonConvert.DeserializeObject<playlist>(json);
            }
            return null;
        }
    }
}
