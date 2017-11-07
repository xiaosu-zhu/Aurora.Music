using Aurora.Music.Core;
using Aurora.Music.Core.Storage;
using Aurora.Shared.Extensions;
using Aurora.Shared.Helpers;
using Aurora.Shared.MVVM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI;

namespace Aurora.Music.ViewModels
{
    class HomePageViewModel : ViewModelBase
    {
        private Color leftGradient = Shared.Palette.GetRandom();
        public Color LeftGradient
        {
            get { return leftGradient; }
            set { SetProperty(ref leftGradient, value); }
        }

        private Color rightGradient = Shared.Palette.GetRandom();
        public Color RightGradient
        {
            get { return rightGradient; }
            set { SetProperty(ref rightGradient, value); }
        }

        private string welcomeTitle = "Hi.";
        public string WelcomeTitle
        {
            get { return welcomeTitle; }
            set { SetProperty(ref welcomeTitle, value); }
        }

        public ObservableCollection<GenericMusicItemViewModel> FavList { get; set; } = new ObservableCollection<GenericMusicItemViewModel>();
        public ObservableCollection<GenericMusicItemViewModel> RandomList { get; set; } = new ObservableCollection<GenericMusicItemViewModel>();

        public ObservableCollection<GenericMusicItemViewModel> HeroList { get; set; } = new ObservableCollection<GenericMusicItemViewModel>();

        public async Task Load()
        {
            var n = await SystemInfoHelper.GetCurrentUserNameAsync();
            var hero = await FileReader.GetHeroListAsync();
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                if (!n.IsNullorWhiteSpace())
                    WelcomeTitle = $"Hi, {n}.";
                foreach (var item in hero)
                {
                    if (item.IsNullorEmpty())
                        continue;
                    var pic = (from i in item where !i.PicturePath.IsNullorEmpty() select i.PicturePath into p orderby Tools.Random.Next() select p).FirstOrDefault();
                    HeroList.Add(new GenericMusicItemViewModel()
                    {
                        IDs = item.Select(x => x.IDs).Aggregate((a, b) =>
                        {
                            return a.Concat(b).ToArray();
                        }),
                        Title = item.Key,
                        Artwork = pic.IsNullorEmpty() ? null : new Uri(pic)
                    });
                }
            });

            var ran = await FileReader.GetRandomListAsync();
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                foreach (var item in ran)
                {
                    RandomList.Add(new GenericMusicItemViewModel(item));
                }
            });

            var fav = await FileReader.GetFavListAsync();
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                foreach (var item in fav)
                {
                    FavList.Add(new GenericMusicItemViewModel(item));
                }
            });

        }

    }
}
