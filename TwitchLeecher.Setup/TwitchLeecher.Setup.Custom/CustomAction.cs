using Microsoft.Deployment.WindowsInstaller;
using System;
using System.IO;

namespace TwitchLeecher.Setup.Custom
{
    public class CustomActions
    {
        [CustomAction]
        public static ActionResult DeleteUserDataDeferred(Session session)
        {
            string logName = "Custom Action DeleteUserDataDeferred: ";

            try
            {
                session.Log(logName + "Retrieving product name");
                string productName = session.CustomActionData["TL_PRODUCT_NAME"];
                session.Log(logName + "Product name is '" + productName + "'");

                string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), productName);

                if (Directory.Exists(appDataPath))
                {
                    session.Log(logName + "Deleting directory '" + appDataPath + "'!");
                    DeleteDirectory(appDataPath);
                }
                else
                {
                    session.Log(logName + "Path '" + appDataPath + "' does not exist! Nothing to delete!");
                }
            }
            catch (Exception ex)
            {
                session.Log(logName + ex.ToString());
            }

            return ActionResult.Success;
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

        private static void DeleteDirectory(string directory)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(directory);

            if (dirInfo.Exists)
            {
                CleanDirectory(directory);
                dirInfo.Delete(true);
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

        private static void ResetFileAttributes(string file)
        {
            if (File.Exists(file))
            {
                File.SetAttributes(file, System.IO.FileAttributes.Normal);
            }
        }
    }
}