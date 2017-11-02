using Aurora.Music.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Aurora.Music.Pages
{
    public sealed partial class AlbumDetailPage : Page
    {
        public AlbumDetailPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if(e.Parameter is AlbumViewModel s)
            {
                await Context.GetSongsAsync(s);
            }
        }
    }
}
