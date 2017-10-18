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
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Aurora.Music.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class CategoryListPage : Page
    {
        public CategoryListPage()
        {
            this.InitializeComponent();
        }

        CategoryListItem[] categoryList = { new CategoryListItem { Title = "Songs" }, new CategoryListItem { Title = "Albums" }, new CategoryListItem { Title = "Artists" } };

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            PrepareAnimationWithItem(e.ClickedItem);
            LibraryPage.Current.Navigate(typeof(CategoryDetailsPage));
        }

        void PrepareAnimationWithItem(object item)
        {
            Category.PrepareConnectedAnimation("CategoryListIn", item, "Panel");
            Category.PrepareConnectedAnimation("CategoryTitleMove", item, "Title");
        }
    }



    public class CategoryListItem
    {
        public string Title { get; set; }
    }

}
