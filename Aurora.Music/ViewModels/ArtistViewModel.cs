using Aurora.Shared.Extensions;
using Aurora.Shared.MVVM;

namespace Aurora.Music.ViewModels
{
    class ArtistViewModel : ViewModelBase
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
                }
                else
                {
                    SetProperty(ref name, value);
                }
                RawName = value;
            }
        }

        private int albumCount;
        public int SongsCount
        {
            get { return albumCount; }
            set { SetProperty(ref albumCount, value); }
        }

        public string CountToString(int count)
        {
            return $"{count} Songs";
        }
    }
}
