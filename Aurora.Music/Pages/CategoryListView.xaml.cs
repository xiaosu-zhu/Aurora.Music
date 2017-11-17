using Aurora.Music.Core.Models;
using Aurora.Shared.MVVM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
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
    public sealed partial class CategoryListView : Page
    {

        public CategoryListView()
        {
            var settings = Settings.Load();
            categoryList = new ObservableCollection<CategoryListItem>() { new CategoryListItem { Title = "Songs", Index = new Uri("ms-appx:///Assets/Images/1.png"), NavigatType = typeof(HomePage) }, new CategoryListItem { Title = "Albums", Index = new Uri("ms-appx:///Assets/Images/2.png"), NavigatType = typeof(AlbumsPage) }, new CategoryListItem { Title = "Artists", Index = new Uri("ms-appx:///Assets/Images/3.png"), NavigatType = typeof(ArtistsPage) } };

            var item = categoryList.FirstOrDefault(x => x.Title == settings.CategoryLastClicked);
            if (item != default(CategoryListItem))
            {
                item.IsCurrent = true;
                categoryList.Remove(item);
                categoryList.Insert(0, item);
            }
            else
            {
                categoryList[0].IsCurrent = true;
            }

            LibraryPage.Current.Navigate(categoryList[0].NavigatType);

            this.InitializeComponent();
        }
        internal ObservableCollection<CategoryListItem> categoryList;

        private async void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            PrepareAnimationWithItem();
            CompleteAnimationWithItems(e.ClickedItem as CategoryListItem);

            await Task.Delay(100);

            LibraryPage.Current.Navigate((e.ClickedItem as CategoryListItem).NavigatType);
        }

        private async void CompleteAnimationWithItems(CategoryListItem item)
        {
            categoryList.Remove(item);
            categoryList.Insert(0, item);

            await Task.Delay(100);


            foreach (var cat in categoryList)
            {
                var ani = ConnectedAnimationService.GetForCurrentView().GetAnimation(cat.Title);
                if (ani != null)
                {
                    await Category.TryStartConnectedAnimationAsync(ani, cat, "Panel");
                }
            }

            foreach (var cat in categoryList)
            {
                cat.IsCurrent = false;
            }

            item.IsCurrent = true;

        }

        void PrepareAnimationWithItem()
        {
            foreach (var cat in categoryList)
            {
                Category.PrepareConnectedAnimation(cat.Title, cat, "Panel");
                cat.IsCurrent = false;
            }
        }
    }



    public class CategoryListItem : ViewModelBase
    {
        public string Title { get; set; }

        public Uri Index { get; set; }

        private bool isCurrent;
        public bool IsCurrent
        {
            get { return isCurrent; }
            set { SetProperty(ref isCurrent, value); }
        }

        public Type NavigatType { get; set; }

        public double GetHeight(bool b)
        {
            return b ? 192d : 96d;
        }
    }

}
