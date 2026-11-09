using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

/// <summary>
/// Single source of truth for the app's store identity (name, company, package id, version).
/// Tools > Homophones > Apply App Identity writes these into Player Settings.
/// </summary>
public static class AppIdentity
{
    public const string ProductName = "Word Echo";
    public const string CompanyName = "Lumipop Games";
    public const string PackageName = "com.lumipopgames.wordecho";
    public const string Version = "1.0.0";
    public const int VersionCode = 1;

    [MenuItem("Tools/Homophones/Apply App Identity")]
    public static void Apply()
    {
        PlayerSettings.productName = ProductName;
        PlayerSettings.companyName = CompanyName;
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, PackageName);
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.iOS, PackageName);
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Standalone, PackageName);
        PlayerSettings.bundleVersion = Version;
        PlayerSettings.Android.bundleVersionCode = VersionCode;

        // The old keystore belonged to the previous developer account and is gone.
        // A new upload keystore must be created for the new package (Player Settings > Publishing Settings).
        PlayerSettings.Android.useCustomKeystore = false;
        PlayerSettings.Android.keystoreName = "";
        PlayerSettings.Android.keyaliasName = "";

        AssetDatabase.SaveAssets();
        Debug.Log($"[AppIdentity] {ProductName} by {CompanyName} ({PackageName}) v{Version} ({VersionCode})");
    }
}
