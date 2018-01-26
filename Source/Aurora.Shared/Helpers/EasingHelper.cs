// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System;
using Windows.UI.Xaml.Media.Animation;

namespace Aurora.Shared.Helpers
{
    public static class EasingHelper
    {
        public static double CircleEase(EasingMode mode, double percent)
        {
            switch (mode)
            {
                case EasingMode.EaseOut:
                    return Math.Sqrt(1 - (1 - percent) * (1 - percent));
                case EasingMode.EaseIn:
                    return 1 - Math.Sqrt(1 - percent * percent);
                case EasingMode.EaseInOut:
                    if (percent <= 0.5)
                    {
                        return 0.5 - Math.Sqrt(0.25 - percent * percent);
                    }
                    return 0.5 + Math.Sqrt(0.25 - (percent - 1) * (percent - 1));
                default: return 0;
            }
        }

        public static double QuinticEase(EasingMode mode, double percent)
        {
            switch (mode)
            {
                case EasingMode.EaseOut:
                    return Math.Pow((percent - 1), 5) + 1;
                case EasingMode.EaseIn:
                    return Math.Pow(percent, 5);
                case EasingMode.EaseInOut:
                    if (percent <= 0.5)
                    {
                        return Math.Pow(percent * 2, 5) / 2;
                    }
                    return Math.Pow((percent - 1) * 2, 5) / 2 + 1;
                default: return 0;
            }
        }
    }
}
