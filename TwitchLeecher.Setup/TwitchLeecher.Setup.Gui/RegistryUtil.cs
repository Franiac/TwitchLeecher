using System;
using Microsoft.Win32;

namespace TwitchLeecher.Setup.Gui
{
    internal static class RegistryUtil
    {
        public static RegistryKey GetRegistryHiveOnBit(RegistryHive registryHive)
        {
            if (Environment.Is64BitOperatingSystem)
            {
                return RegistryKey.OpenBaseKey(registryHive, RegistryView.Registry64);
            }

            return RegistryKey.OpenBaseKey(registryHive, RegistryView.Registry32);
        }
    }
}