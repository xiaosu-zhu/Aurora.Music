using Aurora.Music.Core.Models;
using Aurora.Shared.Extensions;
using Aurora.Shared.MVVM;

namespace Aurora.Music.ViewModels
{
    class ArtistViewModel : ViewModelBase, IKey
    {
        public string RawName;

        private string name;
        public string Name
        {
            get { return name; }
            set
            {
                if (value.IsNullorWhiteSpace())
                {
                    SetProperty(ref name, "Unknown Artist");
                    RawName = string.Empty;
                }
                else
                {
                    SetProperty(ref name, value.Replace("$|$", ", "));
                    RawName = value;
                }
            }
        }

        private int albumCount;
        public int SongsCount
        {
            get { return albumCount; }
            set { SetProperty(ref albumCount, value); }
        }

        public string Key
        {
            get
            {
                if (Name.StartsWith("The ", System.StringComparison.CurrentCultureIgnoreCase))
                {
                    return Name.Substring(4);
                }
                if (Name.StartsWith("A ", System.StringComparison.CurrentCultureIgnoreCase))
                {
                    return Name.Substring(2);
                }
                if (Name.StartsWith("An ", System.StringComparison.CurrentCultureIgnoreCase))
                {
                    return Name.Substring(3);
                }
                return Name;

            }
        }

        public string CountToString(int count)
        {
            return $"{count} Songs";
        }
    }
}
