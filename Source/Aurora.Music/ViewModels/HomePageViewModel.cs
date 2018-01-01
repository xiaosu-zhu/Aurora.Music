using Aurora.Music.Core;
using Aurora.Music.Core.Models;
using Aurora.Music.Core.Storage;
using Aurora.Shared;
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
using Windows.Graphics.Imaging;
using Windows.UI;

namespace Aurora.Music.ViewModels
{
    class HomePageViewModel : ViewModelBase
    {
        private Color leftGradient;
        public Color LeftGradient
        {
            get { return leftGradient; }
            set { SetProperty(ref leftGradient, value); }
        }

        private Color rightGradient;
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

        public DelegateCommand PlayRandom
        {
            get => new DelegateCommand(async () =>
            {
                var list = new List<Song>();
                foreach (var item in RandomList)
                {
                    list.AddRange(await item.GetSongsAsync());
                }
                await MainPageViewModel.Current.InstantPlay(list);
            });
        }

        public DelegateCommand ReRandom
        {
            get => new DelegateCommand(async () =>
            {
                var ran = await FileReader.GetRandomListAsync();
                RandomList.Clear();
                foreach (var item in ran)
                {
                    RandomList.Add(new GenericMusicItemViewModel(item));
                }
            });
        }

        public HomePageViewModel()
        {
            var fes = Tools.IsFestival(DateTime.Today);
            switch (fes)
            {
                case Festival.None:
                    RightGradient = Palette.GetRandom();
                    LeftGradient = Palette.GetRandom();
                    WelcomeTitle = $"Good {DateTime.Now.GetHourString()}";
                    break;
                case Festival.Valentine:
                    RightGradient = Palette.Pink;
                    LeftGradient = Palette.PinkRed;
                    WelcomeTitle = $"Happy Valentine's Day";
                    break;
                case Festival.Halloween:
                    RightGradient = Palette.DarkBlueGray;
                    LeftGradient = Palette.PurpleGray;
                    WelcomeTitle = $"Trick or Treat?";
                    break;
                case Festival.Xmas:
                    RightGradient = Palette.VibrantRed;
                    LeftGradient = Palette.Green;
                    WelcomeTitle = $"Merry Christmas!";
                    break;
                case Festival.Fool:
                    RightGradient = Palette.GetRandom();
                    LeftGradient = Palette.GetRandom();
                    WelcomeTitle = $"Good {DateTime.Now.GetHourString()}";
                    break;
                default:
                    RightGradient = Palette.GetRandom();
                    LeftGradient = Palette.GetRandom();
                    WelcomeTitle = $"Good {DateTime.Now.GetHourString()}";
                    break;
            }
        }

        public ObservableCollection<GenericMusicItemViewModel> FavList { get; set; } = new ObservableCollection<GenericMusicItemViewModel>();
        public ObservableCollection<GenericMusicItemViewModel> RandomList { get; set; } = new ObservableCollection<GenericMusicItemViewModel>();

        public ObservableCollection<GenericMusicItemViewModel> HeroList { get; set; } = new ObservableCollection<GenericMusicItemViewModel>();

        public async Task Load()
        {
            var hero = await FileReader.GetHeroListAsync();
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, async () =>
            {
                HeroList.Clear();
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
                        Artwork = pic.IsNullorEmpty() ? null : new Uri(pic),
                        MainColor = pic.IsNullorEmpty() ? Palette.Blue : await ImagingHelper.GetMainColor(pic.IsNullorEmpty() ? null : new Uri(pic))
                    });
                }
            });

            var ran = await FileReader.GetRandomListAsync();
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                RandomList.Clear();
                foreach (var item in ran)
                {
                    RandomList.Add(new GenericMusicItemViewModel(item));
                }
            });

            var fav = await FileReader.GetFavListAsync();
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                FavList.Clear();
                foreach (var item in fav)
                {
                    FavList.Add(new GenericMusicItemViewModel(item));
                }
            });

        }

    }
}
