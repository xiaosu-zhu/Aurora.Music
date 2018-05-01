using Aurora.Shared.Helpers;
using System.Threading.Tasks;

namespace Aurora.Music.Core.Tools
{
    public static class TimelineCard
    {
        public async static Task<string> AuthorAsync(string title, string album, string performers, string img0, string img1, int count)
        {

            // TODO: Notice that image url must not be ms-appx:/// or file://, how to show local files in timeline?

            var jsonText = await FileIOHelper.ReadStringFromAssetsAsync("Timeline.json");
            var desc0 = string.Format(Consts.Localizer.GetString("TileDesc"), album, performers);
            if (img0 != null)
            {
                img0 = img0.Replace(@"\", @"\\");
            }
            if (img1 != null)
            {
                img1 = img1.Replace(@"\", @"\\");
            }
            var res = jsonText.Replace("$bg", img0 ?? Consts.BlackPlaceholder).Replace("$title", "Last played in Aurora Music").Replace("$col0", title).Replace("$desc0", desc0).Replace("$img", img1 ?? Consts.BlackPlaceholder).Replace("$col1", count > 1 ? string.Format(Consts.Localizer.GetString("AndMore"), count - 1) : string.Empty).Replace("$desc1", "Tap this card to contine listening");
            return res;
        }
    }
}
