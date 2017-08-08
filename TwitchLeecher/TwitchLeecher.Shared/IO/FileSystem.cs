using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;

namespace TwitchLeecher.Shared.IO
{
    public static class FileSystem
    {
        #region Methods

        public static void CleanDirectory(string directory)
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

        public static void CopyFile(string sourceFile, string targetDir, string newFileName = null)
        {
            CreateDirectory(targetDir);

            FileInfo fileInfo = new FileInfo(sourceFile);

            string targetFile = Path.Combine(targetDir, newFileName ?? fileInfo.Name);

            ResetFileAttributes(targetFile);

            File.Copy(fileInfo.FullName, targetFile, true);
        }

        public static void CreateDirectory(string directory)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(directory);

            if (!dirInfo.Exists)
            {
                dirInfo.Create();
            }
        }

        public static void DeleteDirectory(string directory)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(directory);

            if (dirInfo.Exists)
            {
                CleanDirectory(directory);
                dirInfo.Delete(true);
            }
        }

        public static void DeleteFile(string file)
        {
            FileInfo fileInfo = new FileInfo(file);

            if (fileInfo.Exists)
            {
                ResetFileAttributes(fileInfo.FullName);
                fileInfo.Delete();
            }
        }

        public static void ResetFileAttributes(string file)
        {
            if (File.Exists(file))
            {
                File.SetAttributes(file, FileAttributes.Normal);
            }
        }

        public static bool HasWritePermission(string dir)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dir) || !Directory.Exists(dir))
                {
                    return false;
                }

                DriveInfo driveInfo = new DriveInfo(Directory.GetDirectoryRoot(dir));

                // Only check local drives
                if (driveInfo.DriveType != DriveType.Fixed)
                {
                    return true;
                }

                DirectoryInfo dirInfo = new DirectoryInfo(dir);                

                DirectorySecurity dirSecurity = dirInfo.GetAccessControl();

                WindowsIdentity userIdentity = WindowsIdentity.GetCurrent();

                AuthorizationRuleCollection rules = dirSecurity.GetAccessRules(true, true, typeof(SecurityIdentifier));

                foreach (FileSystemAccessRule rule in rules)
                {
                    if (rule.FileSystemRights.HasFlag(FileSystemRights.WriteData & FileSystemRights.Delete))
                    {
                        if (userIdentity.User == rule.IdentityReference ||
                            userIdentity.Groups.Any(g => g == rule.IdentityReference))
                        {
                            return true;
                        }
                    }
                }
            }
            catch
            {
                // In case of an error -> Access denied
            }

            return false;
        }

        public static bool FilenameContainsInvalidChars(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
            {
                return false;
            }

            foreach (char c in Path.GetInvalidFileNameChars())
            {
                if (filename.Contains(c))
                {
                    return true;
                }
            }

            return false;
        }

        #endregion Methods
    }
}