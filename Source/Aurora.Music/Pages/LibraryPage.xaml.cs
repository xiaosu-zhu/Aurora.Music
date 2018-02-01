// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Aurora.Music.Core;
using Aurora.Music.Core.Models;
using Aurora.Music.Core.Storage;
using Aurora.Music.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
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

            MainPageViewModel.Current.Title = string.Empty;
            MainPageViewModel.Current.NeedShowTitle = true;
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
                await Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
                {
                    foreach (var playlist in playlists)
                    {
                        CategoryList.Add(new CategoryListItem
                        {
                            Title = playlist.Title,
                            HeroImages = playlist.HeroArtworks == null ? null : Array.ConvertAll(playlist.HeroArtworks, x => (ImageSource)new BitmapImage(new Uri(x))).ToList(),
                            NavigatType = typeof(PlayListPage)
                        });
                    }
                });
            });

            var item = CategoryList.FirstOrDefault(x => x.Title == Settings.Current.CategoryLastClicked);
            if (item != default(CategoryListItem))
            {
                item.IsCurrent = true;
                CategoryList.Remove(item);
                CategoryList.Insert(0, item);
            }
            else
            {
                CategoryList[0].IsCurrent = true;
            }

            if (CategoryList[0].NavigatType == typeof(PlayListPage))
            {
                Navigate(CategoryList[0].NavigatType, playlists.Find(x => x.Title == (CategoryList[0].Title)));
            }
            else
            {
                Navigate(CategoryList[0].NavigatType);
            }

            if (Window.Current.Bounds.Width <= 640)
            {
                MainPageViewModel.Current.NeedShowTitle = false;
            }
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
            for (int i = 0; i < CategoryList.Count; i++)
            {
                if (CategoryList[i].NavigatType == MainFrame.Content.GetType())
                {
                    if (i == 0)
                        return;
                    var item = CategoryList[i];
                    PrepareAnimationWithItem();
                    CompleteAnimationWithItems(item);
                    break;
                }
            }
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = (e.ClickedItem as CategoryListItem);
            if (item.NavigatType == typeof(PlayListPage))
            {
                Navigate(item.NavigatType, playlists.Find(x => x.Title == (item.Title)));
            }
            else
            {
                Navigate(item.NavigatType);
            }

            Settings.Current.CategoryLastClicked = (e.ClickedItem as CategoryListItem).Title;
            Settings.Current.Save();
            PrepareAnimationWithItem();
            CompleteAnimationWithItems(e.ClickedItem as CategoryListItem);
        }


        private async void CompleteAnimationWithItems(CategoryListItem item)
        {
            CategoryList.Remove(item);
            CategoryList.Insert(0, item);

            await Task.Delay(100);

            foreach (var cat in CategoryList)
            {
                try
                {
                    var ani = ConnectedAnimationService.GetForCurrentView().GetAnimation(cat.Title);
                    if (ani != null)
                    {
                        await Category.TryStartConnectedAnimationAsync(ani, cat, "Panel");
                    }
                }
                catch (Exception)
                {
                }
            }
            foreach (var cat in CategoryList)
            {
                cat.IsCurrent = false;
            }

            item.IsCurrent = true;

        }

        void PrepareAnimationWithItem()
        {
            foreach (var cat in CategoryList)
            {
                try
                {
                    Category.PrepareConnectedAnimation(cat.Title, cat, "Panel");
                }
                catch (Exception)
                {
                }
                cat.IsCurrent = false;
            }
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            MainFrame.Content = null;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }

        private void VisualStateGroup_CurrentStateChanged(object sender, VisualStateChangedEventArgs e)
        {
            if (e.NewState.Name == "Narrow")
            {
                MainPageViewModel.Current.NeedShowTitle = false;
            }
            else
            {
                MainPageViewModel.Current.NeedShowTitle = true;
            }
        }

        private void Grid_DragOver(object sender, DragEventArgs e)
        {

        }

        private void Grid_Drop(object sender, DragEventArgs e)
        {

        }
    }
}
