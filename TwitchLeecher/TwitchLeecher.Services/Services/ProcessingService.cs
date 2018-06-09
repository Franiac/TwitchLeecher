using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using TwitchLeecher.Core.Models;
using TwitchLeecher.Services.Interfaces;
using TwitchLeecher.Shared.IO;

namespace TwitchLeecher.Services.Services
{
    internal class ProcessingService : IProcessingService
    {
        #region Constants

        private const string FFMPEG_EXE_X86 = "ffmpeg_x86.exe";
        private const string FFMPEG_EXE_X64 = "ffmpeg_x64.exe";

        #endregion Constants

        #region Fields

        private readonly string _ffmpegExe;

        #endregion Fields

        #region Constructors

        public ProcessingService()
        {
            string appDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            _ffmpegExe = Path.Combine(appDir, Environment.Is64BitOperatingSystem ? FFMPEG_EXE_X64 : FFMPEG_EXE_X86);
        }

        #endregion Constructors

        #region Properties

        public string FFMPEGExe
        {
            get
            {
                return _ffmpegExe;
            }
        }

        #endregion Properties

        #region Methods

        public void ConcatParts(Action<string> log, Action<string> setStatus, Action<double> setProgress, VodPlaylist vodPlaylist, string concatFile)
        {
            setStatus("Merging files");
            setProgress(0);

            log(Environment.NewLine + Environment.NewLine + "Merging all VOD parts into '" + concatFile + "'...");

            using (FileStream outputStream = new FileStream(concatFile, FileMode.OpenOrCreate, FileAccess.Write))
            {
                int partsCount = vodPlaylist.Count;

                for (int i = 0; i < vodPlaylist.Count; i++)
                {
                    VodPlaylistPart part = vodPlaylist[i];

                    using (FileStream partStream = new FileStream(part.LocalFile, FileMode.Open, FileAccess.Read))
                    {
                        int maxBytes;
                        byte[] buffer = new byte[4096];

                        while ((maxBytes = partStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            outputStream.Write(buffer, 0, maxBytes);
                        }
                    }

                    FileSystem.DeleteFile(part.LocalFile);

                    setProgress(1 * 100 / partsCount);
                }
            }

            setProgress(100);
        }

        public void ConvertVideo(Action<string> log, Action<string> setStatus, Action<double> setProgress,
            Action<bool> setIsIndeterminate, string sourceFile, string outputFile, CropInfo cropInfo)
        {
            setStatus("Converting Video");
            setIsIndeterminate(true);

            log(Environment.NewLine + Environment.NewLine + "Executing '" + _ffmpegExe + "' on '" + sourceFile + "'...");

            ProcessStartInfo psi = new ProcessStartInfo(_ffmpegExe)
            {
                Arguments = "-y" + (cropInfo.CropStart ? " -ss " + cropInfo.Start.ToString(CultureInfo.InvariantCulture) : null) + " -i \"" + sourceFile + "\" -analyzeduration " + int.MaxValue + " -probesize " + int.MaxValue + " -c:v copy -c:a copy -bsf:a aac_adtstoasc" + (cropInfo.CropEnd ? " -t " + cropInfo.Length.ToString(CultureInfo.InvariantCulture) : null) + " \"" + outputFile + "\"",
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                StandardErrorEncoding = Encoding.UTF8,
                StandardOutputEncoding = Encoding.UTF8,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            log(Environment.NewLine + "Command line arguments: " + psi.Arguments + Environment.NewLine);

            using (Process p = new Process())
            {
                TimeSpan duration = TimeSpan.FromSeconds(cropInfo.Length);

                DataReceivedEventHandler outputDataReceived = new DataReceivedEventHandler((s, e) =>
                {
                    try
                    {
                        if (!string.IsNullOrWhiteSpace(e.Data))
                        {
                            string dataTrimmed = e.Data.Trim();

                            if (dataTrimmed.StartsWith("frame", StringComparison.OrdinalIgnoreCase) && duration != TimeSpan.Zero)
                            {
                                string timeStr = dataTrimmed.Substring(dataTrimmed.IndexOf("time") + 4).Trim();
                                timeStr = timeStr.Substring(timeStr.IndexOf("=") + 1).Trim();
                                timeStr = timeStr.Substring(0, timeStr.IndexOf(" ")).Trim();

                                if (TimeSpan.TryParse(timeStr, out TimeSpan current))
                                {
                                    setIsIndeterminate(false);
                                    setProgress(current.TotalMilliseconds / duration.TotalMilliseconds * 100);
                                }
                                else
                                {
                                    setIsIndeterminate(true);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        log(Environment.NewLine + "An error occured while reading '" + _ffmpegExe + "' output stream!" + Environment.NewLine + Environment.NewLine + ex.ToString());
                    }
                });

                p.OutputDataReceived += outputDataReceived;
                p.ErrorDataReceived += outputDataReceived;
                p.StartInfo = psi;
                p.Start();
                p.BeginErrorReadLine();
                p.BeginOutputReadLine();
                p.WaitForExit();

                if (p.ExitCode == 0)
                {
                    log(Environment.NewLine + "Video conversion complete!");
                }
                else
                {
                    throw new ApplicationException("An error occured while converting the video!");
                }
            }
        }

        #endregion Methods
    }
}