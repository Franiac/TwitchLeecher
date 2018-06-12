using Microsoft.Tools.WindowsInstallerXml;
using System;
using System.Diagnostics;
using System.IO;

namespace WixAssemblyInfoExtension
{
    public class AssemlbyInfoPreprocessor : PreprocessorExtension
    {
        private readonly string[] _prefixes = { "AssemblyInfo" };

        public override string[] Prefixes
        {
            get
            {
                return _prefixes;
            }
        }

        public override string EvaluateFunction(string prefix, string function, string[] args)
        {
            switch (prefix)
            {
                case "AssemblyInfo":
                    if (args == null || args.Length == 0 || string.IsNullOrWhiteSpace(args[0]))
                    {
                        throw new ArgumentException("File name not specified!");
                    }

                    if (!File.Exists(args[0]))
                    {
                        throw new ArgumentException(string.Format("File '{0}' not found!", args[0]));
                    }

                    FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(args[0]);

                    switch (function)
                    {
                        case "Manufacturer":
                            return fileVersionInfo.CompanyName;

                        case "Product":
                            return fileVersionInfo.ProductName;

                        case "VersionSuffix":
                            return fileVersionInfo.Comments;

                        case "Version":
                            return Version.Parse(fileVersionInfo.FileVersion).ToString();

                        case "VersionPadded":
                            return Version.Parse(fileVersionInfo.FileVersion).Pad().ToString();

                        case "VersionTrimmed":
                            return Version.Parse(fileVersionInfo.FileVersion).Trim().ToString();

                        case "VersionMinor":
                            return Version.Parse(fileVersionInfo.FileVersion).ToString(2);

                        default:
                            throw new ArgumentException(string.Format("Function '{0}' not found!", function));
                    }

                default:
                    throw new ArgumentException(string.Format("Prefix '{0}' not found!", prefix));
            }
        }
    }
}