using UnityEngine;
#if UNITY_EDITOR && !UNITY_WEBPLAYER
using System.IO;
using System;
using UnityEditor;
#endif

///RageToolsPro-specific RageCamera properties and methods
public partial class RageCamera : MonoBehaviour {
    [ContextMenu("Toggle HOTween")]
    public void ToggleHotween() {
#if UNITY_EDITOR && !UNITY_WEBPLAYER
        Debug.Log("HOTween state toggled: Please wait for compilation to finish. Ignore warning messages below.");
        var rootFolder = Application.dataPath.Substring(0, Application.dataPath.Length - 6) ;    // Removing "Assets" from the end
        bool hotweenEnabled = !Directory.Exists(rootFolder + "HOTween");

        try {
            if (!hotweenEnabled) {
                FileUtil.MoveFileOrDirectory(rootFolder + "HOTween", "Assets/_ThirdParty/HOTween");
                FileUtil.MoveFileOrDirectory(rootFolder + "RageButton.cs", "Assets/_Freakow/RageToolsPro/Code/RageButton.cs");
                FileUtil.MoveFileOrDirectory(rootFolder + "RageButtonData.cs", "Assets/_Freakow/RageToolsPro/Code/_Data/RageButton/RageButtonData.cs");
                FileUtil.MoveFileOrDirectory(rootFolder + "IRageButton.cs", "Assets/_Freakow/RageToolsPro/Code/_Extensions/IRageButton.cs");
                FileUtil.MoveFileOrDirectory(rootFolder + "RageButtonEditor.boo", "Assets/_Freakow/RageToolsPro/Editor/RageButtonEditor.boo");
            } else {
                FileUtil.MoveFileOrDirectory("Assets/_ThirdParty/HOTween", rootFolder + "HOTween");
                FileUtil.MoveFileOrDirectory("Assets/_Freakow/RageToolsPro/Code/RageButton.cs", rootFolder + "RageButton.cs");
                FileUtil.MoveFileOrDirectory("Assets/_Freakow/RageToolsPro/Code/_Data/RageButton/RageButtonData.cs", rootFolder + "RageButtonData.cs");
                FileUtil.MoveFileOrDirectory("Assets/_Freakow/RageToolsPro/Code/_Extensions/IRageButton.cs", rootFolder + "IRageButton.cs");
                FileUtil.MoveFileOrDirectory("Assets/_Freakow/RageToolsPro/Editor/RageButtonEditor.boo", rootFolder + "RageButtonEditor.boo");
            }
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            ReimportRageCamera();
        } catch (Exception e) {
            Debug.LogWarning("RageTools Pro: File or Folder Operation Error. Make sure to close your MonoDevelop/IDE first and check if your folder and file permissions " +
                      "aren't set to read-only in your project folder. To resolve errors, you might need to manually move the HOTween folder now.");
            Debug.LogWarning("Error Code: " + e + "");
        }
#else
        Debug.LogWarning("HOTween toggle disabled in Webplayer mode, please temporarily switch to PC/Mac deploy option.");
#endif
    }

    /// <summary> Used to force the recompilation of the project </summary>
    private static void ReimportRageCamera() {
#if UNITY_EDITOR && !UNITY_WEBPLAYER
        var dataPathDir = new DirectoryInfo(Application.dataPath);
        var dataPathUri = new Uri(Application.dataPath);
        foreach (var file in dataPathDir.GetFiles("RageCamera.cs", SearchOption.AllDirectories)) {
            var relUri = dataPathUri.MakeRelativeUri(new Uri(file.FullName));
            var relPath = Uri.UnescapeDataString(relUri.ToString());
            AssetDatabase.ImportAsset(relPath, ImportAssetOptions.ForceUpdate);
        }
#endif
    }
}
