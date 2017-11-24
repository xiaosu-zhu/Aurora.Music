
# Extension Development

Aurora Music have implemented the support for Extensions (1: [Extend your app with services, extensions, and packages][1], 2: [Create and consume an app service][2]). Here's what extensions in Aurora Music can do:

 - Performe as a lyric provider
 - Performe as an online music provider
 - Performe as an online metadata provider

# How It Works
Just as what you see in the link above, Aurora Music uses `Windows.ApplicationModel.Extensions` namespace to retrieve installed extensions, and uses `Windows.ApplicationModel.AppService` to call a specific service associated with an extension. In UWP, `AppService` is just like web service, we send query strings to it and wait for a response. So, it is easy to implement an extension in the following steps:

 1. Complete the declaration of appxmanifest
 2. Write the code of your core service
 3. Implement Background Task and AppService Handler
 
 # Sample: A Simple Lyric Provider
Now, it's play time, You can follow these steps to create a basic extension!

## Modify the APPXManifest ##
To declare your app as an extension, you should add these lines to the `Package.appxmanifest`. First, you should check if it already included such namespaces at the first line:

    <Package xmlns="http://schemas.microsoft.com/appx/manifest/foundation/windows10"
        xmlns:mp="http://schemas.microsoft.com/appx/2014/phone/manifest"
        xmlns:uap="http://schemas.microsoft.com/appx/manifest/uap/windows10" 
        xmlns:uap3="http://schemas.microsoft.com/appx/manifest/uap/windows10/3" 
        xmlns:uap4="http://schemas.microsoft.com/appx/manifest/uap/windows10/4" 
        IgnorableNamespaces="uap mp uap3 uap4">

Then, you should declare this is an appExtension, under the Extension Node:

    <Extensions>
        <uap3:Extension Category="windows.appExtension">
          <uap3:AppExtension Name="Aurora.Music.Extensions" Id="BuiltIn" PublicFolder="Public" DisplayName="Lyric" Description="Aurora Music Lyric Provider">
            <uap3:Properties>
              <Service>Aurora.Music.Services</Service>
              <Category>Lyric</Category>
            </uap3:Properties>
          </uap3:AppExtension>
        </uap3:Extension>
    </Extensions>

NOTE: the use of `uap3:Properties` is explained in LINK TO ANOTHER, this is a declaration of your extension's features.

By using AppService, you should also declare it, this time you can add it in the manifest manager:





  [1]: https://docs.microsoft.com/en-us/windows/uwp/launch-resume/extend-your-app-with-services-extensions-packages
  [2]: https://docs.microsoft.com/en-us/windows/uwp/launch-resume/how-to-create-and-consume-an-app-service
