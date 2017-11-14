using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Media.Playback;

namespace Aurora.Music.Background
{
    public sealed class BackgroundPlayer : IBackgroundTask
    {
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();

#pragma warning disable CS0618 // 类型或成员已过时
            var bgPlayer = BackgroundMediaPlayer.Current;
#pragma warning restore CS0618 // 类型或成员已过时

        }
    }
}
