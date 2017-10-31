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
    public sealed partial class CategoryDetailsView : Page
    {
        public CategoryDetailsView()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            Title.Text = e.Parameter as string;
            var ani = ConnectedAnimationService.GetForCurrentView().GetAnimation("CategoryListIn");
            if (ani != null)
            {
                ani.TryStart(Panel, new UIElement[] { Main });
            }
            ani = ConnectedAnimationService.GetForCurrentView().GetAnimation("CategoryTitleIn");
            if (ani != null)
            {
                ani.TryStart(Title);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("CategoryListOut", Panel);

            ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("CategoryTitleOut", Title);

            LibraryPage.Current.LefPanelNavigate(typeof(CategoryListView), Title.Text);
        }
    }
}
