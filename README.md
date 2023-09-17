The is a new Maintained and developed Fork of [Twitch Leecher](https://github.com/Franiac/TwitchLeecher)
Don't forget to leve a star if you like it!
<p align="center">
  <img src="https://github.com/schneidermanuel/TwitchLeecher-Dx/assets/57318033/35f55b28-9970-4c95-89fb-01fac4ad5711" />
</p>

# Twitch Leecher-DX
If you are looking for an extremely fast and easy to use Twitch VOD downloader, this is your tool!

## Is this project alive?
Yes! After the original project was discontinued, I decided to start maintaining and develop this fork!

## Download & Requirements
- Requires Windows 7 SP1 64 Bit or higher
- Requires [.NET 6.0]([https://support.microsoft.com/en-us/topic/microsoft-net-framework-4-8-offline-installer-for-windows-9d23f658-3b97-68ab-d013-aa3c3e7495e0](https://dotnet.microsoft.com/en-us/download/dotnet/6.0))

## What is the difference compared to other VOD downloaders?
Nearly all of the well known VOD downloaders execute the download process via FFMPEG's integrated download capabilities. However, this is extremely slow. The download speed rarely exceeds 1.5Mbit even if the internet connection is 100 times faster. Twitch Leecher-DX does not use FFMPEG for download tasks at all. It downloads thousands of small video chunks in parallel while using all of the available bandwidth of your internet connection. As soon as all video chunks are downloaded, FFMPEG is only used to merge those chunks together in order to create a single video file again.

## Features
- Very easy to use, no manual needed
- Intuitive and stylish GUI
- Up to 20 times faster download speed compared to direct download with FFMPEG
- Browse your past broadcasts, uploads and highlights within the application
- Search channels, VOD urls and VOD IDs
- Sub-Only video download support
- Audio-Only download support
- Time Selection for VOD downloads
- Queue multiple downloads
- Specify default search parameters
- Specify default download quality
- Specify default download folders
- Specify a filename template with wildcards for your downloads
- Developed by an experienced Software Engineer
- Free and Open Source
- Actively maintained
- Modern .net 6.0 Framework
- Save favorite search parameter as preset
- Download VOD's that might are unavailable on twitch due to missing chunk data
  
## Support & Issues

If you have any problem or wish a feature for Twitch Leecher-DX, feel free to open an issue on this page!
> **IMPORTANT:** Help me be efficient, please! I am developing Twitch Leecher in my free time for no money. Contribute to the project by posting complete, structured and helpful issues which I can reproduce quickly without asking for missing information. When creating a new issue please follow the below checklist:

- Windows Insider Builds are NOT supported!
- Upgrade to the latest version of Twitch Leecher DX and .net 6.0 runtime
- Provide the version of Twitch Leecher-DX you are using
- Provide as much information about the VOD as possible (Url, Channel, ID)
- Provide information about your operating system (e.g. Windows 10 64 Bit)
- Try to describe the problem as detailed as possible, I cannot read your mind ;)
- Is there any additional information about the issue that might be interesting for me? Write it down!
- When you have a problem with a download, provide the download log created by Twitch Leecher (see screenshot below)
![2023-09-17 10 19 42](https://github.com/schneidermanuel/TwitchLeecher-Dx/assets/57318033/1472d989-4df9-44c6-9ccb-4519345d2234)

# Donate

If you wan't to support me and the development of this project, please consider donating! [Donate](https://www.tipeeestream.com/brainyxs/donation)

## LICENSE
[MIT License](https://github.com/schneidermanuel/TwitchLeecher-DX/blob/master/LICENSE)
