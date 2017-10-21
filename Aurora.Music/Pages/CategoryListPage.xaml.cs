using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Aurora.Music.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class CategoryListPage : Page
    {
        private static object clickedItem;

        public CategoryListPage()
        {
            this.InitializeComponent();
        }

        CategoryListItem[] categoryList = { new CategoryListItem { Title = "Songs" }, new CategoryListItem { Title = "Albums" }, new CategoryListItem { Title = "Artists" } };

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            PrepareAnimationWithItem(e.ClickedItem);
            clickedItem = e.ClickedItem;
            LibraryPage.Current.Navigate(typeof(CategoryDetailsPage), (e.ClickedItem as CategoryListItem).Title);
        }

        void PrepareAnimationWithItem(object item)
        {
            Category.PrepareConnectedAnimation("CategoryListIn", item, "Panel");
            Category.PrepareConnectedAnimation("CategoryTitleIn", item, "Title");
        }

        private void Category_Loaded(object sender, RoutedEventArgs e)
        {
            //if (clickedItem != null)
            //{
            //    var animation =
            //        ConnectedAnimationService.GetForCurrentView().GetAnimation("CategoryListOut");
            //    var animation1 =
            //       ConnectedAnimationService.GetForCurrentView().GetAnimation("CategoryTitleOut");
            //    if (animation != null)
            //    {
            //        Category.TryStartConnectedAnimationAsync(
            //            animation, clickedItem, "Panel");
            //    }
            //    if (animation1 != null)
            //    {
            //        Category.TryStartConnectedAnimationAsync(
            //            animation1, clickedItem, "Title");
            //    }
            //}
        }
    }



    public class CategoryListItem
    {
        public string Title { get; set; }
    }

}
