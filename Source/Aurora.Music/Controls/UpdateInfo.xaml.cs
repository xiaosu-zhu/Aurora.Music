using Aurora.Music.Core;
using Aurora.Shared.Helpers;
using Windows.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace Aurora.Music.Controls
{
    public sealed partial class UpdateInfo : ContentDialog
    {
        public UpdateInfo()
        {
            this.InitializeComponent();
            Title = string.Format(Consts.UpdateNoteTitle, SystemInfoHelper.GetPackageVer());
            Note.Text = Consts.UpdateNote;
        }
    }
}
