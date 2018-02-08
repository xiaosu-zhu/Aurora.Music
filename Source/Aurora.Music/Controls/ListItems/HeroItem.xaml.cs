// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Aurora.Music.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace Aurora.Music.Controls.ListItems
{
    public sealed partial class HeroItem : UserControl
    {
        public HeroItem()
        {
            this.InitializeComponent();
        }
        public GenericMusicItemViewModel Data
        {
            get { return (GenericMusicItemViewModel)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }
        // Using a DependencyProperty as the backing store for Data.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register("Data", typeof(GenericMusicItemViewModel), typeof(HeroItem), new PropertyMetadata(null));

        private void Grid_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, "Normal", false);
        }

        private void Grid_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, "PointerOver", false);
        }

        private void Grid_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, "Pressed", false);
        }

        private void Grid_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, "PointerOver", false);
        }


        public double TwoYOffset
        {
            get { return (double)GetValue(TwoYOffsetProperty); }
            set { SetValue(TwoYOffsetProperty, value); }
        }
        // Using a DependencyProperty as the backing store for TwoYOffset.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TwoYOffsetProperty =
            DependencyProperty.Register("TwoYOffset", typeof(double), typeof(HeroItem), new PropertyMetadata(-14.0));

        public double OneYOffset
        {
            get { return (double)GetValue(OneYOffsetProperty); }
            set { SetValue(OneYOffsetProperty, value); }
        }
        // Using a DependencyProperty as the backing store for TwoYOffset.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OneYOffsetProperty =
            DependencyProperty.Register("OneYOffset", typeof(double), typeof(HeroItem), new PropertyMetadata(-8.0));
    }
}
