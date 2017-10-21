using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Aurora.Shared.Helpers
{
    public class StringHelper
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


    }
}
