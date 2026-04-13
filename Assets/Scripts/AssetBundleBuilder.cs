using UnityEditor;
using UnityEngine;

public class AssetBundleBuilder
{
    [MenuItem("Tools/Build AssetBundles")]
    public static void BuildAllAssetBundles()
    {
        string path = "Assets/AssetBundles";

        if (!System.IO.Directory.Exists(path))
        {
            System.IO.Directory.CreateDirectory(path);
        }

        BuildPipeline.BuildAssetBundles(
            path,
            BuildAssetBundleOptions.None,
            BuildTarget.StandaloneWindows // ganti sesuai target kamu
        );

        Debug.Log("AssetBundles rebuilt!");
    }
}