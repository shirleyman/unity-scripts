using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace OrbitalNine.Editor
{
    public class Utils
    {
        internal static string[] GetScenePaths()
        {
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            string[] scenePaths = new string[scenes.Length];
            for (int i = 0; i < scenePaths.Length; i++)
            {
                scenePaths[i] = scenes[i].path;
            }
            return scenePaths;
        }

        internal static BuildOptions GetBuildOptions(bool isDebug, bool showPlayer, bool runPlayer, bool attachProfiler)
        {
            BuildOptions buildOps = new BuildOptions();
            if (isDebug)
            {
                buildOps |= BuildOptions.Development;
                buildOps |= BuildOptions.AllowDebugging;
            }
            if (showPlayer)
            {
                buildOps |= BuildOptions.ShowBuiltPlayer;
            }
            if (runPlayer)
            {
                buildOps |= BuildOptions.AutoRunPlayer;
            }
            if (EditorUserBuildSettings.symlinkLibraries)
            {
                buildOps |= BuildOptions.SymlinkLibraries;
            }
            if (attachProfiler)
            {
                buildOps |= BuildOptions.ConnectWithProfiler;
            }
            return buildOps;
        }
    }
}
