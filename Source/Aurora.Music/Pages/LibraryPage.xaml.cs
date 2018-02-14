// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Aurora.Music.Core;
using Aurora.Music.Core.Models;
using Aurora.Music.Core.Storage;
using Aurora.Music.ViewModels;
using Aurora.Shared.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Aurora.Music.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class LibraryPage : Page, Controls.IRequestGoBack
    {
        public static LibraryPage Current;

        internal ObservableCollection<CategoryListItem> CategoryList;
        private List<PlayList> playlists;

        public LibraryPage()
        {
            this.InitializeComponent();
            Current = this;

            MainPageViewModel.Current.NeedShowTitle = false;
            MainPageViewModel.Current.LeftTopColor = Resources["SystemControlForegroundBaseHighBrush"] as SolidColorBrush;

            CategoryList = new ObservableCollection<CategoryListItem>() {
                new CategoryListItem
                {
                    Title = Consts.Localizer.GetString("SongsText"),
                    HeroImages = new List<ImageSource>() { new BitmapImage(new Uri("ms-appx:///Assets/Images/songs.png")) },
                    NavigatType = typeof(SongsPage)
                },
                new CategoryListItem
                {
                    Title = Consts.Localizer.GetString("AlbumsText"),
                    HeroImages = new List<ImageSource>() { new BitmapImage(new Uri("ms-appx:///Assets/Images/albums.png")) },
                    NavigatType = typeof(AlbumsPage)
                },
                new CategoryListItem
                {
                    Title = Consts.Localizer.GetString("ArtistsText"),
                    HeroImages = new List<ImageSource>() { new BitmapImage(new Uri("ms-appx:///Assets/Images/artists.png")) },
                    NavigatType = typeof(ArtistsPage)
                }
            };

            Task.Run(async () =>
            {
                playlists = await SQLOperator.Current().GetPlayListBriefAsync();
                var podcasts = await SQLOperator.Current().GetPodcastListBriefAsync();
                await Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
                {
                    foreach (var playlist in playlists)
                    {
                        CategoryList.Add(new CategoryListItem
                        {
                            Title = playlist.Title,
                            HeroImages = playlist.HeroArtworks == null ? null : Array.ConvertAll(playlist.HeroArtworks, x => (ImageSource)new BitmapImage(new Uri(x.IsNullorEmpty() ? Consts.BlackPlaceholder : x))).ToList(),
                            NavigatType = typeof(PlayListPage)
                        });
                    }
                    foreach (var podcast in podcasts)
                    {
                        CategoryList.Add(new CategoryListItem
                        {
                            Title = podcast.Title,
                            HeroImages = podcast.HeroArtworks == null ? null : new ImageSource[] { new BitmapImage(new Uri(podcast.HeroArtworks)) },
                            NavigatType = typeof(PodcastPage),
                            ID = podcast.ID
                        });
                    }
                    var item = CategoryList.FirstOrDefault(x => x.Title == Settings.Current.CategoryLastClicked);
                    if (item != default(CategoryListItem))
                    {
                        item.IsCurrent = true;
                    }
                    else
                    {
                        CategoryList[0].IsCurrent = true;
                    }
                    Category.SelectionChanged += Category_SelectionChanged;
                    Category.SelectedItem = item ?? CategoryList[0];
                });
            });
        }

        private void Category_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = (Category.SelectedItem as CategoryListItem);
            if (item == null)
            {
                item = CategoryList[0];
            }
            if (item.NavigatType == typeof(PlayListPage))
            {
                Navigate(item.NavigatType, playlists.Find(x => x.Title == (item.Title)));
            }
            else if (item.NavigatType == typeof(PodcastPage))
            {
                Navigate(item.NavigatType, item.ID);
            }
            else
            {
                Navigate(item.NavigatType);
            }

            Settings.Current.CategoryLastClicked = item.Title;
            Settings.Current.Save();

            foreach (var a in CategoryList)
            {
                a.IsCurrent = false;
            }
            item.IsCurrent = true;
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
        }

        public void RequestGoBack()
        {
            if (MainFrame.CanGoBack)
            {
                if (MainFrame.Content is Controls.IRequestGoBack g)
                {
                    g.RequestGoBack();
                }
                else
                {
                    MainFrame.GoBack();
                }

            }
            else
            {
                MainPage.Current.GoBack();
            }
        }

        internal void Navigate(Type type, object parameter)
        {
            MainFrame.Navigate(type, parameter);
        }

        internal void Navigate(Type type)
        {
            MainFrame.Navigate(type);
        }

        internal void GoBack()
        {
            if (MainFrame.CanGoBack)
            {
                MainFrame.GoBack();
                RefreshPaneCurrent();
            }
            else
            {
                MainPage.Current.GoBack();
            }
        }

        private void RefreshPaneCurrent()
        {
            var item = CategoryList.FirstOrDefault(a => a.NavigatType == MainFrame.Content.GetType()) ?? CategoryList[0];
            foreach (var a in CategoryList)
            {
                a.IsCurrent = false;
            }
            item.IsCurrent = true;
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            MainFrame.Content = null;
            Category.SelectionChanged -= Category_SelectionChanged;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }

        private void Grid_DragOver(object sender, DragEventArgs e)
        {

        }

        private void Grid_Drop(object sender, DragEventArgs e)
        {

        }
    }
}
