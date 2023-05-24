The is a new Maintained and developed Fork of [Twitch Leecher](https://github.com/Franiac/TwitchLeecher)
Don't forget to leve a star if you like it!

# Twitch Leecher
If you are looking for an extremely fast and easy to use Twitch VOD downloader, this is your tool!

## Is this project alive?
Yes! After the original project was discontinued, I decided to start maintaining and develop this Fork!
Note that this project is not my highest priority; I will keep is working, but new features might take some time

## Download & Requirements
- Requires Windows 7 SP1 64 Bit or higher
- Requires [.NET Framework 4.8](https://support.microsoft.com/en-us/topic/microsoft-net-framework-4-8-offline-installer-for-windows-9d23f658-3b97-68ab-d013-aa3c3e7495e0)

## What is the difference compared to other VOD downloaders?
Nearly all of the well known VOD downloaders execute the download process via FFMPEG's integrated download capabilities. However, this is extremely slow. The download speed rarely exceeds 1.5Mbit even if the internet connection is 100 times faster. Twitch Leecher does not use FFMPEG for download tasks at all. It downloads thousands of small video chunks in parallel while using all of the available bandwidth of your internet connection. As soon as all video chunks are downloaded, FFMPEG is only used to merge those chunks together in order to create a single video file again.

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
- 
## Support & Issues
**IMPORTANT:** Help me be efficient, please! I am developing Twitch Leecher in my free time for no money. Contribute to the project by posting complete, structured and helpful issues which I can reproduce quickly without asking for missing information. When creating a new issue please follow the below checklist:
- Windows Insider Builds are NOT supported!
- Take a look at the latest closed issues [HERE](https://github.com/Franiac/TwitchLeecher/issues?q=is%3Aissue+is%3Aclosed). Maybe your problem has already been resolved
- Provide the version of Twitch Leecher you are using
- Provide as much information about the VOD as possible (Url, Channel, ID)
- Provide information about your operating system (e.g. Windows 10 64 Bit)
- Try to describe the problem as detailed as possible, I cannot read your mind ;)
- Is there any additional information about the issue that might be interesting for me? Write it down!
- When you have a problem with a download, provide the download log created by Twitch Leecher (see screenshot below)
![Twitch Leecher Log Screenshot](http://www.fakesmilerevolution.com/files/fsr/twitchleecher/tl14log.jpg)

# Donate

If you wan't to support me, you might donate here: [Tipeeestream](https://www.tipeeestream.com/brainyxs/donation)

## LICENSE
[MIT License](https://github.com/Franiac/TwitchLeecher/blob/master/LICENSE)
