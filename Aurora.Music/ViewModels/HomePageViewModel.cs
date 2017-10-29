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

        public ObservableCollection<GenericMusicItemViewModel> HeroList { get; set; } = new ObservableCollection<GenericMusicItemViewModel>();

        public async Task Load()
        {
            SQLOperator.Current();
            var n = await SystemInfoHelper.GetCurrentUserNameAsync();
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                if (!n.IsNullorWhiteSpace())
                    WelcomeTitle = $"Hi, {n}.";
                FavList.Add(new GenericMusicItemViewModel
                {
                    Title = "Octopus Garden",
                    Description = "3 mins, 3 secs",
                    Addtional = "Favorite",
                });
                FavList.Add(new GenericMusicItemViewModel
                {
                    Title = "LOVE",
                    Description = "15 Songs",
                    Addtional = "Played 35 times",
                });
                FavList.Add(new GenericMusicItemViewModel
                {
                    Title = "Let It Be",
                    Description = "5 mins, 13 secs",
                    Addtional = "The Beatles",
                });
                FavList.Add(new GenericMusicItemViewModel
                {
                    Title = "Love Me Do",
                    Description = "2 mins, 24 secs",
                    Addtional = "The Beatles",
                });
                FavList.Add(new GenericMusicItemViewModel
                {
                    Title = "The Beatles",
                    Description = "30 Songs",
                    Addtional = "The Beatles",
                });
                FavList.Add(new GenericMusicItemViewModel
                {
                    Title = "Octopus Garden",
                    Description = "3 mins, 3 secs",
                    Addtional = "Favorite",
                });
                HeroList.Add(new GenericMusicItemViewModel
                {
                    Title = "Afternoon Pick",
                    Description = "3 mins, 3 secs",
                    Addtional = "Afternoon Picks",
                });
                HeroList.Add(new GenericMusicItemViewModel
                {
                    Title = "Shuffle All",
                    Description = "15 Songs",
                    Addtional = "Played 35 times",
                });
                HeroList.Add(new GenericMusicItemViewModel
                {
                    Title = "Favorties Picks",
                    Description = "5 mins, 13 secs",
                    Addtional = "The Beatles",
                });
                HeroList.Add(new GenericMusicItemViewModel
                {
                    Title = "Friday Picks",
                    Description = "2 mins, 24 secs",
                    Addtional = "The Beatles",
                });
                HeroList.Add(new GenericMusicItemViewModel
                {
                    Title = "Recently Played",
                    Description = "30 Songs",
                    Addtional = "The Beatles",
                });
                HeroList.Add(new GenericMusicItemViewModel
                {
                    Title = "Classic Picks",
                    Description = "3 mins, 3 secs",
                    Addtional = "Favorite",
                });
                FavList.Add(new GenericMusicItemViewModel
                {
                    Title = "LOVE",
                    Description = "15 Songs",
                    Addtional = "Played 35 times",
                });
                FavList.Add(new GenericMusicItemViewModel
                {
                    Title = "Let It Be",
                    Description = "5 mins, 13 secs",
                    Addtional = "The Beatles",
                });
                FavList.Add(new GenericMusicItemViewModel
                {
                    Title = "Love Me Do",
                    Description = "2 mins, 24 secs",
                    Addtional = "The Beatles",
                });
                FavList.Add(new GenericMusicItemViewModel
                {
                    Title = "The Beatles",
                    Description = "30 Songs",
                    Addtional = "The Beatles",
                });
            });
        }

    }
}
