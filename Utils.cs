﻿using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace OrbitalNine.Editor
{
    public class Utils
    {
        internal static string[] GetScenePaths()
        {
            return EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
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
