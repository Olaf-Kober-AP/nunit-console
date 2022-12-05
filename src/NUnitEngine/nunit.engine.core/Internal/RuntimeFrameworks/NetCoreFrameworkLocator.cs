﻿// Copyright (c) Charlie Poole, Rob Prouse and Contributors. MIT License - see LICENSE.txt

#if NETFRAMEWORK
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace NUnit.Engine.Internal.RuntimeFrameworks
{
    internal static class NetCoreFrameworkLocator
    {
        static Logger log = InternalTrace.GetLogger(typeof(NetCoreFrameworkLocator));

        public static IEnumerable<RuntimeFramework> FindDotNetCoreFrameworks()
        {
            List<Version> alreadyFound = new List<Version>();

            foreach (string dirName in GetRuntimeDirectories())
            {
                Version newVersion;
                if (TryGetVersionFromString(dirName, out newVersion) && !alreadyFound.Contains(newVersion))
                {
                    alreadyFound.Add(newVersion);
                    // HACK: Avoid Exception for an unknown version - see issue #1223
                    // Requires change in RuntimeFramework.GetClrVersionForFramework()
                    if (newVersion.Major <= 7)
                        yield return new RuntimeFramework(RuntimeType.NetCore, newVersion);
                    else
                        log.Error($"Found .NET {newVersion.ToString(2)}, which is not yet supported.");
                }
            }

            foreach (string line in GetRuntimeList())
            {
                Version newVersion;
                if (TryGetVersionFromString(line, out newVersion) && !alreadyFound.Contains(newVersion))
                {
                    alreadyFound.Add(newVersion);
                    yield return new RuntimeFramework(RuntimeType.NetCore, newVersion);
                }
            }
        }

        private static IEnumerable<string> GetRuntimeDirectories()
        {
            string installDir = GetDotNetInstallDirectory();

            if (installDir != null && Directory.Exists(installDir) &&
                File.Exists(Path.Combine(installDir, "dotnet.exe")))
            {
                string runtimeDir = Path.Combine(installDir, Path.Combine("shared", "Microsoft.NETCore.App"));
                if (Directory.Exists(runtimeDir))
                    foreach (var dir in new DirectoryInfo(runtimeDir).GetDirectories())
                        yield return dir.Name;
            }
        }

        private static IEnumerable<string> GetRuntimeList()
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = "--list-runtimes",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            try
            {
                process.Start();
            }
            catch(Exception)
            {
                // No versions are installed, just return
                yield break;
            }

            const string PREFIX = "Microsoft.NETCore.App ";
            const int VERSION_START = 22;

            while (!process.StandardOutput.EndOfStream)
            {
                var line = process.StandardOutput.ReadLine();
                if (line.StartsWith(PREFIX))
                    yield return line.Substring(VERSION_START);
            }
        }

        private static string GetDotNetInstallDirectory()
        {
            if (Path.DirectorySeparatorChar == '\\')
            {
                // Running on Windows so use registry
                RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\dotnet\SetUp\InstalledVersions\x64\sharedHost\");
                return (string)key?.GetValue("Path");
            }
            else
                return "/usr/shared/dotnet/";
        }

        private static bool TryGetVersionFromString(string text, out Version newVersion)
        {
            const string VERSION_CHARS = ".0123456789";

            int len = 0;
            foreach (char c in text)
            {
                if (VERSION_CHARS.IndexOf(c) >= 0)
                    len++;
                else
                    break;
            }

            try
            {
                var fullVersion = new Version(text.Substring(0, len));
                newVersion = new Version(fullVersion.Major, fullVersion.Minor);
                return true;
            }
            catch
            {
                newVersion = new Version();
                return false;
            }
        }
    }
}
#endif
