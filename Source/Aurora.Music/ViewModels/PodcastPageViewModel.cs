// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Aurora.Music.Controls;
using Aurora.Music.Core;
using Aurora.Music.Core.Models;
using Aurora.Shared.Extensions;
using Aurora.Shared.MVVM;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using TagLib.Id3v2;
using Windows.ApplicationModel.Core;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;

namespace Aurora.Music.ViewModels
{
    class PodcastPageViewModel : ViewModelBase
    {

        private ObservableCollection<SongViewModel> songsList;
        public ObservableCollection<SongViewModel> SongsList
        {
            get { return songsList; }
            set { SetProperty(ref songsList, value); }
        }

        private Uri heroImage;
        public Uri HeroImage
        {
            get { return heroImage; }
            set { SetProperty(ref heroImage, value); }
        }

        private string desc;
        public string Description
        {
            get { return desc; }
            set { SetProperty(ref desc, value); }
        }

        private string lastUpdate;
        public string LastUpdate
        {
            get { return lastUpdate; }
            set { SetProperty(ref lastUpdate, value); }
        }

        private string title;
        public string Title
        {
            get { return title; }
            set { SetProperty(ref title, value); }
        }

        public PodcastPageViewModel()
        {
            SongsList = new ObservableCollection<SongViewModel>();
        }

        public DelegateCommand PlayAll
        {
            get
            {
                return new DelegateCommand(async () =>
                {
                    var s = Model[0];
                    if (s.IsVideo)
                    {
                        await CoreApplication.CreateNewView().Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            var frame = new Windows.UI.Xaml.Controls.Frame();
                            videoWindowID = ApplicationView.GetForCurrentView().Id;
                            frame.Navigate(typeof(VideoPodcast), s.FilePath);
                            Window.Current.Content = frame;
                            Window.Current.Activate();
                            ApplicationView.GetForCurrentView().Title = s.Title;
                            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
                            var titleBar = ApplicationView.GetForCurrentView().TitleBar;
                            titleBar.ButtonBackgroundColor = Colors.Transparent;
                            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
                            titleBar.ButtonHoverBackgroundColor = Color.FromArgb(0x33, 0x00, 0x00, 0x00);
                            titleBar.ButtonForegroundColor = Colors.Black;
                            titleBar.ButtonHoverForegroundColor = Colors.White;
                            titleBar.ButtonInactiveForegroundColor = Colors.Gray;
                        });
                        bool viewShown = await ApplicationViewSwitcher.TryShowAsStandaloneAsync(videoWindowID);
                        return;
                    }
                    await MainPageViewModel.Current.InstantPlay(Model);
                });
            }
        }

        private int videoWindowID;

        private bool isSubscribe;
        public bool IsSubscribe
        {
            get { return isSubscribe; }
            set { SetProperty(ref isSubscribe, value); }
        }

        private bool sortRevert;
        public bool SortRevert
        {
            get { return sortRevert; }
            set { SetProperty(ref sortRevert, value); }
        }

        public string SubscribeGlyph(bool b)
        {
            return !b ? "\uE8D9" : "\uE735";
        }

        public string RevertText(bool b)
        {
            return Consts.Localizer.GetString(b ? "OlderTop" : "NewerTop");
        }

        public string SubscribeText(bool b)
        {
            return Consts.Localizer.GetString(!b ? "SubscribeAction" : "UnSubscribeAction");
        }

        public DelegateCommand ToggleSubscribe
        {
            get
            {
                return new DelegateCommand(async () =>
                {
                    Model.Subscribed = !Model.Subscribed;
                    await Model.SaveAsync();
                    IsSubscribe = Model.Subscribed;
                    MainPage.Current.PopMessage(Model.Subscribed ? "Subscribed" : "Un-Subscribed");
                });
            }
        }

        public DelegateCommand ToggleSortRevert
        {
            get
            {
                return new DelegateCommand(async () =>
                {
                    Model.SortRevert = !Model.SortRevert;
                    await Model.SaveAsync();
                    SortRevert = Model.SortRevert;
                    var list = SongsList.ToList();
                    SongsList.Clear();
                    list.Reverse();
                    foreach (var item in list)
                    {
                        SongsList.Add(item);
                    }
                });
            }
        }

        public DelegateCommand Refresh
        {
            get
            {
                return new DelegateCommand(async () =>
                {
                    await Model.Refresh();
                    SongsList.Clear();
                    uint i = 0;
                    foreach (var item in Model)
                    {
                        SongsList.Add(new SongViewModel(item)
                        {
                            Index = ++i
                        });
                    }
                    LastUpdate = Model.LastUpdate.PubDatetoString($"'{Consts.Today}'", "ddd", "M/dd ddd", "yy/M/dd", Consts.Next, Consts.Last);
                    Description = Model.Description;
                    Title = Model.Title;
                    HeroImage = new Uri(Model.HeroArtworks[0]);
                    IsSubscribe = Model.Subscribed;
                    SortRevert = Model.SortRevert;
                    MainPage.Current.PopMessage("Refreshed");
                });
            }
        }

        public Podcast Model { get; private set; }

        public async Task Init(int ID)
        {
            Model = await Podcast.ReadFromLocalAsync(ID);
            if (Model == null)
            {
                return;
            }

            await CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                SongsList.Clear();
                uint i = 0;
                foreach (var item in Model)
                {
                    SongsList.Add(new SongViewModel(item)
                    {
                        Index = ++i
                    });
                }
                LastUpdate = Model.LastUpdate.PubDatetoString($"'{Consts.Today}'", "ddd", "M/dd ddd", "yy/M/dd", Consts.Next, Consts.Last);
                Description = Model.Description;
                IsSubscribe = Model.Subscribed;
                SortRevert = Model.SortRevert;
                Title = Model.Title;
                HeroImage = new Uri(Model.HeroArtworks[0]);
            });
        }

        internal async Task PlayAt(SongViewModel songViewModel)
        {
            var i = (int)songViewModel.Index - 1;
            var s = Model[i];
            if (s.IsVideo)
            {
                await CoreApplication.CreateNewView().Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    var frame = new Windows.UI.Xaml.Controls.Frame();
                    videoWindowID = ApplicationView.GetForCurrentView().Id;
                    frame.Navigate(typeof(VideoPodcast), s.FilePath);
                    Window.Current.Content = frame;
                    Window.Current.Activate();
                    ApplicationView.GetForCurrentView().Title = s.Title;
                    CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
                    var titleBar = ApplicationView.GetForCurrentView().TitleBar;
                    titleBar.ButtonBackgroundColor = Colors.Transparent;
                    titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
                    titleBar.ButtonHoverBackgroundColor = Color.FromArgb(0x33, 0x00, 0x00, 0x00);
                    titleBar.ButtonForegroundColor = Colors.Black;
                    titleBar.ButtonHoverForegroundColor = Colors.White;
                    titleBar.ButtonInactiveForegroundColor = Colors.Gray;
                });
                bool viewShown = await ApplicationViewSwitcher.TryShowAsStandaloneAsync(videoWindowID);
                return;
            }
            if (Model.Count < i + 20)
            {
                var k = Model.Count < 20 ? Model.ToList() : Model.GetRange(Model.Count - 20, 20);
                await MainPageViewModel.Current.InstantPlay(k, k.IndexOf(s));
            }
            else
            {
                await MainPageViewModel.Current.InstantPlay(Model.GetRange(i, 20), 0);
            }
        }
    }
}
