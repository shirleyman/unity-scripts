using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace OrbitalNine.Editor
{
    public class AndroidUtils
    {
        private const int MENU_START_INDEX = 2000;
        private static readonly string ADB_PATH = Path.Combine(EditorPrefs.GetString("AndroidSdkRoot"), "platform-tools/adb");

        [MenuItem("Tools/Android/Build and Run", false, MENU_START_INDEX)]
        private static void BuildAndRun()
        {
            Build(false, true, false);
        }

        [MenuItem("Tools/Android/Build and Run (Debug)", false, MENU_START_INDEX + 1)]
        private static void BuildAndRunDebug()
        {
            Build(true, true, false);
        }

        [MenuItem("Tools/Android/Build and Profile", false, MENU_START_INDEX + 2)]
        private static void BuildAndProfile()
        {
            Build(true, true, true);
        }

        private static void Build(bool isDebug, bool runPlayer, bool attachProfiler)
        {
            string[] scenePaths = Utils.GetScenePaths();
            string apkPath = EditorUserBuildSettings.GetBuildLocation(BuildTarget.Android);
            if (string.IsNullOrEmpty(apkPath)) // also empty when directory doesn't exist anymore
            {
                string defaultBuildDir = Directory.GetParent(Application.dataPath).FullName;
                string defaultBuildFilename = Path.GetFileName(defaultBuildDir);
                apkPath = EditorUtility.SaveFilePanel("Save Android APK", defaultBuildDir, defaultBuildFilename, "apk");
            }
            if (!string.IsNullOrEmpty(apkPath))
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.Android);
                BuildPipeline.BuildPlayer(scenePaths, apkPath, BuildTarget.Android, Utils.GetBuildOptions(isDebug, false, runPlayer, attachProfiler)); 
            }
        }

        [MenuItem("Tools/Android/Deploy APK...", false, MENU_START_INDEX + 100)]
        private static void DeployAPK()
        {
            string lastAPK = EditorUserBuildSettings.GetBuildLocation(BuildTarget.Android);
            string apkPath = EditorUtility.OpenFilePanel("Select APK", lastAPK, "apk");
            if (!string.IsNullOrEmpty(apkPath))
            {
                RunProcess(ADB_PATH, "-d install -r " + apkPath);
            }
        }

        [MenuItem("Tools/Android/Re-run APK on Device", false, MENU_START_INDEX + 101)]
        public static void LaunchAndroid()
        {
            string appid = PlayerSettings.bundleIdentifier;
            RunProcess(ADB_PATH, "shell am force-stop " + appid).WaitForExit();
            RunProcess(ADB_PATH, "shell am start -n " + appid + "/com.unity3d.player.UnityPlayerNativeActivity");
        }

        [MenuItem("Tools/Android/Open Log Window...", false, MENU_START_INDEX + 200)]
        private static void LaunchADBLog()
        {
            RunProcess(ADB_PATH, "logcat -s Unity");
        }

        [MenuItem("Tools/Android/Open Log Window...", true)]
        private static bool EnableADBLog()
        {
            return Application.platform == RuntimePlatform.WindowsEditor;
        }

        private static System.Diagnostics.Process RunProcess(string fullPath, string arguments = "")
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
                Debug.LogError(e.Message);
            }
            return proc;
        }
    }
}
