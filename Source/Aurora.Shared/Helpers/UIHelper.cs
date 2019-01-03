// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Aurora.Shared.Helpers
{
    public enum ScrollPosition
    {
        Top, Center, Bottom
    }

    public static class UIHelper
    {
        public static void Change_Row_Column(FrameworkElement d, int row, int column)
        {
            d.SetValue(Grid.RowProperty, row);
            d.SetValue(Grid.ColumnProperty, column);
        }

        public static void ReverseVisibility(FrameworkElement e)
        {
            if (e.Visibility == Visibility.Collapsed)
            {
                e.Visibility = Visibility.Visible;
            }
            else
            {
                e.Visibility = Visibility.Collapsed;
            }
        }

        public static ScrollViewer GetScrollViewer(this DependencyObject element)
        {
            if (element is ScrollViewer)
            {
                return (ScrollViewer)element;
            }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
            {
                var child = VisualTreeHelper.GetChild(element, i);

                var result = GetScrollViewer(child);
                if (result == null)
                {
                    continue;
                }
                else
                {
                    return result;
                }
            }

            return null;
        }


        public static void ReplaceElements(this Canvas canvas, double horizontalScale, double verticalScale)
        {
            foreach (var item in canvas.Children)
            {
                var left = (double)item.GetValue(Canvas.LeftProperty);
                item.SetValue(Canvas.LeftProperty, left * horizontalScale);
                var top = (double)item.GetValue(Canvas.TopProperty);
                item.SetValue(Canvas.TopProperty, top * verticalScale);
            }
        }

        public static T GetFirst<T>(this DependencyObject o) where T : FrameworkElement
        {
            // Return the DependencyObject if it is a ScrollViewer
            if (o is T)
            {
                return o as T;
            }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(o); i++)
            {
                var child = VisualTreeHelper.GetChild(o, i);

                var result = GetFirst<T>(child);
                if (result == null)
                {
                    continue;
                }
                else
                {
                    return result;
                }
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="listViewBase"></param>
        /// <param name="index"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public async static Task ScrollToIndex(this ListViewBase listViewBase, int index, ScrollPosition s)
        {
            try
            {
                bool isVirtualizing = default(bool);
                double previousHorizontalOffset = default(double), previousVerticalOffset = default(double);
                // get the ScrollViewer withtin the ListView/GridView
                var scrollViewer = listViewBase.GetScrollViewer();
                // get the SelectorItem to scroll to
                var selectorItem = listViewBase.ContainerFromIndex(index) as UIElement;

                // when it's null, means virtualization is on and the item hasn't been realized yet
                if (selectorItem == null)
                {
                    isVirtualizing = true;

                    previousHorizontalOffset = scrollViewer.HorizontalOffset;
                    previousVerticalOffset = scrollViewer.VerticalOffset;

                    // call task-based ScrollIntoViewAsync to realize the item
                    await listViewBase.ScrollIntoViewAsync(listViewBase.Items[index]);

                    // this time the item shouldn't be null again
                    selectorItem = (UIElement)listViewBase.ContainerFromIndex(index);
                }

                // calculate the position object in order to know how much to scroll to
                var transform = selectorItem.TransformToVisual((UIElement)scrollViewer.Content);

                // offset : positivie down, negative up
                Point position;
                switch (s)
                {
                    case ScrollPosition.Top:
                        position = transform.TransformPoint(new Point(0, 0));
                        break;
                    case ScrollPosition.Center:
                        position = transform.TransformPoint(new Point(0, 0 - (listViewBase.ActualHeight - (selectorItem as ListViewItem).ActualHeight) / 2));
                        break;
                    case ScrollPosition.Bottom:
                        position = transform.TransformPoint(new Point(0, 0 - listViewBase.ActualHeight + (selectorItem as ListViewItem).ActualHeight));
                        break;
                    default:
                        position = transform.TransformPoint(new Point(0, 0));
                        break;
                }


                // when virtualized, scroll back to previous position without animation
                if (isVirtualizing)
                {
                    await scrollViewer.ChangeViewAsync(previousHorizontalOffset, previousVerticalOffset, true);
                }

                // scroll to desired position with animation!
                scrollViewer.ChangeView(position.X, position.Y, null);
            }
            catch (Exception)
            {

            }
        }

        public async static Task ScrollToItem(this ListViewBase listViewBase, object item)
        {
            bool isVirtualizing = default(bool);
            double previousHorizontalOffset = default(double), previousVerticalOffset = default(double);

            // get the ScrollViewer withtin the ListView/GridView
            var scrollViewer = listViewBase.GetScrollViewer();
            // get the SelectorItem to scroll to
            var selectorItem = listViewBase.ContainerFromItem(item) as UIElement;

            // when it's null, means virtualization is on and the item hasn't been realized yet
            if (selectorItem == null)
            {
                isVirtualizing = true;

                previousHorizontalOffset = scrollViewer.HorizontalOffset;
                previousVerticalOffset = scrollViewer.VerticalOffset;

                // call task-based ScrollIntoViewAsync to realize the item
                await listViewBase.ScrollIntoViewAsync(item);

                // this time the item shouldn't be null again
                selectorItem = (UIElement)listViewBase.ContainerFromItem(item);
            }

            // calculate the position object in order to know how much to scroll to
            var transform = selectorItem.TransformToVisual((UIElement)scrollViewer.Content);
            var position = transform.TransformPoint(new Point(0, 0));

            // when virtualized, scroll back to previous position without animation
            if (isVirtualizing)
            {
                await scrollViewer.ChangeViewAsync(previousHorizontalOffset, previousVerticalOffset, true);
            }

            // scroll to desired position with animation!
            scrollViewer.ChangeView(position.X, position.Y, null);
        }

        public static async Task ScrollIntoViewAsync(this ListViewBase listViewBase, object item)
        {
            var tcs = new TaskCompletionSource<object>();
            var scrollViewer = listViewBase.GetScrollViewer();

            EventHandler<ScrollViewerViewChangedEventArgs> viewChanged = (s, e) => tcs.TrySetResult(null);
            try
            {
                scrollViewer.ViewChanged += viewChanged;
                listViewBase.ScrollIntoView(item, ScrollIntoViewAlignment.Leading);
                await tcs.Task;
            }
            finally
            {
                scrollViewer.ViewChanged -= viewChanged;
            }
        }

        public static async Task ChangeViewAsync(this ScrollViewer scrollViewer, double? horizontalOffset, double? verticalOffset, bool disableAnimation)
        {
            var tcs = new TaskCompletionSource<object>();

            EventHandler<ScrollViewerViewChangedEventArgs> viewChanged = (s, e) => tcs.TrySetResult(null);
            try
            {
                scrollViewer.ViewChanged += viewChanged;
                scrollViewer.ChangeView(horizontalOffset, verticalOffset, null, disableAnimation);
                await tcs.Task;
            }
            finally
            {
                scrollViewer.ViewChanged -= viewChanged;
            }
        }

        public static void ChangeTitlebarButtonColor(Color backGround, Color foreGround)
        {
            var view = ApplicationView.GetForCurrentView();
            var otherB = backGround;
            var otherF = foreGround;
            view.TitleBar.ButtonBackgroundColor = backGround;
            view.TitleBar.ButtonForegroundColor = foreGround;
        }
    }
}
