// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Aurora.Music.ViewModels;
using Windows.System.Threading;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Aurora.Music.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class AddFoldersView : Page
    {
        public AddFoldersView()
        {
            this.InitializeComponent();
            var t = ThreadPool.RunAsync(async x => await Context.Init());
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter != null)
            {
                Context.ChangeForeGround();
            }
        }

        private async void DeleteFolderBtn_Click(object sender, RoutedEventArgs e)
        {
            await Context.RemoveFolder((sender as Button).DataContext as FolderViewModel);
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var b = (e.ClickedItem as FolderViewModel).IsOpened;
            foreach (var item in Context.Folders)
            {
                item.IsOpened = false;
            }
            (e.ClickedItem as FolderViewModel).IsOpened = !b;
        }
    }
}
