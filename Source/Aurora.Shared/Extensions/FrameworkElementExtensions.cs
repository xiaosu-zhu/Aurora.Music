// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Windows.Foundation;
using Windows.UI.Xaml;

namespace Aurora.Shared.Extensions
{
    public static class FrameworkElementExtensions
    {
        public static Point GetPositioninParent(this FrameworkElement f, FrameworkElement root)
        {
            if (f != null)
            {
                var transform = f.TransformToVisual(root);
                return transform.TransformPoint(new Point(0, 0));
            }
            return default(Point);
        }
    }
}
