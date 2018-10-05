# Something about build and deploy

System requirement:
* Windows 10 version 1709 or higher
* Visual Studio 2017 with Windows 10 SDK 10.0.16299
* Please enable the development mode





You may notice that, there's something missed in the solution:
1. **The `excluded.cs`**


![](https://i.loli.net/2018/10/05/5bb6bcaa7dc48.png)

This is a file that contains following code:

``` csharp
namespace Aurora.Music.Core
{
    public static partial class Consts
    {
        public const string HockeyAppID = "XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX";
        public const string GraphServiceKey = "XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX";
    }
}

```


These two api keys is for app diagnostic (Hockey App), and Onedrive support (Microsoft Graph Service). You can register an ID for them, 
If you don't want so, just find all references in the whole solution and comment all lines.




2. **The Store Key and Temporary Key in the solution**

These files are created by Visual Studio for Windows Store packages, just ignore them.
