using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace OrbitalNine.Editor
{
    public class AndroidUtils
    {
        private static readonly string ADB_PATH = Path.Combine(EditorPrefs.GetString("AndroidSdkRoot"), "platform-tools/adb");
        private static System.Diagnostics.Process sLogCatProc = null;
        const string apkPrefs = "APK Path";

        private static System.Diagnostics.Process RUN_PROCESS(string fullPath, string arguments = "")
        {
            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            try
            {
                proc.StartInfo.FileName = fullPath;
                proc.StartInfo.Arguments = arguments;
                proc.Start();
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
            return proc;
        }

        [MenuItem("Tools/Launch ADB Log...", false, 2001)]
        private static void LaunchADBLog()
        {
            if (sLogCatProc != null)
            {
                sLogCatProc.Kill();
            }
            sLogCatProc = new System.Diagnostics.Process();
            sLogCatProc.StartInfo.FileName = ADB_PATH;
            sLogCatProc.StartInfo.Arguments = "logcat -s Unity";
            sLogCatProc.Start();
        }

        private static string PromptForAPK()
        {
            string apkPath = EditorUtility.OpenFilePanel("Select APK", PlayerPrefs.GetString(apkPrefs), "apk");
            if (!string.IsNullOrEmpty(apkPath))
            {
                PlayerPrefs.SetString(apkPrefs, apkPath);
            }
            return apkPath;
        }

        [MenuItem("Tools/Deploy APK...", false, 2002)]
        private static void DeployAPK()
        {
            string apkPath = PromptForAPK();
            if (!string.IsNullOrEmpty(apkPath))
            {
                RUN_PROCESS(ADB_PATH, "-d install -r " + apkPath);
            }
        }

        [MenuItem("Tools/Run APK", false, 2003)]
        public static void LaunchAndroid()
        {
            string appid = PlayerSettings.bundleIdentifier;
            RUN_PROCESS(ADB_PATH, "shell am force-stop " + appid).WaitForExit();
            RUN_PROCESS(ADB_PATH, "shell am start -n " + appid + "/com.unity3d.player.UnityPlayerNativeActivity");
        }
    }
}
