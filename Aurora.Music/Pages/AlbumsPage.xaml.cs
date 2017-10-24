using Aurora.Music.ViewModels;
using Windows.System.Threading;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Aurora.Music.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class AlbumsPage : Page
    {
        public AlbumsPage()
        {
            this.InitializeComponent();
            ThreadPool.RunAsync(async x =>
            {
                await Context.GetAlbums();
            });
        }

        private void StackPanel_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (sender is StackPanel s)
            {
                (s.Resources["PointerOver"] as Storyboard).Begin();
            }
        }

        private void StackPanel_PointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (sender is StackPanel s)
            {
                (s.Resources["Normal"] as Storyboard).Begin();
            }
        }

        private async void AlbumList_ItemClick(object sender, ItemClickEventArgs e)
        {
            await Context.PlayAlbum(e.ClickedItem as AlbumViewModel);
        }
    }
}
