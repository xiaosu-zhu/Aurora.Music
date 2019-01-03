// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Aurora.Music.Core.Models;
using Aurora.Music.ViewModels;
using System;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Aurora.Music.Controls
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class CompactOverlayPanel : Page
    {
        public CompactOverlayPanel()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is SongViewModel s)
            {
                Context.Init(s);
            }
            else
            {
                throw new Exception();
            }

            SystemNavigationManager.GetForCurrentView().BackRequested += CompactOverlayPanel_BackRequested;
            Window.Current.SizeChanged += Current_SizeChanged;
            RequestedTheme = Settings.Current.Theme;

            AddHandler(PointerExitedEvent, new PointerEventHandler(Page_PointerExited), true);
            AddHandler(PointerCanceledEvent, new PointerEventHandler(Page_PointerExited), true);
            AddHandler(PointerCaptureLostEvent, new PointerEventHandler(Page_PointerExited), true);
            AddHandler(PointerEnteredEvent, new PointerEventHandler(Page_PointerEntered), true);
        }

        private void Current_SizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            Settings.Current.CompactHeight = e.Size.Height;
            Settings.Current.CompactWidth = e.Size.Width;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            SystemNavigationManager.GetForCurrentView().BackRequested -= CompactOverlayPanel_BackRequested;
            Window.Current.SizeChanged -= Current_SizeChanged;
            Settings.Current.Save();

            RemoveHandler(PointerExitedEvent, new PointerEventHandler(Page_PointerExited));
            RemoveHandler(PointerCanceledEvent, new PointerEventHandler(Page_PointerExited));
            RemoveHandler(PointerCaptureLostEvent, new PointerEventHandler(Page_PointerExited));
            RemoveHandler(PointerEnteredEvent, new PointerEventHandler(Page_PointerEntered));
        }

        private void CompactOverlayPanel_BackRequested(object sender, BackRequestedEventArgs e)
        {
            e.Handled = true;
            Context.ReturnNormal.Execute();
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().BackRequested -= CompactOverlayPanel_BackRequested;
            Window.Current.SizeChanged -= Current_SizeChanged;
            Settings.Current.Save();
        }

        private void ArtworkBGBlur_Loaded(object sender, RoutedEventArgs e)
        {
            Window.Current.SetTitleBar(null);
        }

        private void Page_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            PointerIn.Begin();
        }

        private bool tap = true;

        private void Page_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Touch || e.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Pen)
            {
                tap = !tap;
                if (tap)
                {
                    PointerOut.Begin();
                }
            }
            else
            {
                PointerOut.Begin();
            }
        }
    }
}
