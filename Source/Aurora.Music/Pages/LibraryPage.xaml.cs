// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

using Aurora.Music.Core;
using Aurora.Music.Core.Models;
using Aurora.Music.Core.Storage;
using Aurora.Music.ViewModels;
using Aurora.Shared.Helpers;

using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Aurora.Music.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    [UriActivate("library", Usage = ActivateUsage.Navigation)]
    public sealed partial class LibraryPage : Page, Controls.IRequestGoBack
    {
        public static LibraryPage Current;

        internal ObservableCollection<CategoryListItem> CategoryList;
        private List<PlayList> playlists;

        public LibraryPage()
        {
            InitializeComponent();
            Current = this;
            MainPageViewModel.Current.NeedShowTitle = false;
            CategoryList = new ObservableCollection<CategoryListItem>();
        }

        private void CategoryList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // Update Index in order to handle collection add/remove;
            var i = 0;
            var list = CategoryList.ToList();
            list.Sort((a, b) => a.Index.CompareTo(b.Index));
            foreach (var item in list)
            {
                item.Index = i++;
            }
        }

        internal void RemovePlayList(PlayList model)
        {
            CategoryList.Remove(CategoryList.First(a => a.ID == model.ID && a.NavigatType == typeof(PlayListPage)));
            playlists.Remove(model);
        }
        internal void RemoveStoragePlayList(string title)
        {
            CategoryList.Remove(CategoryList.First(a => a.Title == title && a.NavigatType == typeof(StoragePlaylistPage)));
        }

        internal async Task AddPlayList(PlayListViewModel p)
        {

            CategoryList.CollectionChanged -= CategoryList_CollectionChanged;
            Category.SelectionChanged -= Category_SelectionChanged;
            await InitCategoryList();
            var item = CategoryList.FirstOrDefault(x => x.Title == Settings.Current.CategoryLastClicked);
            Category.SelectionChanged += Category_SelectionChanged;
            if (item != default(CategoryListItem))
            {
                Category.SelectedIndex = CategoryList.IndexOf(item);
            }
            else
            {
                Category.SelectedIndex = 0;
            }

            CategoryList.CollectionChanged += CategoryList_CollectionChanged;
        }

        private async Task InitCategoryList()
        {
            CategoryList.Clear();

            var list = new List<CategoryListItem>();
            var i = 3;

            list.Add(new CategoryListItem
            {
                Title = Consts.Localizer.GetString("SongsText"),
                Glyph = "\uE189",
                NavigatType = typeof(SongsPage),
                Index = 0,
                Desc = SmartFormat.Smart.Format(Consts.Localizer.GetString("SmartSongs"), await FileReader.CountAsync<Song>())
            });
            list.Add(new CategoryListItem
            {
                Title = Consts.Localizer.GetString("AlbumsText"),
                Glyph = "\uE93C",
                NavigatType = typeof(AlbumsPage),
                Index = 1,
                Desc = SmartFormat.Smart.Format(Consts.Localizer.GetString("SmartAlbums"), await FileReader.CountAsync<Album>())
            });
            list.Add(new CategoryListItem
            {
                Title = Consts.Localizer.GetString("ArtistsText"),
                Glyph = "\uE77B",
                NavigatType = typeof(ArtistsPage),
                Index = 2,
                Desc = SmartFormat.Smart.Format(Consts.Localizer.GetString("SmartArtists"), (await SQLOperator.Current().GetArtistsAsync()).Count)
            });

            playlists = await SQLOperator.Current().GetPlayListBriefAsync();
            var podcasts = await SQLOperator.Current().GetPodcastListBriefAsync();

            var playlistFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Playlist", CreationCollisionOption.OpenIfExists);
            var files = await playlistFolder.GetFilesAsync();

            foreach (var playlist in playlists)
            {
                list.Add(new CategoryListItem
                {
                    Title = playlist.Title,
                    Glyph = "\uE142",
                    NavigatType = typeof(PlayListPage),
                    ID = playlist.ID,
                    Index = i++,
                    Desc = playlist.Description
                });
            }
            foreach (var item in files)
            {
                list.Add(new CategoryListItem
                {
                    Title = item.DisplayName,
                    Parameter = item.Name,
                    Glyph = "\uE142",
                    ID = -1,
                    NavigatType = typeof(StoragePlaylistPage),
                    Index = i++,
                    Desc = item.Path
                });
            }
            foreach (var podcast in podcasts)
            {
                list.Add(new CategoryListItem
                {
                    Title = podcast.Title,
                    Glyph = "\uE95A",
                    NavigatType = typeof(PodcastPage),
                    ID = podcast.ID,
                    Index = i++,
                    Desc = podcast.Description
                });
            }

            ReorderList(ref list);


            foreach (var item in list)
            {
                CategoryList.Add(item);
            }
        }

        private void ReorderList(ref List<CategoryListItem> list)
        {
            var index = Settings.LibraryIndex();

            if (index == null)
            {
                return;
            }

            var c = new List<CategoryListItem>();

            foreach (var i in index)
            {
                var item = list.Find(a => a.Index == i);
                if (item != null)
                {
                    list.Remove(item);
                    c.Add(item);
                }
            }

            c.AddRange(list);
            list = c;
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            await InitCategoryList();
            Category.SelectionChanged += Category_SelectionChanged;

            if (e.Parameter is ValueTuple<Type, int, string> m)
            {
                if (m.Item1 == null)
                {
                    Category.SelectedIndex = 0;
                }
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
                var item = CategoryList.FirstOrDefault(x => x.Title == Settings.Current.CategoryLastClicked);
                if (item != default(CategoryListItem))
                {
                    Category.SelectedIndex = CategoryList.IndexOf(item);
                }
                else
                {
                    Category.SelectedIndex = 0;
                }
            }



            CategoryList.CollectionChanged += CategoryList_CollectionChanged;
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
            else if (item.NavigatType == typeof(StoragePlaylistPage))
            {
                Navigate(item.NavigatType, item.Parameter);
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
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);

            CategoryList.CollectionChanged -= CategoryList_CollectionChanged;

            var dump = CategoryList.ToList().ConvertAll(a => a.Index);
            Task.Run(async () =>
            {
                await Settings.SaveLibraryIndex(dump);
            });
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
            }
            else
            {
                MainPage.Current.GoBack();
            }
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

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var filePicker = new Windows.Storage.Pickers.FileOpenPicker
            {
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.ComputerFolder
            };
            filePicker.FileTypeFilter.Add(".m3u");
            filePicker.FileTypeFilter.Add(".m3u8");
            filePicker.FileTypeFilter.Add(".wpl");
            filePicker.FileTypeFilter.Add(".zpl");

            var files = await filePicker.PickMultipleFilesAsync();

            var playlistFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Playlist", CreationCollisionOption.OpenIfExists);

            if (files != null && files.Count > 0)
            {
                var index = CategoryList.IndexOf(CategoryList.Last(a => a.NavigatType == typeof(PlayListPage)));
                foreach (var file in files)
                {
                    await file.CopyAsync(playlistFolder, file.Name, NameCollisionOption.ReplaceExisting);
                    CategoryList.Insert(index + 1, new CategoryListItem()
                    {
                        Title = file.DisplayName,
                        Parameter = file.Name,
                        ID = -1,
                        Glyph = "\uE142",
                        NavigatType = typeof(StoragePlaylistPage),
                    });
                }
            }
            else
            {
                return;
            }

            if (MainPageViewModel.Current != null)
            {
                var t = Task.Run(async () =>
                {
                    await MainPageViewModel.Current.FilesChangedAsync();
                });
            }
        }
    }
}
