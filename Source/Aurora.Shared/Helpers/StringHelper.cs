// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Aurora.Shared.Helpers
{
    public class StringHelper : IComparer<string>
    {
        private const string FOLDER_SEPARATOR = "/";
        private const string WINDOWS_FOLDER_SEPARATOR = "\\";
        private const string TOP_PATH = "..";
        private const string CURRENT_PATH = ".";
        private const char EXTENSION_SEPARATOR = '.';
        public const string REG_CN = "[\u4e00-\u9fa5]";

        public static bool IsChinese(string s)
        {
            return Regex.IsMatch(s, REG_CN);
        }

        public int Compare(string x, string y)
        {
            return CultureInfo.CurrentCulture.CompareInfo.Compare(x, y);
        }
    }
}
