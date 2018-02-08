// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

// The Templated Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234235

namespace Aurora.Music.Controls
{
    public sealed class ImageGrid : Control
    {
        private Grid main;

        public ImageGrid()
        {
            this.DefaultStyleKey = typeof(ImageGrid);
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            main = GetTemplateChild("Main") as Grid;
        }

        public object ImageSources
        {
            get { return (object)GetValue(ImageSourcesProperty); }
            set { SetValue(ImageSourcesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ImageSources.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImageSourcesProperty =
            DependencyProperty.Register("ImageSources", typeof(object), typeof(ImageGrid), new PropertyMetadata(null, ImageSourcesChanged));

        private static void ImageSourcesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ImageGrid imgGrid)
            {
                List<ImageSource> source = new List<ImageSource>();
                if (e.NewValue is IList<ImageSource> imgs)
                {
                    source = imgs.ToList();
                }
                else if (e.NewValue is IList<Uri> u)
                {
                    for (int i = 0; i < u.Count; i++)
                    {
                        source.Add(new BitmapImage());
                    }
                }
                var main = imgGrid.main;
                main.Children.Clear();
                main.ColumnDefinitions.Clear();
                main.RowDefinitions.Clear();
                var count = source.Count;
                if (count > 9) count = 9;

                for (int i = 0; i < count; i++)
                {
                    main.Children.Add(new Image
                    {
                        Source = source[i],
                        Stretch = Stretch.UniformToFill,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    });
                }
                switch (count)
                {
                    case 0:
                        return;
                    case 1:
                        break;
                    case 2:
                        main.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        main.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        main.Children[0].SetValue(Grid.ColumnProperty, 0);
                        main.Children[1].SetValue(Grid.ColumnProperty, 1);
                        break;
                    case 3:
                        main.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        main.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        main.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        main.Children[0].SetValue(Grid.ColumnProperty, 0);
                        main.Children[1].SetValue(Grid.ColumnProperty, 1);
                        main.Children[2].SetValue(Grid.ColumnProperty, 2);
                        break;
                    case 4:
                        main.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        main.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        main.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                        main.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                        main.Children[0].SetValue(Grid.ColumnProperty, 0);
                        main.Children[1].SetValue(Grid.ColumnProperty, 1);
                        main.Children[2].SetValue(Grid.ColumnProperty, 0);
                        main.Children[3].SetValue(Grid.ColumnProperty, 1);
                        main.Children[2].SetValue(Grid.RowProperty, 1);
                        main.Children[3].SetValue(Grid.RowProperty, 1);
                        break;
                    case 5:
                        main.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        main.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        main.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        main.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        main.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        main.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        main.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                        main.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                        main.Children[0].SetValue(Grid.ColumnProperty, 0);
                        main.Children[1].SetValue(Grid.ColumnProperty, 3);
                        main.Children[2].SetValue(Grid.ColumnProperty, 0);
                        main.Children[3].SetValue(Grid.ColumnProperty, 2);
                        main.Children[4].SetValue(Grid.ColumnProperty, 4);
                        main.Children[2].SetValue(Grid.RowProperty, 1);
                        main.Children[3].SetValue(Grid.RowProperty, 1);
                        main.Children[4].SetValue(Grid.RowProperty, 1);
                        main.Children[0].SetValue(Grid.ColumnSpanProperty, 3);
                        main.Children[1].SetValue(Grid.ColumnSpanProperty, 3);
                        main.Children[2].SetValue(Grid.ColumnSpanProperty, 2);
                        main.Children[3].SetValue(Grid.ColumnSpanProperty, 2);
                        main.Children[4].SetValue(Grid.ColumnSpanProperty, 2);
                        break;
                    case 6:
                        main.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        main.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        main.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        main.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                        main.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                        main.Children[0].SetValue(Grid.ColumnProperty, 0);
                        main.Children[1].SetValue(Grid.ColumnProperty, 1);
                        main.Children[2].SetValue(Grid.ColumnProperty, 2);
                        main.Children[3].SetValue(Grid.ColumnProperty, 0);
                        main.Children[4].SetValue(Grid.ColumnProperty, 1);
                        main.Children[5].SetValue(Grid.ColumnProperty, 2);
                        main.Children[3].SetValue(Grid.RowProperty, 1);
                        main.Children[4].SetValue(Grid.RowProperty, 1);
                        main.Children[5].SetValue(Grid.RowProperty, 1);
                        break;
                    case 7:
                        main.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        main.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        main.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        main.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        main.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        main.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        main.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        main.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        main.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        main.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        main.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        main.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        main.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                        main.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                        main.Children[0].SetValue(Grid.ColumnProperty, 0);
                        main.Children[1].SetValue(Grid.ColumnProperty, 4);
                        main.Children[2].SetValue(Grid.ColumnProperty, 8);
                        main.Children[3].SetValue(Grid.ColumnProperty, 0);
                        main.Children[4].SetValue(Grid.ColumnProperty, 3);
                        main.Children[5].SetValue(Grid.ColumnProperty, 6);
                        main.Children[6].SetValue(Grid.ColumnProperty, 9);
                        main.Children[3].SetValue(Grid.RowProperty, 1);
                        main.Children[4].SetValue(Grid.RowProperty, 1);
                        main.Children[5].SetValue(Grid.RowProperty, 1);
                        main.Children[6].SetValue(Grid.RowProperty, 1);
                        main.Children[0].SetValue(Grid.ColumnSpanProperty, 4);
                        main.Children[1].SetValue(Grid.ColumnSpanProperty, 4);
                        main.Children[2].SetValue(Grid.ColumnSpanProperty, 4);
                        main.Children[3].SetValue(Grid.ColumnSpanProperty, 3);
                        main.Children[4].SetValue(Grid.ColumnSpanProperty, 3);
                        main.Children[5].SetValue(Grid.ColumnSpanProperty, 3);
                        main.Children[6].SetValue(Grid.ColumnSpanProperty, 3);
                        break;
                    case 8:
                        main.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        main.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        main.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        main.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        main.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                        main.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                        main.Children[0].SetValue(Grid.ColumnProperty, 0);
                        main.Children[1].SetValue(Grid.ColumnProperty, 1);
                        main.Children[2].SetValue(Grid.ColumnProperty, 2);
                        main.Children[3].SetValue(Grid.ColumnProperty, 3);
                        main.Children[4].SetValue(Grid.ColumnProperty, 0);
                        main.Children[5].SetValue(Grid.ColumnProperty, 1);
                        main.Children[6].SetValue(Grid.ColumnProperty, 2);
                        main.Children[7].SetValue(Grid.ColumnProperty, 3);
                        main.Children[4].SetValue(Grid.RowProperty, 1);
                        main.Children[5].SetValue(Grid.RowProperty, 1);
                        main.Children[6].SetValue(Grid.RowProperty, 1);
                        main.Children[7].SetValue(Grid.RowProperty, 1);
                        break;
                    case 9:
                        main.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        main.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        main.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        main.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                        main.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                        main.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                        main.Children[0].SetValue(Grid.ColumnProperty, 0);
                        main.Children[1].SetValue(Grid.ColumnProperty, 1);
                        main.Children[2].SetValue(Grid.ColumnProperty, 2);
                        main.Children[3].SetValue(Grid.ColumnProperty, 0);
                        main.Children[4].SetValue(Grid.ColumnProperty, 1);
                        main.Children[5].SetValue(Grid.ColumnProperty, 2);
                        main.Children[6].SetValue(Grid.ColumnProperty, 0);
                        main.Children[7].SetValue(Grid.ColumnProperty, 1);
                        main.Children[8].SetValue(Grid.ColumnProperty, 2);
                        main.Children[0].SetValue(Grid.RowProperty, 0);
                        main.Children[1].SetValue(Grid.RowProperty, 0);
                        main.Children[2].SetValue(Grid.RowProperty, 0);
                        main.Children[3].SetValue(Grid.RowProperty, 1);
                        main.Children[4].SetValue(Grid.RowProperty, 1);
                        main.Children[5].SetValue(Grid.RowProperty, 1);
                        main.Children[6].SetValue(Grid.RowProperty, 2);
                        main.Children[7].SetValue(Grid.RowProperty, 2);
                        main.Children[8].SetValue(Grid.RowProperty, 2);
                        break;
                    default:
                        break;
                }

                if (e.NewValue is IList<Uri> uris)
                {
                    for (int i = 0; i < uris.Count; i++)
                    {
                        (source[i] as BitmapImage).UriSource = uris[i];
                    }
                }
            }
        }
    }

    public enum ImageGridOrientation { Horizontal, Vertical }
}
