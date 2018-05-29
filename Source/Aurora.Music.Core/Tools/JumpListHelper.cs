using System;
using System.Linq;
using System.Threading.Tasks;

using Windows.UI.StartScreen;

namespace Aurora.Music.Core.Tools
{
    public static class JumpListHelper
    {
        private static readonly string[] args = new string[]
        {
            "as-music:///?action=last-play",
            "as-music:///?action=play",
            "as-music:///?action=pause",
            "as-music:///?action=previous",
            "as-music:///?action=next",
            "as-music:///home",
            "as-music:///library",
            "as-music:///library/songs",
            "as-music:///library/albums",
            "as-music:///library/artists",
            "as-music:///library/playlist",
            "as-music:///library/podcast",
        };

        private static readonly string[] groups = new string[]
        {
            "NavigateGroup",
            "ActionGroup",
        };

        private static readonly string[] displays = new string[]
        {
            "JumplistLastPlay",
            "JumplistPlay",
            "JumplistPause",
            "JumplistPrevious",
            "JumplistNext",
            "JumplistHome",
            "JumplistLibrary",
            "JumplistSongs",
            "JumplistAlbums",
            "JumplistArtists",
            "JumplistPlaylist",
            "JumplistPodcast",
        };

        private static readonly string[] pics = new string[]
        {
            "ms-appx:///Assets/JumpList/lastplay.png",
            "ms-appx:///Assets/JumpList/play.png",
            "ms-appx:///Assets/JumpList/pause.png",
            "ms-appx:///Assets/JumpList/previous.png",
            "ms-appx:///Assets/JumpList/next.png",
            "ms-appx:///Assets/JumpList/home.png",
            "ms-appx:///Assets/JumpList/library.png",
            "ms-appx:///Assets/JumpList/songs.png",
            "ms-appx:///Assets/JumpList/albums.png",
            "ms-appx:///Assets/JumpList/artists.png",
            "ms-appx:///Assets/JumpList/playlist.png",
            "ms-appx:///Assets/JumpList/podcast.png",
        };

        public static async Task<JumpList> LoadJumpListAsync()
        {
            var jumpList = await JumpList.LoadCurrentAsync();
            var items = (from a in jumpList.Items where a.Arguments.StartsWith("as-music") select a).ToList();

            foreach (var j in items)
            {
                jumpList.Items.Remove(j);
            }

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].Contains("action"))
                {
                    jumpList.Items.Add(CreateItem(args[i], Consts.Localizer.GetString(displays[i]), Consts.Localizer.GetString(groups[1]), Consts.Localizer.GetString(displays[i]), new Uri(pics[i])));
                }
                else
                {
                    jumpList.Items.Add(CreateItem(args[i], Consts.Localizer.GetString(displays[i]), Consts.Localizer.GetString(groups[0]), Consts.Localizer.GetString(displays[i]), new Uri(pics[i])));
                }
            }

            return jumpList;
        }

        public static JumpListItem CreateItem(string argument, string displayName, string group, string desc, Uri pic)
        {
            var adding = JumpListItem.CreateWithArguments(argument, displayName);
            adding.GroupName = group;
            adding.Description = desc;
            adding.Logo = pic;
            return adding;
        }
    }
}
