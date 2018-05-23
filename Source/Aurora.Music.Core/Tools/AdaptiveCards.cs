using Aurora.Shared.Helpers;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace Aurora.Music.Core.Tools
{
    public static class TimelineCard
    {
        public async static Task<string> AuthorAsync(string title, string album, string performers, string img0, string img1, int count)
        {

            // TODO: Notice that image url must be web content, not any ms-appdata:/// or file://, so how to show local images in timeline?


            var desc0 = string.Format(Consts.Localizer.GetString("TileDesc"), album, performers);

            var jsonText = await FileIOHelper.ReadStringFromAssetsAsync(Consts.TimelineJson);

            var content = JObject.Parse(jsonText);

            if (img0 == null && img1 == null)
                content.Remove("backgroundImage");
            else
            {
                var channel = (JValue)content["backgroundImage"];
                channel.Value = img0 ?? img1;
            }

            var items = (JArray)content["body"][0]["items"];

            ((JValue)items[0]["text"]).Value = Consts.Localizer.GetString("TimelineTitle");

            var col0 = (JArray)(items[1]["columns"]);

            ((JValue)col0[1]["items"][0]["text"]).Value = title;
            ((JValue)col0[1]["items"][1]["text"]).Value = desc0;

            if (img0 != null)
                ((JValue)col0[0]["items"][0]["url"]).Value = img0;
            else
            {
                (col0).RemoveAt(0);
            }

            var col1 = (JArray)(items[2]["columns"]);

            ((JValue)col1[1]["items"][0]["text"]).Value = count > 1 ? string.Format(Consts.Localizer.GetString("AndMore"), count - 1) : string.Empty;
            ((JValue)col1[1]["items"][1]["text"]).Value = Consts.Localizer.GetString("TimelineDetail");

            if (img1 != null)
                ((JValue)col1[0]["items"][0]["url"]).Value = img1;
            else
            {
                (col1).RemoveAt(0);
            }

            return content.ToString();
        }
    }
}
