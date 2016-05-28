using System;
using System.Diagnostics;
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
            Utils.ClearLog();
            Stopwatch timer = Stopwatch.StartNew();
            bool succeeded = (Build(false, true, false));
            timer.Stop();
            TimeSpan elapsed = timer.Elapsed;
            if (succeeded)
            {
                UnityEngine.Debug.Log("Android build and run succeeded(" + string.Format("{ 0:D2}:{ 1:D2}:{ 2:D2}", elapsed.Hours, elapsed.Minutes, timer.Elapsed.Seconds) + ").");
            }
            else
            {
                UnityEngine.Debug.LogError("Android build and run failed.");
            }
        }

        [MenuItem("Tools/Android/Build and Run (Debug)", false, MENU_START_INDEX + 1)]
        private static void BuildAndRunDebug()
        {
            Utils.ClearLog();
            Stopwatch timer = Stopwatch.StartNew();
            bool succeeded = Build(true, true, false);
            timer.Stop();
            TimeSpan elapsed = timer.Elapsed;
            if (succeeded)
            {
                UnityEngine.Debug.Log("Android debug build and run succeeded(" + string.Format("{ 0:D2}:{ 1:D2}:{ 2:D2}", elapsed.Hours, elapsed.Minutes, timer.Elapsed.Seconds) + ").");
            }
            else
            {
                UnityEngine.Debug.LogError("Android debug build and run failed.");
            }
        }

        [MenuItem("Tools/Android/Build and Profile", false, MENU_START_INDEX + 2)]
        private static void BuildAndProfile()
        {
            Utils.ClearLog();
            Stopwatch timer = Stopwatch.StartNew();
            bool succeeded = Build(true, true, true);
            timer.Stop();
            TimeSpan elapsed = timer.Elapsed;
            if (succeeded)
            {
                UnityEngine.Debug.Log("Android build and profile succeeded(" + string.Format("{ 0:D2}:{ 1:D2}:{ 2:D2}", elapsed.Hours, elapsed.Minutes, timer.Elapsed.Seconds) + ").");
            }
            else
            {
                UnityEngine.Debug.LogError("Android build and profile failed.");
            }
        }

        private static bool Build(bool isDebug, bool runPlayer, bool attachProfiler)
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
                string result = BuildPipeline.BuildPlayer(scenePaths, apkPath, BuildTarget.Android, Utils.GetBuildOptions(isDebug, false, runPlayer, attachProfiler));
                if (string.IsNullOrEmpty(result))
                {
                    if (runPlayer)
                    {
                        TurnScreenOn();
                    }
                    return true;
                }                
            }
            return false;
        }

        [MenuItem("Tools/Android/Deploy APK...", false, MENU_START_INDEX + 100)]
        private static void DeployAPK()
        {
            string lastAPK = EditorUserBuildSettings.GetBuildLocation(BuildTarget.Android);
            string apkPath = EditorUtility.OpenFilePanel("Select APK", lastAPK, "apk");
            if (!string.IsNullOrEmpty(apkPath))
            {
                Process proc = RunProcess(ADB_PATH, "-d install -r " + apkPath);
                string error = proc.StandardError.ReadToEnd();
                if (string.IsNullOrEmpty(error))
                {
                    TurnScreenOn();
                    UnityEngine.Debug.Log("APK file deployed.");
                }
                else
                {
                    UnityEngine.Debug.LogError(error);
                }
            }
        }

        [MenuItem("Tools/Android/Re-run APK on Device", false, MENU_START_INDEX + 101)]
        public static void LaunchAndroid()
        {
            string appid = PlayerSettings.bundleIdentifier;
            TurnScreenOn();
            Process proc = RunProcess(ADB_PATH, "shell am force-stop " + appid);
            proc.WaitForExit();
            string error = proc.StandardError.ReadToEnd();
            if (!string.IsNullOrEmpty(error))
            {
                UnityEngine.Debug.LogError(error);
            }
            else
            { 
                proc = RunProcess(ADB_PATH, "shell am start -n " + appid + "/com.unity3d.player.UnityPlayerNativeActivity");
                error = proc.StandardError.ReadToEnd();
                if (!string.IsNullOrEmpty(error))
                {
                    UnityEngine.Debug.LogError(error);
                }
                else
                {
                    UnityEngine.Debug.Log("APK launched on device.");
                }
            }
        }

        [MenuItem("Tools/Android/Open Log Window...", false, MENU_START_INDEX + 200)]
        private static void LaunchADBLog()
        {
            Process proc = RunProcess(ADB_PATH, "logcat -s Unity", true);
            string error = proc.StandardError.ReadToEnd();
            if (string.IsNullOrEmpty(error))
            {
                UnityEngine.Debug.Log("ADB logcat started");
            }
            else
            {
                UnityEngine.Debug.LogError(error);
            }
        }

        [MenuItem("Tools/Android/Open Log Window...", true)]
        private static bool EnableADBLog()
        {
            return Application.platform == RuntimePlatform.WindowsEditor;
        }

        [MenuItem("Tools/Android/Restart ADB Server", false, MENU_START_INDEX + 201)]
        private static void RestartADBServer()
        {
            Process proc = RunProcess(ADB_PATH, "kill-server");
            proc.WaitForExit();
            string error = proc.StandardError.ReadToEnd();
            if (string.IsNullOrEmpty(error))
            {
                proc = RunProcess(ADB_PATH, "start-server");
                if (string.IsNullOrEmpty(error))
                {
                    UnityEngine.Debug.Log("ADB server restarted.");
                }
                else
                {
                    UnityEngine.Debug.LogError(error);
                }
            }
            else
            {
                UnityEngine.Debug.LogError(error);
            }
        }

        private static Process RunProcess(string fullPath, string arguments = "", bool popupWindow = false)
        {
            Process proc = new Process();
            try
            {
                proc.StartInfo.FileName = fullPath;
                proc.StartInfo.Arguments = arguments;
                proc.StartInfo.UseShellExecute = popupWindow;
                proc.StartInfo.CreateNoWindow = !popupWindow;
                proc.StartInfo.RedirectStandardOutput = !popupWindow;
                proc.StartInfo.RedirectStandardError = !popupWindow;
                proc.Start();
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError(e.Message);
            }
            return proc;
        }

        private static void TurnScreenOn()
        {
            Process proc = RunProcess(ADB_PATH, "shell dumpsys input_method | grep mScreenOn");
            string output = proc.StandardOutput.ReadToEnd();
            if (output.Contains("mScreenOn=false"))
            {
                RunProcess(ADB_PATH, "shell input keyevent 26").WaitForExit(); // power button
            }
            RunProcess(ADB_PATH, "shell input keyevent 82"); // unlock screen
        }
    }
}
