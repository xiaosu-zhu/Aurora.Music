# Aurora Music

[中文](https://github.com/pkzxs/Aurora.Music/blob/master/README_CN.md)

<p align="center">
<a href="https://www.microsoft.com/store/apps/9NBLGGH6JVDT?ocid=badge"><img width="200" src="https://i.loli.net/2017/12/30/5a479416604d9.png" alt="Logo" /></a></p>


<p align="center">
<a href="https://www.microsoft.com/store/apps/9NBLGGH6JVDT?ocid=badge"><img width="200" src="https://assets.windowsphone.com/85864462-9c82-451e-9355-a3d5f874397a/English_get-it-from-MS_InvariantCulture_Default.png" alt="Get it from Microsoft" /></a></p>

A small, lightweight UWP music player.

## Brief Introduction

Aurora Music has been one of four finalists in **Design Innovator** of [Windows Developer Awards 2018](https://developer.microsoft.com/en-us/windows/projects/events/build/2018/awards?utm_campaign=devawards18&utm_source=devcenter&utm_medium=owned&utm_content=hero)! 🎉

---

Aurora Music is built for a modern, fluent experience of listening music on Windows 10. It is MIT Licensed, you can fork it and build your own personalized music players.

Meanwhile, Aurora Music is extensible, we've preserved interface for lyrics, metadata and online musics. Building your extension can extend the ability of Aurora Music. We're also planning to support more advanced extensibility, such as audio effects, UI layouts, etc. Please see [To-Dos](https://github.com/pkzxs/Aurora.Music/blob/master/README.md#to-dos).

Here are a few screenshots:

(Background Image by [Jeremy Bishop on Unsplash](https://unsplash.com/photos/9pRjY4d7nJE), albums are owned by artists, only for demo here)

![](https://i.loli.net/2018/04/09/5acb122b411df.png "Home page")
![](https://i.loli.net/2018/04/09/5acb122acf565.png "Now playing")
![](https://i.loli.net/2018/04/09/5acb122b02ea3.png "Albums")
![](https://i.loli.net/2018/04/09/5acb122b86cf4.png "Album Details")
![](https://i.loli.net/2018/04/09/5acb122b22dc3.png "Songs")
![](https://i.loli.net/2018/04/09/5acb122b883cf.png "Artists")
![](https://i.loli.net/2018/04/09/5acb122b7d9ad.png "Douban FM")


## Documents and Samples

Please read instructions for extension development from **[here](./Documentation)**, inside there is a simple tutorial to let you make your own extension, as well as a detailed query explanation.

There's also a code sample **[here](./Samples)**, clone it and deploy, you can see what happens to lyrics.

Moreover, if you have any trouble using app, feel free to contact with [aurora.studio@outlook.com](mailto:aurora.studio@outlook.com).


## Contribution

To create an issue or pull request, please first have a look at [Issue Template](./ISSUE_TEMPLATE.md) and [Pull Request Template](./PULL_REQUEST_TEMPLATE.md), please don't pollute issues with unuseful complaints.

We also appreciated to translations added by contributors, if you're interested in localization, please have a look at [Translations](https://aurorastudio.oneskyapp.com/collaboration/project?id=141901).


## To-Dos

* Advance Extensibility, including UI layout extensibility with config files, and audio effects from optional packages.
* Using machine learning to generate mixed collections.
* Using trained models to classify music by genres.
* Radios.
* Lyric syncing word-by-word.
* Lastfm scrobbler.


## Open Source Info

You can explorer currently using open source libraries below:

| Name | License | Site |
| --- | --- | --- |
| taglib-sharp | [GNU LGPL v2.1](https://github.com/mono/taglib-sharp/blob/master/COPYING) | [github.com/mono/taglib-sharp](https://github.com/mono/taglib-sharp) |
| SQLite for Universal Windows Platform | [Public Domain](http://www.sqlite.org/copyright.html) | [sqlite.org](http://www.sqlite.org/) |
| SQLite-net | [MIT License](https://github.com/praeclarum/sqlite-net/blob/master/LICENSE.md) | [github.com/praeclarum/sqlite-net](https://github.com/praeclarum/sqlite-net) |
| UWP Community Toolkit | [MIT License](https://github.com/Microsoft/UWPCommunityToolkit/blob/master/license.md) | [github.com/Microsoft/UWPCommunityToolkit ](https://github.com/Microsoft/UWPCommunityToolkit) |
| Json.NET | [MIT License](https://github.com/JamesNK/Newtonsoft.Json/blob/master/LICENSE.md) | [newtonsoft.com](https://www.newtonsoft.com/json) |
| NAudio | [MS-PL](https://github.com/naudio/NAudio/blob/master/license.txt) | [github.com/naudio/NAudio](https://github.com/naudio/NAudio) |
| UWPAudioVisualizer | [MIT License](https://github.com/clarkezone/audiovisualizer/blob/master/LICENSE) | [github.com/clarkezone/audiovisualizer](https://github.com/clarkezone/audiovisualizer) |
| ExpressionBuilder | [MIT License](https://github.com/Microsoft/WindowsUIDevLabs/blob/master/LICENSE.txt) | [github.com/Microsoft/ExpressionBuilder](https://github.com/Microsoft/WindowsUIDevLabs/tree/master/ExpressionBuilder) |
| ColorThief.NET | [MIT License](https://github.com/KSemenenko/ColorThief/blob/master/LICENSE) | [github.com/KSemenenko/ColorThief](https://github.com/KSemenenko/ColorThief) |
| SmartFormat | [MIT License](https://github.com/scottrippey/SmartFormat.NET/wiki/License) | [github.com/scottrippey/SmartFormat.NET](https://github.com/scottrippey/SmartFormat.NET) |
| WriteableBitmapEx | [MIT License](https://github.com/teichgraf/WriteableBitmapEx/blob/master/LICENSE) | [github.com/teichgraf/WriteableBitmapEx/](https://github.com/teichgraf/WriteableBitmapEx/) |
| Win2D | [MIT License](https://github.com/Microsoft/Win2D/blob/master/LICENSE.txt) | [github.com/Microsoft/Win2D](https://github.com/Microsoft/Win2D) |
| LrcParser | [MIT License](https://github.com/pkzxs/Aurora.Music/blob/master/LICENSE) | [github.com/pkzxs/LrcParser](https://github.com/pkzxs/Aurora.Music/tree/master/Source/LrcParser) |


[/Source/Taglib.Sharp](./Source/TagLib.Sharp/) is ported to UWP from [mono/taglib-sharp](https://github.com/mono/taglib-sharp).

[/Source/ExpressionBuilder](./Source/ExpressionBuilder/) is from [Microsoft/WindowsUIDevLabs](https://github.com/Microsoft/WindowsUIDevLabs)

