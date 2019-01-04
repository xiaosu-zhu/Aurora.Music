// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Aurora.Music.Controls;
using Aurora.Music.Core;
using Aurora.Music.Core.Models;
using Aurora.Music.Core.Tools;
using Aurora.Shared.Extensions;
using Aurora.Shared.MVVM;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.StartScreen;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;

namespace Aurora.Music.ViewModels
{
    internal class PodcastPageViewModel : ViewModelBase
    {

        private ObservableCollection<SongViewModel> songsList;
        public ObservableCollection<SongViewModel> SongsList
        {
            get { return songsList; }
            set { SetProperty(ref songsList, value); }
        }

        public bool NightModeEnabled { get; set; } = Settings.Current.NightMode;

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
                    await MainPageViewModel.Current.InstantPlayAsync(Model);
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
                    MainPage.Current.PopMessage(Model.Subscribed ? Consts.Localizer.GetString("SubscribeText") : Consts.Localizer.GetString("UnSubscribeText"));
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

        public string PinnedtoGlyph(bool b)
        {
            return b ? "\uE196" : "\uE141";
        }
        public string PinnedtoText(bool b)
        {
            return b ? Consts.Localizer.GetString("UnPinText") : Consts.Localizer.GetString("PinText");
        }

        public DelegateCommand PintoStart
        {
            get => new DelegateCommand(async () =>
            {
                // Construct a unique tile ID, which you will need to use later for updating the tile
                var tileId = $"podcast{Model.ID}";
                if (SecondaryTile.Exists(tileId))
                {
                    // Initialize a secondary tile with the same tile ID you want removed
                    var toBeDeleted = new SecondaryTile(tileId);

                    // And then unpin the tile
                    await toBeDeleted.RequestDeleteAsync();
                    IsPinned = SecondaryTile.Exists(tileId);
                }
                else
                {
                    // Use a display name you like
                    var displayName = Model.Title;

                    // Provide all the required info in arguments so that when user
                    // clicks your tile, you can navigate them to the correct content
                    var arguments = $"as-music:///library/podcast/id/{Model.ID}";

                    // Initialize the tile with required arguments
                    var tile = new SecondaryTile
                    {
                        Arguments = arguments,
                        TileId = tileId,
                        DisplayName = displayName
                    };
                    tile.VisualElements.Square150x150Logo = new Uri("ms-appx:///Assets/Square150x150Logo.png");
                    // Enable wide and large tile sizes
                    tile.VisualElements.Wide310x150Logo = new Uri("ms-appx:///Assets/Wide310x150Logo.png");
                    tile.VisualElements.Square310x310Logo = new Uri("ms-appx:///Assets/LargeTile.png");

                    // Add a small size logo for better looking small tile
                    tile.VisualElements.Square71x71Logo = new Uri("ms-appx:///Assets/SmallTile.png");

                    // Add a unique corner logo for the secondary tile
                    tile.VisualElements.Square44x44Logo = new Uri("ms-appx:///Assets/Square44x44Logo.png");

                    tile.VisualElements.ShowNameOnSquare150x150Logo = true;
                    tile.VisualElements.ShowNameOnWide310x150Logo = true;
                    tile.VisualElements.ShowNameOnSquare310x310Logo = true;

                    // Pin the tile
                    await tile.RequestCreateAsync();

                    await UpdateTile();
                }
            });
        }

        private bool isPinned;
        public bool IsPinned
        {
            get { return isPinned; }
            set { SetProperty(ref isPinned, value); }
        }

        public async Task UpdateTile()
        {
            var tileId = $"podcast{Model.ID}";
            // Check if the secondary tile is pinned
            isPinned = SecondaryTile.Exists(tileId);
            if (isPinned)
            {
                Tile.UpdatePodcast(tileId, Model);
            }
            await CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            {
                RaisePropertyChanged("IsPinned");
            });
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

            await CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
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
            await UpdateTile();
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
                await MainPageViewModel.Current.InstantPlayAsync(k, k.IndexOf(s));
            }
            else
            {
                await MainPageViewModel.Current.InstantPlayAsync(Model.GetRange(i, 20), 0);
            }
        }
    }
}
