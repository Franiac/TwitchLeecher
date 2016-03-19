# Twitch Leecher

If you are looking for a extremely fast and easy to use Twitch VOD downloader, this is your tool!

## What is different here compared to other VOD downloaders?

Nearly 95% of all quickly written VOD downloaders execute the download process via FFMPEG's integrated download capabilities. However, this is extremely slow. The download speed rarely exceeds 1.5Mbit even if the internet connection is 100 times faster. Twitch Leecher does not use FFMPEG for download tasks at all. It downloads thousands of small video chunks (usually ~500kb) in parallel while using all the available bandwidth of your internet connection. As soon as all video chunks are downloaded, FFMPEG is only used to glue those chunks together in order to create a single video again.

## Sounds cool, but are there any drawbacks?

All those small video chunks (*.ts files) need space on your harddrive (let's there are 11000 chunks that need 6GB). If all chunks are downloaded, FFMPEG can merge those chunks into a single *.mp4 file. This output file will have the size of the sum of all video chunks (6GB). FFMPEG cannot delete processed chunks on-the-fly while merging. This means, during the merge process, you will need 12GB of space on your harddrive for the 6GB video. After the merge process is complete and the *.mp4 file is created, all the video chunks are automatically deleted.

So what's better?

1. Waiting 10 hours for your VOD download
2. Temporarily sacrificing a little bit of cheap space on your harddrive <============== (!!!)

## Main Features

- Up to 20 times faster download speed compared to direct download with FFMPEG
- User friendly GUI
