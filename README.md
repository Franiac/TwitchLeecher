# Twitch Leecher

If you are looking for a extremely fast and easy to use Twitch VOD downloader, this is your tool!

## Download & Requirements

Twitch Leecher requires .NET Framework 4.5 or higher in order to run!

32 & 64 Bit Installers are available [HERE](https://github.com/Franiac/TwitchLeecher/releases)

Once installed, future releases will automatically update current installations with a single click!

## What is the difference compared to other VOD downloaders?

Nearly all of the well known VOD downloaders execute the download process via FFMPEG's integrated download capabilities. However, this is extremely slow. The download speed rarely exceeds 1.5Mbit even if the internet connection is 100 times faster. Twitch Leecher does not use FFMPEG for download tasks at all. It downloads thousands of small video chunks (usually ~500kb) in parallel while using all of the available bandwidth of your internet connection. As soon as all video chunks are downloaded, FFMPEG is only used to merge those chunks together in order to create a single video file again.

## Main Features

- Very easy to use, no manual needed
- Intuitive and stylish GUI
- Up to 20 times faster download speed compared to direct download with FFMPEG
- Browse your past broadcasts and highlights within the application
- Time Selection for VOD downloads
- Queue multiple downloads
- Specify default search parameters
- Specify default download folders
- Specify a filename template with wildcards for your downloads
- Developed by an experienced Software Engineer
- Free and Open Source

## LICENSE
[MIT License](https://github.com/Franiac/TwitchLeecher/blob/master/LICENSE)

## Screenshot

![Twitch Leecher Screenshot](http://www.fakesmilerevolution.com/files/fsr/twitchleecher/twitchleecher11.jpg)
