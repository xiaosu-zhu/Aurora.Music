# Extension Development

Aurora Music have implemented the support for Extensions ([Extend your app with services, extensions, and packages][1], [Create and consume an app service][2]). Here's what extensions in Aurora Music can do:

 - Performe as a lyric provider
 - Performe as an online music provider
 - Performe as an online metadata provider

# How It Works
Just as what you see in the link above, Aurora Music uses `Windows.ApplicationModel.Extensions` namespace to retrieve installed extensions, and uses `Windows.ApplicationModel.AppService` to call a specific service associated with an extension. In UWP, `AppService` is just like web service, we send query strings to it and wait for a response. So, it is easy to implement an extension in the following steps.

  [1]: https://docs.microsoft.com/en-us/windows/uwp/launch-resume/extend-your-app-with-services-extensions-packages
  [2]: https://docs.microsoft.com/en-us/windows/uwp/launch-resume/how-to-create-and-consume-an-app-service
