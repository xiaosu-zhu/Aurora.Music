using Aurora.Music.Core.Models;
using Aurora.Music.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Aurora.Music.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class LibraryPage : Page
    {
        public static LibraryPage Current;

        internal ObservableCollection<CategoryListItem> CategoryList;

        public LibraryPage()
        {
            this.InitializeComponent();
            Current = this;

            MainPageViewModel.Current.Title = "Library";
            MainPageViewModel.Current.NeedShowTitle = true;
            MainPageViewModel.Current.LeftTopColor = Resources["SystemControlForegroundBaseHighBrush"] as SolidColorBrush;

            var settings = Settings.Load();
            CategoryList = new ObservableCollection<CategoryListItem>() { new CategoryListItem { Title = "Songs", Index = new Uri("ms-appx:///Assets/Images/songs.png"), NavigatType = typeof(SongsPage) }, new CategoryListItem { Title = "Albums", Index = new Uri("ms-appx:///Assets/Images/albums.png"), NavigatType = typeof(AlbumsPage) }, new CategoryListItem { Title = "Artists", Index = new Uri("ms-appx:///Assets/Images/artists.png"), NavigatType = typeof(ArtistsPage) } };

            var item = CategoryList.FirstOrDefault(x => x.Title == settings.CategoryLastClicked);
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

            Navigate(CategoryList[0].NavigatType);

            if (Window.Current.Bounds.Width <= 640)
            {
                MainPageViewModel.Current.NeedShowTitle = false;
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
            if (MainPage.Current.SubPageCanGoBack)
            {
                MainFrame.GoBack();
            }
        }

        private async void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            Navigate((e.ClickedItem as CategoryListItem).NavigatType);
            var s = Settings.Load();
            s.CategoryLastClicked = (e.ClickedItem as CategoryListItem).Title;
            s.Save();
            await Task.Delay(100);
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
    }
}
