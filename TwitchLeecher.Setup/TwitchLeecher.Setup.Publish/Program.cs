using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace TwitchLeecher.Setup.Publish
{
    public class Program
    {
        public static int Main(string[] args)
        {
            try
            {
                if (args.Length == 0)
                {
                    throw new ArgumentException("Missing arguments in main method", "args");
                }

                string solutionDir = args[0];

                if (!Directory.Exists(solutionDir))
                {
                    throw new ApplicationException("SolutionDir '" + solutionDir + "' does not exist!");
                }

                Assembly exe = Assembly.LoadFile(Path.Combine(solutionDir, "..", "TwitchLeecher", "TwitchLeecher", "bin", "TwitchLeecher.exe"));

                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(exe.Location);

                Version version = new Version(fvi.FileVersion).Trim();

                string srcX86 = Path.Combine(solutionDir, "TwitchLeecher.Setup.Bootstrapper.x86", "bin", "TwitchLeecher_x86.exe");
                string srcX64 = Path.Combine(solutionDir, "TwitchLeecher.Setup.Bootstrapper.x64", "bin", "TwitchLeecher_x64.exe");

                string tgtFilenameX86 = "TwitchLeecher_" + version.ToString() + "_x86.exe";
                string tgtFilenameX64 = "TwitchLeecher_" + version.ToString() + "_x64.exe";

                string publishDir = Path.Combine(solutionDir, "..", "TwitchLeecher.Setup.Publish");

                CleanDirectory(publishDir);

                CopyFile(srcX86, publishDir, tgtFilenameX86);
                CopyFile(srcX64, publishDir, tgtFilenameX64);
            }
            catch
            {
                return -1;
            }

            return 0;
        }

        private static void ResetFileAttributes(string file)
        {
            if (File.Exists(file))
            {
                File.SetAttributes(file, FileAttributes.Normal);
            }
        }

        private static void DeleteFile(string file)
        {
            FileInfo fileInfo = new FileInfo(file);

            if (fileInfo.Exists)
            {
                ResetFileAttributes(fileInfo.FullName);
                fileInfo.Delete();
            }
        }

        private static void CreateDirectory(string directory)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(directory);

            if (!dirInfo.Exists)
            {
                dirInfo.Create();
            }
        }

        private static void DeleteDirectory(string directory)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(directory);

            if (dirInfo.Exists)
            {
                CleanDirectory(directory);
                dirInfo.Delete(true);
            }
        }

        private static void CleanDirectory(string directory)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(directory);

            if (dirInfo.Exists)
            {
                foreach (FileInfo file in dirInfo.GetFiles())
                {
                    DeleteFile(file.FullName);
                }

                foreach (DirectoryInfo dir in dirInfo.GetDirectories())
                {
                    DeleteDirectory(dir.FullName);
                }
            }
        }

        private static void CopyFile(string sourceFile, string targetDir, string newFileName = null)
        {
            CreateDirectory(targetDir);

            FileInfo fileInfo = new FileInfo(sourceFile);

            string targetFile = Path.Combine(targetDir, newFileName ?? fileInfo.Name);

            ResetFileAttributes(targetFile);

            File.Copy(fileInfo.FullName, targetFile, true);
        }
    }

    public static class Extensions
    {
        public static Version Trim(this Version version)
        {
            if (version.Build > 0 && version.Revision > 0)
            {
                return version;
            }
            else if (version.Build > 0)
            {
                return Version.Parse(version.ToString(3));
            }
            else
            {
                return Version.Parse(version.ToString(2));
            }
        }
    }
}