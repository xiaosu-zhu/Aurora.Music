# Something about build and deploy

System requirements:
* Windows 10 version 1709 or higher
* Visual Studio 2017 with Windows 10 SDK 10.0.16299
* Please enable dev mode in system settings





You may notice that, there're something missed in the solution:


1. **The `excluded.cs`**


![](https://i.loli.net/2018/10/05/5bb6bcaa7dc48.png)

This file contains following code:

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


One is for app diagnostic (Hockey App), and another is for Onedrive support (Microsoft Graph Service). You can register in the associated web site and require api keys, If you don't want so, just find all references in the whole solution and comment them.




2. **The Store Key and Temporary Key in the solution**

These files are created by Visual Studio for store upload, just ignore them.
