// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Aurora.Music.Core;
using Aurora.Music.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace Aurora.Music.Controls.ListItems
{
    public sealed partial class AlbumItem : UserControl
    {
        public AlbumItem()
        {
            this.InitializeComponent();
        }

        public AlbumViewModel Data
        {
            get { return (AlbumViewModel)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }
        // Using a DependencyProperty as the backing store for Data.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register("Data", typeof(AlbumViewModel), typeof(AlbumItem), new PropertyMetadata(null));

        public event RoutedEventHandler PlayAlbum;

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            PlayAlbum?.Invoke(Data, e);
        }

        public event RoutedEventHandler FlyoutRequired;

        private void Flyout_Click(object sender, RoutedEventArgs e)
        {
            FlyoutRequired?.Invoke(sender, e);
        }

        private void StackPanel_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Touch)
            {
                VisualStateManager.GoToState(this, "TouchPointerOver", false);
            }
            else
            {
                VisualStateManager.GoToState(this, "PointerOver", false);
            }
        }

        private void StackPanel_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Touch)
            {
                VisualStateManager.GoToState(this, "TouchNormal", false);
            }
            else
            {
                VisualStateManager.GoToState(this, "Normal", false);
            }
        }

        private void StackPanel_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Touch)
            {
                VisualStateManager.GoToState(this, "TouchPressed", false);
            }
            else
            {
                VisualStateManager.GoToState(this, "Pressed", false);
            }
        }

        private void StackPanel_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Touch)
            {
                VisualStateManager.GoToState(this, "TouchPointerOver", false);
            }
            else
            {
                VisualStateManager.GoToState(this, "PointerOver", false);
            }
        }

        internal void StartConnectedAnimation(ConnectedAnimation ani, string n)
        {
            if (FindName(n) is UIElement item)
            {
                ani.TryStart(item);
            }
        }

        internal void PrePareConnectedAnimation()
        {
            ConnectedAnimationService.GetForCurrentView().PrepareToAnimate(Consts.AlbumItemConnectedAnimation + "_1", AlbumName);
            ConnectedAnimationService.GetForCurrentView().PrepareToAnimate(Consts.AlbumItemConnectedAnimation + "_2", Arts);
            VisualStateManager.GoToState(this, "Normal", false);
        }
    }
}
