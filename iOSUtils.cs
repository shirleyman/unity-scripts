using UnityEditor;
using UnityEngine;
using System.IO;
using System;

namespace OrbitalNine.Editor
{
    public class iOSUtils
    {
        private const int MENU_START_INDEX = 2000;

        [MenuItem("Tools/iOS/Build and Run (IL2CPP)", false, MENU_START_INDEX)]
        private static void BuildAndRunRelease()
        {
            Utils.ClearLog();
            System.Diagnostics.Stopwatch timer = System.Diagnostics.Stopwatch.StartNew();
            bool succeeded = Build(false, ScriptingImplementation.IL2CPP, false, true, false);
            timer.Stop();
            TimeSpan elapsed = timer.Elapsed;
            string result = succeeded ? "succeeded (" + string.Format("{0:D2}:{1:D2}:{2:D2}", elapsed.Hours, elapsed.Minutes, timer.Elapsed.Seconds) + ")." : "failed.";
            UnityEngine.Debug.Log("iOS build and run (IL2CPP release) " + result);
        }

        [MenuItem("Tools/iOS/Build and Run (Mono Debug)", false, MENU_START_INDEX + 1)]
        private static void BuildAndRunMonoDebug()
        { 
            Utils.ClearLog();
            System.Diagnostics.Stopwatch timer = System.Diagnostics.Stopwatch.StartNew();
            bool succeeded = Build(true, ScriptingImplementation.Mono2x, false, true, false);
            timer.Stop();
            TimeSpan elapsed = timer.Elapsed;
            string result = succeeded ? "succeeded (" + string.Format("{0:D2}:{1:D2}:{2:D2}", elapsed.Hours, elapsed.Minutes, timer.Elapsed.Seconds) + ")." : "failed.";
            UnityEngine.Debug.Log("iOS build and run (Mono debug) " + result);
        }

        [MenuItem("Tools/iOS/Generate XCode Project (IL2CPP)", false, MENU_START_INDEX + 100)]
        private static void GenerateXCodeProject()
        {
            Utils.ClearLog();
            System.Diagnostics.Stopwatch timer = System.Diagnostics.Stopwatch.StartNew();
            bool succeeded = Build(false, ScriptingImplementation.IL2CPP, false, false, false);
            timer.Stop();
            TimeSpan elapsed = timer.Elapsed;
            string result = succeeded ? "succeeded (" + string.Format("{0:D2}:{1:D2}:{2:D2}", elapsed.Hours, elapsed.Minutes, timer.Elapsed.Seconds) + ")." : "failed.";
            UnityEngine.Debug.Log("iOS Generate XCode Project " + result);
        }

        //        [MenuItem("Tools/iOS/Clear XCode Project Files", false, MENU_START_INDEX + 200)]
        private static void ClearProject()
        {
            string projPath = EditorUserBuildSettings.GetBuildLocation(BuildTarget.iOS);
            if (!string.IsNullOrEmpty(projPath))
            {
                foreach (string f in Directory.GetFiles(projPath))
                {
                    FileUtil.DeleteFileOrDirectory(f);
                }
                foreach (string d in Directory.GetDirectories(projPath))
                {
                    FileUtil.DeleteFileOrDirectory(d);
                }
            }
        }

        private static bool Build(bool isDebug, ScriptingImplementation scriptingBackend = ScriptingImplementation.IL2CPP, bool isAppend = false, bool openInXcode = true, bool attachProfiler = false)
        {
            string[] scenePaths = Utils.GetScenePaths();
            string projPath = EditorUserBuildSettings.GetBuildLocation(BuildTarget.iOS);
            if (string.IsNullOrEmpty(projPath)) // also empty when directory doesn't exist anymore
            {
                string defaultBuildDir = Directory.GetParent(Application.dataPath).FullName;
                string defaultBuildFilename = Path.GetFileName(defaultBuildDir);
                projPath = EditorUtility.SaveFilePanel("Save Xcode Project", defaultBuildDir, defaultBuildFilename, "");
            }
            if (!string.IsNullOrEmpty(projPath))
            {
                BuildOptions buildOps = Utils.GetBuildOptions(isDebug, !openInXcode, openInXcode, attachProfiler);
                if (isAppend)
                {
                    buildOps |= BuildOptions.AcceptExternalModificationsToPlayer;
                }
                if (scriptingBackend == ScriptingImplementation.IL2CPP)
                {
                    buildOps |= BuildOptions.Il2CPP;
                }
                PlayerSettings.SetPropertyInt("ScriptingBackend", (int)scriptingBackend, BuildTargetGroup.iOS);
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.iOS);
                string result = BuildPipeline.BuildPlayer(scenePaths, projPath, BuildTarget.iOS, buildOps); 
                if (string.IsNullOrEmpty(result))
                {
                    return true;
                }
                Debug.LogError(result);
            }
            return false;
        }

        [MenuItem("Tools/iOS/Build and Run (IL2CPP)", true)]
        [MenuItem("Tools/iOS/Build and Run (Mono Debug)", true)]
        [MenuItem("Tools/iOS/Generate XCode Project", true)]
        //        [MenuItem("Tools/iOS/Clear XCode Project Files", true)]
        private static bool IsEditorOSX()
        {
            return Application.platform == RuntimePlatform.OSXEditor;
        }

    }
}