using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Aurora.Shared.Extensions
{
    public static class UIElementExtensions
    {
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

        public static ScrollViewer GetScrollViewer(this DependencyObject o)
        {
            // Return the DependencyObject if it is a ScrollViewer
            if (o is ScrollViewer)
            {
                return o as ScrollViewer;
            }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(o); i++)
            {
                var child = VisualTreeHelper.GetChild(o, i);

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
    }
}
