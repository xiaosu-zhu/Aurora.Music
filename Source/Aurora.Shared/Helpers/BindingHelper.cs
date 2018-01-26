// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
namespace Aurora.Shared.Helpers
{
    public static class BindingHelper
    {
        // Parse the path-format string into a Geometry
        public static Windows.UI.Xaml.Media.Geometry StringToPath(string pathData)
        {
            string xamlPath =
                "<Geometry xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>"
                + pathData + "</Geometry>";

            return Windows.UI.Xaml.Markup.XamlReader.Load(xamlPath) as Windows.UI.Xaml.Media.Geometry;
        }
    }
}
