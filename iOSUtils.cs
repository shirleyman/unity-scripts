using UnityEditor;
using UnityEngine;
using System.IO;

namespace OrbitalNine.Editor
{
    public class iOSUtils
    {
        private const int MENU_START_INDEX = 2000;

        [MenuItem("Tools/iOS/Build and Run (IL2CPP)", false, MENU_START_INDEX)]
        private static void BuildAndRunRelease()
        {
            Build(false, false, true);
        }

        [MenuItem("Tools/iOS/Build and Run (Mono Debug)", false, MENU_START_INDEX + 1)]
        private static void BuildAndRunMonoDebug()
        {
            Build(true, false, true);
        }

        [MenuItem("Tools/iOS/Generate XCode Project", false, MENU_START_INDEX + 100)]
        private static void GenerateXCodeProject()
        {
            Build(false, false, false);
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

        private static void Build(bool isDebug, bool isAppend, bool openInXcode, bool attachProfiler = false)
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
                if (!isDebug)
                {
                    buildOps |= BuildOptions.Il2CPP;
                }
                if (isAppend)
                {
                    buildOps |= BuildOptions.AcceptExternalModificationsToPlayer;
                }
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.iOS);
                BuildPipeline.BuildPlayer(scenePaths, projPath, BuildTarget.iOS, buildOps); 
            }
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