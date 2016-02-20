using Microsoft.Deployment.WindowsInstaller;
using Microsoft.Win32;
using System;
using System.IO;

namespace TwitchLeecher.Setup.Custom
{
    public class CustomActions
    {
        [CustomAction]
        public static ActionResult DeleteProgramMenuFolderDeferred(Session session)
        {
            string logName = "Custom Action DeleteProgramMenuFolderDeferred: ";

            try
            {
                session.Log(logName + "Retrieving start menu folder");
                string startMenuFolder = session.CustomActionData["START_MENU_FOLDER"];
                session.Log(logName + "Start menu folder is '" + startMenuFolder + "'");

                if (string.IsNullOrWhiteSpace(startMenuFolder))
                {
                    throw new ArgumentNullException("START_MENU_FOLDER");
                }

                session.Log(logName + "Retrieving registry path");
                string registryPath = session.CustomActionData["TL_REGISTRY_PATH"];
                session.Log(logName + "Registry path is '" + registryPath + "'");

                if (string.IsNullOrWhiteSpace(registryPath))
                {
                    throw new ArgumentNullException("TL_REGISTRY_PATH");
                }

                if (Directory.Exists(startMenuFolder))
                {
                    session.Log(logName + "Removing start menu folder '" + startMenuFolder + "'");
                    Directory.Delete(startMenuFolder, true);
                }

                using (RegistryKey HKCU = GetRegistryHiveOnBit(RegistryHive.CurrentUser))
                {
                    try
                    {
                        session.Log(logName + "Removing HKCU key '" + registryPath + "'");
                        HKCU.DeleteSubKeyTree(registryPath);
                    }
                    catch
                    {
                        // Already deleted
                    }
                }

                using (RegistryKey HKLM = GetRegistryHiveOnBit(RegistryHive.LocalMachine))
                {
                    try
                    {
                        session.Log(logName + "Removing HKLM key '" + registryPath + "'");
                        HKLM.DeleteSubKeyTree(registryPath);
                    }
                    catch
                    {
                        // Already deleted
                    }
                }
            }
            catch (Exception ex)
            {
                session.Log(logName + ex.ToString());
            }

            return ActionResult.Success;
        }

        internal static RegistryKey GetRegistryHiveOnBit(RegistryHive registryHive)
        {
            if (Environment.Is64BitOperatingSystem)
            {
                return RegistryKey.OpenBaseKey(registryHive, RegistryView.Registry64);
            }
            else
            {
                return RegistryKey.OpenBaseKey(registryHive, RegistryView.Registry32);
            }
        }
    }
}