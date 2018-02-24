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
        private Task launchTask;
        private List<PlayList> playlists;

        public LibraryPage()
        {
            this.InitializeComponent();
            Current = this;
            MainPageViewModel.Current.NeedShowTitle = Window.Current.Bounds.Width > 640;
            MainPageViewModel.Current.Title = Consts.Localizer.GetString("LibraryText");
            MainPageViewModel.Current.LeftTopColor = Resources["SystemControlForegroundBaseHighBrush"] as SolidColorBrush;
            CategoryList = new ObservableCollection<CategoryListItem>();
        }

        internal void RemovePlayList(PlayList model)
        {
            var i = Category.SelectedIndex;
            CategoryList.Remove(CategoryList.First(a => a.ID == model.ID && a.NavigatType == typeof(PlayListPage)));
            playlists.Remove(model);
        }

        internal async Task AddPlayList(PlayListViewModel p)
        {
            Category.SelectionChanged -= Category_SelectionChanged;


            CategoryList.Clear();
            CategoryList.Add(new CategoryListItem
            {
                Title = Consts.Localizer.GetString("SongsText"),
                Glyph = "\uE189",
                NavigatType = typeof(SongsPage)
            });
            CategoryList.Add(new CategoryListItem
            {
                Title = Consts.Localizer.GetString("AlbumsText"),
                Glyph = "\uE93C",
                NavigatType = typeof(AlbumsPage)
            });
            CategoryList.Add(new CategoryListItem
            {
                Title = Consts.Localizer.GetString("ArtistsText"),
                Glyph = "\uE77B",
                NavigatType = typeof(ArtistsPage)
            });

            playlists = await SQLOperator.Current().GetPlayListBriefAsync();
            var podcasts = await SQLOperator.Current().GetPodcastListBriefAsync();

            foreach (var playlist in playlists)
            {
                CategoryList.Add(new CategoryListItem
                {
                    Title = playlist.ToString(),
                    Glyph = "\uE142",
                    NavigatType = typeof(PlayListPage)
                });
            }
            foreach (var podcast in podcasts)
            {
                CategoryList.Add(new CategoryListItem
                {
                    Title = podcast.Title,
                    Glyph = "\uE95A",
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
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            CategoryList.Clear();
            CategoryList.Add(new CategoryListItem
            {
                Title = Consts.Localizer.GetString("SongsText"),
                Glyph = "\uE189",
                NavigatType = typeof(SongsPage)
            });
            CategoryList.Add(new CategoryListItem
            {
                Title = Consts.Localizer.GetString("AlbumsText"),
                Glyph = "\uE93C",
                NavigatType = typeof(AlbumsPage)
            });
            CategoryList.Add(new CategoryListItem
            {
                Title = Consts.Localizer.GetString("ArtistsText"),
                Glyph = "\uE77B",
                NavigatType = typeof(ArtistsPage)
            });

            playlists = await SQLOperator.Current().GetPlayListBriefAsync();
            var podcasts = await SQLOperator.Current().GetPodcastListBriefAsync();
            foreach (var playlist in playlists)
            {
                CategoryList.Add(new CategoryListItem
                {
                    Title = playlist.ToString(),
                    Glyph = "\uE142",
                    NavigatType = typeof(PlayListPage),
                    ID = playlist.ID
                });
            }
            foreach (var podcast in podcasts)
            {
                CategoryList.Add(new CategoryListItem
                {
                    Title = podcast.Title,
                    Glyph = "\uE95A",
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

            if (e.Parameter is ValueTuple<Type, int, string> m)
            {
                if (m.Item2 != -1)
                {
                    try
                    {
                        var i = CategoryList.IndexOf(CategoryList.First(a => a.ID == m.Item2 && a.NavigatType == m.Item1));
                        if (i == -1)
                        {
                            Category.SelectedIndex = 0;
                        }
                        else
                        {
                            Category.SelectedIndex = i;
                        }
                    }
                    catch (Exception)
                    {
                        Category.SelectedIndex = 0;
                    }
                }
                else
                {
                    Category.SelectedIndex = CategoryList.IndexOf(CategoryList.First(a => a.NavigatType == m.Item1));
                }
            }
            else
            {
                Category.SelectedIndex = 0;
            }
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
                Navigate(item.NavigatType, playlists.Find(x => x.ID == (item.ID)));
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
            var d = e.GetDeferral();
            e.AcceptedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Link;
            e.Handled = true;
            d.Complete();
        }

        private void Grid_Drop(object sender, DragEventArgs e)
        {
            var d = e.GetDeferral();
            e.AcceptedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Link;
            e.Handled = true;
            d.Complete();
        }

        internal void ShowPodcast(string iD)
        {
            if (int.TryParse(iD, out int i))
                Category.SelectedIndex = CategoryList.IndexOf(CategoryList.First(a => a.ID == i && a.NavigatType == typeof(PodcastPage)));
        }

        private void VisualStateGroup_CurrentStateChanged(object sender, VisualStateChangedEventArgs e)
        {
            MainPageViewModel.Current.NeedShowTitle = e.NewState.Name != "Narrow";
        }
    }
}
