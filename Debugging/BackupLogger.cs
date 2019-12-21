//   BackupLogger.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2017 Allis Tauri
#if DEBUG
using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace AT_Utils
{
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    public class BackupLogger : MonoBehaviour
    {
        static BackupLogger instance;
        public static BackupLogger Instance { get { return instance; } }

        static string backup_file;
        static string logfile;
        string unity_log = "%HOME%/.config/unity3d/Squad/Kerbal Space Program/Player.log";

        public static void Log(string msg, params object[] args)
        {
            if(!string.IsNullOrEmpty(logfile))
                Utils.Log2File(logfile, msg, args);
        }

        public static void LogRaw(string msg)
        {
            if(!string.IsNullOrEmpty(logfile))
            {
                using(var f = new StreamWriter(logfile, true))
                    f.WriteLine(msg);
            }
        }

        void Awake()
        {
            if(instance != null)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            DontDestroyOnLoad(gameObject);
            //prepare log filenames
            var date = DateTime.Now.ToString("yyyy-MM-dd_HH-ss");
            backup_file = "Player-"+date+".log";
            logfile = "AT_Utils-Debug-"+date+".log";
            unity_log = Environment.ExpandEnvironmentVariables(unity_log);
            //start log backup routine
            if(File.Exists(unity_log))
                StartCoroutine(backup_log());
            else
                Log("Unity log is not here:\n{}", unity_log);
        }

        private void OnDestroy()
        {
            if(instance != this)
                return;
            instance = null;
        }

        IEnumerator<YieldInstruction> backup_log()
        {
            while(true)
            {
                try { File.Copy(unity_log, backup_file, true); }
                catch {}
                yield return new WaitForSeconds(1);
            }
        }
    }
}
#endif
