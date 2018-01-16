# Extension Sample

This sample introduces how to implement an extension for Aurora Music. A brief tutorial is announced [here](../Documentation/Extension%20Development.md).

This sample targets at `10.0.16299`, you can clone and build in the latest Visual Studio.

**The most important staffs in [Package.appxmanifest](./ExtensionSample/Package.appxmanifest):**

```xml
<Extensions>
    <uap:Extension Category="windows.appService">
        <uap:AppService Name="ExtensionSample.Service" />
    </uap:Extension>
    <uap3:Extension Category="windows.appExtension">
        <uap3:AppExtension Name="Aurora.Music.Extensions" Id="Sample" PublicFolder="Public" DisplayName="Extension Sample" Description="This is a sample for lyric extension">
            <uap3:Properties>
                <Service>ExtensionSample.Service</Service>
                <Category>Lyric</Category>
            </uap3:Properties>
        </uap3:AppExtension>
    </uap3:Extension>
</Extensions>
```

The code to support App Service is in [App.xaml.cs](./ExtensionSample/App.xaml.cs)

Deploy this app, and turn on the "Debug Mode switch" in Aurora Music->Settings->Advance

![Modify the Setting](https://i.loli.net/2017/11/26/5a19a11d5519b.png "Enable the Debug Mode")

And you will see sample appears in app:

![](https://i.loli.net/2018/01/16/5a5e0bf0115ed.png)
![](https://i.loli.net/2018/01/16/5a5e0bf0116d7.png)

Then choose any song to play, go to Now Playing, you can find it works well:
![](https://i.loli.net/2018/01/16/5a5e0bfd3d439.png)
