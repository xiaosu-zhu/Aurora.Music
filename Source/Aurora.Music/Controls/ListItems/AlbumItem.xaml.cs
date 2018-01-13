using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;

namespace Aurora.Music.Controls.ListItems
{
    internal partial class AlbumItem
    {
        public AlbumItem()
        {
            InitializeComponent();
        }

        private void StackPanel_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Panel s)
            {
                (s.Resources["PointerPressed"] as Storyboard).Begin();
            }
        }

        private void StackPanel_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Touch)
            { return; }
            if (sender is Panel s)
            {
                (s.Resources["PointerOver"] as Storyboard).Begin();
            }
        }

        private void StackPanel_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Touch)
            { return; }
            if (sender is Panel s)
            {
                (s.Resources["PointerOver"] as Storyboard).Begin();
            }
        }

        private void StackPanel_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Panel s)
            {
                (s.Resources["Normal"] as Storyboard).Begin();
            }
        }
    }
}
