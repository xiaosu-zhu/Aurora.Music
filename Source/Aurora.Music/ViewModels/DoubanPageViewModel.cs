using Aurora.Music.Core.Models;
using Aurora.Music.Core.Models.Json;
using Aurora.Shared.Helpers;
using Aurora.Shared.MVVM;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation.Collections;

namespace Aurora.Music.ViewModels
{
    class DoubanPageViewModel : ViewModelBase
    {
        public ObservableCollection<ChannelGroup> Channels { get; set; }

        public DoubanPageViewModel()
        {
            Channels = new ObservableCollection<ChannelGroup>();

            Task.Run(async () =>
            {
                await Init();
            });
        }

        public async Task Init()
        {
            var result = await ApiRequestHelper.HttpGet("https://api.douban.com/v2/fm/app_channels?alt=json&apikey=02646d3fb69a52ff072d47bf23cef8fd&app_name=radio_iphone&client=s%3Amobile%7Cy%3AiOS%2010.2%7Cf%3A115%7Cd%3Ab88146214e19b8a8244c9bc0e2789da68955234d%7Ce%3AiPhone7%2C1%7Cm%3Aappstore&douban_udid=b635779c65b816b13b330b68921c0f8edc049590&icon_cate=xlarge&udid=b88146214e19b8a8244c9bc0e2789da68955234d&version=115");
            var douban = JsonConvert.DeserializeObject<Douban>(result);

            await CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                foreach (var item in douban.groups)
                {
                    var g = new ChannelGroup()
                    {
                        Name = item.group_name,
                        ID = item.group_id,
                    };
                    foreach (var c in item.chls)
                    {
                        g.Add(new ChannelViewModel()
                        {
                            Cover = new Uri(c.cover),
                            Description = c.intro,
                            Name = c.name,
                            ID = c.id,
                        });
                    }
                    Channels.Add(g);
                }
            });
            await Login("pkzxs1232125@126.com", "ZXSzxs1232125");
        }

        public async Task Login(string username, string password)
        {
            if (Settings.Current.DoubanLogin.AddSeconds(Settings.Current.DoubanExpireTime) < DateTime.Now.AddDays(1))
            {
                // Login and get access_token
                var dix = new Dictionary<string, string>
                {
                    ["apikey"] = "02646d3fb69a52ff072d47bf23cef8fd",
                    ["client_id"] = "02646d3fb69a52ff072d47bf23cef8fd",
                    ["client_secret"] = "cde5d61429abcd7c",
                    ["udid"] = "b88146214e19b8a8244c9bc0e2789da68955234d",
                    ["douban_udid"] = "b635779c65b816b13b330b68921c0f8edc049590",
                    ["device_id"] = "b88146214e19b8a8244c9bc0e2789da68955234d",
                    ["grant_type"] = "password",
                    ["redirect_uri"] = "http://www.douban.com/mobile/fm",
                    ["username"] = username,
                    ["password"] = password
                };

                var result = await ApiRequestHelper.HttpPostForm("https://www.douban.com/service/auth2/token", dix);
            }
            else
            {
                // do nothing
            }
        }
    }
}
