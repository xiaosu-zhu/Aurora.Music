// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Aurora.Music.ViewModels;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace Aurora.Music.Controls.ListItems
{
    public sealed partial class SongItem : UserControl
    {
        public SongViewModel Data
        {
            get { return (SongViewModel)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }
        // Using a DependencyProperty as the backing store for Data.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register("Data", typeof(SongViewModel), typeof(SongItem), new PropertyMetadata(null));

        public bool IsMultiSelect
        {
            get { return (bool)GetValue(IsMultiSelectProperty); }
            set { SetValue(IsMultiSelectProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsMultiSelect.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsMultiSelectProperty =
            DependencyProperty.Register("IsMultiSelect", typeof(bool), typeof(SongItem), new PropertyMetadata(false, IsMultiSelectChanged));

        private static void IsMultiSelectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SongItem s)
            {
                s.Root.PointerCanceled -= s.Grid_PointerExited;
                s.Root.PointerCaptureLost -= s.Grid_PointerExited;
                s.Root.PointerExited -= s.Grid_PointerExited;
                s.Root.PointerEntered -= s.Grid_PointerEntered;
                s.Root.PointerPressed -= s.Grid_PointerPressed;
                s.Root.PointerReleased -= s.Grid_PointerReleased;
                if ((bool)e.NewValue)
                {
                    VisualStateManager.GoToState(s, "MultiNormal", false);
                }
                else
                {
                    VisualStateManager.GoToState(s, "Normal", false);
                }
                s.Root.PointerCanceled += s.Grid_PointerExited;
                s.Root.PointerCaptureLost += s.Grid_PointerExited;
                s.Root.PointerExited += s.Grid_PointerExited;
                s.Root.PointerEntered += s.Grid_PointerEntered;
                s.Root.PointerPressed += s.Grid_PointerPressed;
                s.Root.PointerReleased += s.Grid_PointerReleased;
            }
        }

        public SongItem()
        {
            InitializeComponent();
        }


        public string IsOnedrive(bool b)
        {
            return b ? "\uEBC3 " : "";
        }

        private void Grid_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (IsMultiSelect)
            {
                VisualStateManager.GoToState(this, "MultiPointerOver", false);
                return;
            }
            if (e.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Touch)
            {
                VisualStateManager.GoToState(this, "TouchPointerOver", false);
            }
            else
            {
                VisualStateManager.GoToState(this, "PointerOver", false);
            }
        }

        private void Grid_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (IsMultiSelect)
            {
                VisualStateManager.GoToState(this, "MultiNormal", false);
                return;
            }
            if (e.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Touch)
            {
                VisualStateManager.GoToState(this, "TouchNormal", false);
            }
            else
            {
                VisualStateManager.GoToState(this, "Normal", false);
            }
        }

        private void Grid_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (IsMultiSelect)
            {
                VisualStateManager.GoToState(this, "MultiPressed", false);
                return;
            }
            if (e.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Touch)
            {
                VisualStateManager.GoToState(this, "TouchPressed", false);
            }
            else
            {
                VisualStateManager.GoToState(this, "Pressed", false);
            }
        }

        private void Grid_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (IsMultiSelect)
            {
                VisualStateManager.GoToState(this, "MultiPointerOver", false);
                return;
            }
            if (e.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Touch)
            {
                VisualStateManager.GoToState(this, "TouchPointerOver", false);
            }
            else
            {
                VisualStateManager.GoToState(this, "PointerOver", false);
            }
        }

        public event RoutedEventHandler Play;
        public event RoutedEventHandler RequestMultiSelect;

        private void PlayBtn_Click(object sender, RoutedEventArgs e)
        {
            Play?.Invoke(Data, e);
        }

        private void HeaderBtn_Click(object sender, RoutedEventArgs e)
        {
            RequestMultiSelect?.Invoke(sender, e);
            Task.Run(async () =>
            {
                await Task.Delay(500);
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
                {
                    HeaderBtn.Visibility = Visibility.Collapsed;
                    PlayBtn.Visibility = Visibility.Collapsed;
                });
            });
        }
    }
}
