using UnityEditor;

// Unity editor does not remember the following Android Player Publishing settings.
// Use this script if you don't want to re-enter the values.
[InitializeOnLoad]
public class AndroidPreloadSigning
{
    static AndroidPreloadSigning()
    {
        PlayerSettings.Android.keystorePass = "your password";
        PlayerSettings.Android.keyaliasName = "your keystore file";
        PlayerSettings.Android.keyaliasPass = "your password";
    }
}
