// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System;

namespace Aurora.Shared.Extensions
{
    public static class EnumExtensions
    {
        public static string GetDisplayName(this Enum e)
        {
            try
            {
                var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
                var resourceDisplayName = loader.GetString(e.GetType().Name + "_" + e);
                return resourceDisplayName;
            }
            catch (Exception)
            {
                return e.ToString();
            }
        }
    }
}
