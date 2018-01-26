// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Windows.Devices.Input;

namespace Aurora.Shared.Helpers
{
    public static class InteractionHelper
    {
        public static bool HaveTouchCapabilities()
        {
            var touch = new TouchCapabilities();
            return touch.TouchPresent > 0;
        }

        public static string CheckDeviceFamily()
        {
            return Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily;
        }
    }
}
