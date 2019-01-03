# 极光音乐

[English](./README.md)

<p align="center">
<a href="https://www.microsoft.com/store/apps/9NBLGGH6JVDT?ocid=badge"><img width="200" src="https://github.com/pkzxs/Aurora.Music/blob/master/Assets/appstore1080_cn.png" alt="Logo" /></a></p>


<p align="center">
<a href="https://www.microsoft.com/store/apps/9NBLGGH6JVDT?ocid=badge"><img width="200" src="https://assets.windowsphone.com/42e5aa4a-f19a-4205-9191-a97105ed7663/Chinese_Simplified_get-it-from-MS_InvariantCulture_Default.png" alt="从 Microsoft 获取" /></a></p>

一个通用 Windows 平台上的轻巧简洁的音乐播放器。

## 简介

极光音乐已入选 [Windows 2018 开发者奖项](https://developer.microsoft.com/en-us/windows/projects/events/build/2018/awards?utm_campaign=devawards18&utm_source=devcenter&utm_medium=owned&utm_content=hero)：**设计启发者**之一！🎉

---

极光音乐致力于为 Windows 10 平台带来现代，流畅的音乐体验。极光音乐使用 MIT 许可，你可以从这里分叉然后构建你自己的个性化音乐播放器。

同时，极光音乐可以扩展功能，我们为歌词、媒体信息、在线音乐保留了接口。通过你开发的扩展，极光音乐就能扩充它的能力。我们同样计划引入更高级的扩展功能，比如音频效果、界面布局等等。请见[待完成](https://github.com/pkzxs/Aurora.Music/blob/master/README_CN.md#待完成)。

下面是一些运行截图：

（背景图像来自 [Jeremy Bishop on Unsplash](https://unsplash.com/photos/9pRjY4d7nJE)，专辑版权归艺术家所有，此处仅作为演示）

![](https://i.loli.net/2018/04/09/5acad08ca1bf7.png "主页")
![](https://i.loli.net/2018/04/09/5acad0d79a2d2.png "正在播放")
![](https://i.loli.net/2018/04/09/5acad0cb88213.png "全部专辑")
![](https://i.loli.net/2018/04/09/5acad0d170c2c.png "全部歌曲")
![](https://i.loli.net/2018/04/09/5acad0d17c25f.png "全部歌手")
![](https://i.loli.net/2018/04/09/5acad0d1aba75.png "专辑详情")
![](https://i.loli.net/2018/04/09/5acad0d623383.png "豆瓣 FM")


## 文档与样例

请移步 **[这里](./Documentation)** 阅读开发扩展的技巧，其中包含一个小教程帮助你建立第一个扩展，同时其中还有一份详细的接口解释。

**[这里](./Samples)** 同样有一份代码示例，克隆到本地并部署它，然后瞧瞧歌词有什么变化吧。

别急，如果你在使用应用的过程中遇到了任何问题，请联系 [aurora.studio@outlook.com](mailto:aurora.studio@outlook.com)，尽我们所能为你解决问题。😀

## 尽自己一份力

要提出 Issue 或 Pull Request 之前，请先参考 [Issue 模板](./ISSUE_TEMPLATE.md) 和 [Pull Request 模板](./PULL_REQUEST_TEMPLATE.md)，请勿在 Issues 中添加无用的抱怨、无关的牢骚。

我们非常感谢那些协助翻译的人们，如果你同样想让极光音乐本地化，请移步 [翻译](https://aurorastudio.oneskyapp.com/collaboration/project?id=141901)。


## 待完成

* 高级扩展功能，如通过配置文件来修改界面布局，通过可选包扩展音频效果；
* 推荐系统的机器学习能力；
* 使用训练过的深度学习模型，对音乐库文件依据曲风分类；
* 电台；
* 以字为单位的歌词同步；
* Lastfm scrobbler。


## 开源信息

下面是本应用使用的开源库们：

| 大名 | 许可类型 | 项目网站 |
| --- | --- | --- |
| taglib-sharp | [GNU LGPL v2.1](https://github.com/mono/taglib-sharp/blob/master/COPYING) | [github.com/mono/taglib-sharp](https://github.com/mono/taglib-sharp) |
| SQLite for Universal Windows Platform | [Public Domain](http://www.sqlite.org/copyright.html) | [sqlite.org](http://www.sqlite.org/) |
| SQLite-net | [MIT License](https://github.com/praeclarum/sqlite-net/blob/master/LICENSE.md) | [github.com/praeclarum/sqlite-net](https://github.com/praeclarum/sqlite-net) |
| UWP Community Toolkit | [MIT License](https://github.com/Microsoft/UWPCommunityToolkit/blob/master/license.md) | [github.com/Microsoft/UWPCommunityToolkit ](https://github.com/Microsoft/UWPCommunityToolkit) |
| Json.NET | [MIT License](https://github.com/JamesNK/Newtonsoft.Json/blob/master/LICENSE.md) | [newtonsoft.com](https://www.newtonsoft.com/json) |
| NAudio | [MS-PL](https://github.com/naudio/NAudio/blob/master/license.txt) | [github.com/naudio/NAudio](https://github.com/naudio/NAudio) |
| UWPAudioVisualizer | [MIT License](https://github.com/clarkezone/audiovisualizer/blob/master/LICENSE) | [github.com/clarkezone/audiovisualizer](https://github.com/clarkezone/audiovisualizer) |
| ExpressionBuilder | [MIT License](https://github.com/Microsoft/WindowsUIDevLabs/blob/master/LICENSE.txt) | [github.com/Microsoft/ExpressionBuilder](https://github.com/Microsoft/WindowsCompositionSamples/tree/master/ExpressionBuilder) |
| ColorThief.NET | [MIT License](https://github.com/KSemenenko/ColorThief/blob/master/LICENSE) | [github.com/KSemenenko/ColorThief](https://github.com/KSemenenko/ColorThief) |
| SmartFormat | [MIT License](https://github.com/scottrippey/SmartFormat.NET/wiki/License) | [github.com/scottrippey/SmartFormat.NET](https://github.com/scottrippey/SmartFormat.NET) |
| WriteableBitmapEx | [MIT License](https://github.com/teichgraf/WriteableBitmapEx/blob/master/LICENSE) | [github.com/teichgraf/WriteableBitmapEx/](https://github.com/teichgraf/WriteableBitmapEx/) |
| Win2D | [MIT License](https://github.com/Microsoft/Win2D/blob/master/LICENSE.txt) | [github.com/Microsoft/Win2D](https://github.com/Microsoft/Win2D) |
| LrcParser | [MIT License](https://github.com/pkzxs/Aurora.Music/blob/master/LICENSE) | [github.com/pkzxs/LrcParser](https://github.com/pkzxs/Aurora.Music/tree/master/Source/LrcParser) |


[/Source/Taglib.Sharp](./Source/TagLib.Sharp/) 是从 [mono/taglib-sharp](https://github.com/mono/taglib-sharp) 移植到 UWP 平台的。

[/Source/ExpressionBuilder](./Source/ExpressionBuilder/) 来自于 [Microsoft/WindowsCompositionSamples](https://github.com/Microsoft/WindowsCompositionSamples)。
