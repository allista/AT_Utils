﻿//   KSP_AVC_Updater.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2015 Allis Tauri

using System;
using System.IO;
using System.Reflection;

namespace AT_Utils
{
    public class KSP_AVC_Info
    {
        public string  Name;
        public Version ModVersion;
        public Version MinKSPVersion;
        public Version MaxKSPVersion;
        public string  VersionURL;
        public string  UpgradeURL;
        public string  ChangeLogURL;
        public string  PluginDir;
        public string  VersionFile { get { return Path.Combine(PluginDir, string.Format("{0}.version", Name)); } }

        public KSP_AVC_Info()
        {
            Name = Assembly.GetCallingAssembly().GetName().Name;
            ModVersion = Assembly.GetCallingAssembly().GetName().Version;
            MinKSPVersion = new Version(1,0,0);
            MaxKSPVersion = new Version(1,0,0);
            VersionURL    = "";
            UpgradeURL    = "";
            ChangeLogURL  = "";
            PluginDir     = Name;
        }
    }

    public static class KSP_AVC_Updater
    {
        public static void UpdateFor(params KSP_AVC_Info[] infos)
        {
            foreach(var info in infos)
            {
                using(var file = new StreamWriter(info.VersionFile))
                {
                    file.WriteLine(
                        @"{{ 
    ""NAME"":""{0}"",
    ""URL"":""{1}"",
    ""DOWNLOAD"":""{2}"",
    ""CHANGE_LOG_URL"":""{3}"",
    ""VERSION"":
     {{", info.Name, info.VersionURL, info.UpgradeURL, info.ChangeLogURL);
                    file.WriteLine("         \"MAJOR\":{0},", info.ModVersion.Major);
                    file.WriteLine("         \"MINOR\":{0},", info.ModVersion.Minor);
                    file.WriteLine("         \"PATCH\":{0},", info.ModVersion.Build);
                    file.WriteLine("         \"BUILD\":{0}",  info.ModVersion.Revision);
                    file.WriteLine(
                        @"     },
    ""KSP_VERSION_MIN"":
     {");
                    file.WriteLine("         \"MAJOR\":{0},", info.MinKSPVersion.Major);
                    file.WriteLine("         \"MINOR\":{0},", info.MinKSPVersion.Minor);
                    file.WriteLine("         \"PATCH\":{0}",  info.MinKSPVersion.Build);
                    file.WriteLine(
                        @"     },
    ""KSP_VERSION_MAX"":
     {");
                    file.WriteLine("         \"MAJOR\":{0},", info.MaxKSPVersion.Major);
                    file.WriteLine("         \"MINOR\":{0},", info.MaxKSPVersion.Minor);
                    file.WriteLine("         \"PATCH\":{0}",  info.MaxKSPVersion.Build);
                    file.WriteLine(
                        @"     }
}");
                }
            }
        }
    }
}

