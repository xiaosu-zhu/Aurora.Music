// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System;
using System.ComponentModel;
using Aurora.Music.Core;
using Aurora.Music.Core.Models;
using Aurora.Music.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace Aurora.Music.Controls.ListItems
{
    public sealed partial class HomeItem : UserControl
    {
        public HomeItem()
        {
            InitializeComponent();
        }
        public bool NightModeEnabled { get; set; } = Settings.Current.NightMode;
        public GenericMusicItemViewModel Data
        {
            get { return (GenericMusicItemViewModel)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }
        // Using a DependencyProperty as the backing store for Data.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register("Data", typeof(GenericMusicItemViewModel), typeof(HomeItem), new PropertyMetadata(null, OnDataChanged));

        public BitmapImage DataArtwork
        {
            get { return (BitmapImage)GetValue(DataArtworkProperty); }
            set { SetValue(DataArtworkProperty, value); }
        }
        // Using a DependencyProperty as the backing store for DataArtwork.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DataArtworkProperty =
            DependencyProperty.Register("DataArtwork", typeof(BitmapImage), typeof(HomeItem), new PropertyMetadata(null));

        private static void OnDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is HomeItem h)
            {
                if (e.NewValue is GenericMusicItemViewModel g)
                {
                    if (g.Artwork == null)
                    {
                        h.DataArtwork = new BitmapImage(new Uri(Consts.BlackPlaceholder));
                    }
                    else
                    {
                        h.DataArtwork = new BitmapImage(g.Artwork)
                        {
                            DecodePixelHeight = 150,
                            DecodePixelType = DecodePixelType.Logical
                        };
                        h.DataArtwork.ImageOpened += h.DataArtwork_ImageOpened;
                    }
                    g.PropertyChanged += h.G_PropertyChanged;
                }
            }
        }

        public string AlbumIndicate(bool b)
        {
            return b ? "    " : "";
        }

        public Visibility Placeholder(MediaType t)
        {
            return t == MediaType.Placeholder ? Visibility.Collapsed : Visibility.Visible;
        }

        public Visibility NotPlaceholder(MediaType t)
        {
            return t != MediaType.Placeholder ? Visibility.Collapsed : Visibility.Visible;
        }

        private void G_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Artwork")
            {
                if (Data.Artwork == null)
                {
                    DataArtwork = new BitmapImage(new Uri(Consts.BlackPlaceholder));
                }
                else
                {
                    DataArtwork = new BitmapImage(Data.Artwork)
                    {
                        DecodePixelHeight = 150,
                        DecodePixelType = DecodePixelType.Logical
                    };
                    DataArtwork.ImageOpened += DataArtwork_ImageOpened;
                }
            }
        }

        private void DataArtwork_ImageOpened(object sender, RoutedEventArgs e)
        {
            if (DataArtwork == null)
                return;
            MaxWidth = DataArtwork.PixelWidth * 150d / DataArtwork.PixelHeight;
            DataArtwork.ImageOpened -= DataArtwork_ImageOpened;
            SizeChanged += HomeItem_SizeChanged;
            Artwork.Width = double.NaN;
        }

        private void HomeItem_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            MaxWidth = Artwork.ActualWidth;
        }

        double ToActualWidth(int pixelHeight, int pixelWidth)
        {
            return pixelWidth * 150d / pixelHeight;
        }

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

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            DataArtwork = null;
            SizeChanged -= HomeItem_SizeChanged;
        }
    }
}
