using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using Debug = UnityEngine.Debug;
public class UmaAssetManager
{
    public static string BodyPath = "3d/chara/body/";
    public static string MiniBodyPath = "3d/chara/mini/body/";
    public static string HeadPath = "3d/chara/head/";
    public static string TailPath = "3d/chara/tail/";
    public static string MotionPath = "3d/motion/";
    public static string CharaPath = "3d/chara/";
    public static string EffectPath = "3d/effect/";
    public static string CostumePath = "outgame/dress/";

    static List<string> genericCostumeIds = null;

    public static List<string> QueryAvailableCostumeId(int characterId)
    {
        List<string> result = new List<string>();

        foreach (var entry in UmaDatabaseController.MetaData)
        {
            var key = entry.Key;
            
            if (key.Contains("clothes")) { continue; }

            if (key.Contains($"pfb_bdy{characterId}"))
            {
                result.Add(key[^7..]);
            }
        }

        return result;
    }

    public static List<string> QueryAvailableGenericCostumeId(int characterId)
    {
        if (genericCostumeIds != null) //avoid continuously rescanning metadata
        {
            return genericCostumeIds;
        }

        List<string> result = new List<string>();

        foreach (var entry in UmaDatabaseController.MetaData)
        {
            var key = entry.Key;

            if (key.Contains("clothes")) { continue; }

            if (key.Contains($"pfb_bdy0") && key.StartsWith(BodyPath))
            {
                result.Add(key[^16..]); //add only the last 16 characters of the key
            }
        }

        genericCostumeIds = result;
        return result;
    }

    public static string QueryBodyPath(int characterId, int costumeId)
    {
        /*
        DataRow character = UmaDatabaseController.CharaData.FirstOrDefault(row => row.Field<long>("id") == characterId);

        if (character == null)
        {
            throw new Exception();
        }

        long height = character.Field<long>("height");
        long shape = character.Field<long>("shape");
        long bust = character.Field<long>("bust");
        */

        string _costumeId = costumeId.ToString();

        if (_costumeId.Length == 1)
        {
            _costumeId = "0"+_costumeId;
        }

        return $"{BodyPath}bdy{characterId}_{_costumeId}/pfb_bdy{characterId}_{_costumeId}";
    }

    public static string QueryHeadPath(int characterId, int headId)
    {
        string _headId = headId.ToString();

        if (_headId.Length == 1)
        {
            _headId = "0" + _headId;
        }

        return $"{HeadPath}chr{characterId}_{_headId}/pfb_chr{characterId}_{_headId}";
    }

    public static string QueryTailPath(int tailId)
    {
        string _tailId = tailId.ToString();

        if (_tailId.Length < 4)
        {
            _tailId = new string('0', 4 - _tailId.Length) + _tailId;
        }

        return $"{TailPath}tail{_tailId}_00/pfb_tail{_tailId}_00";
    }

    public static string ResolvePath(string logicalPath)
    {
        return UmaDatabaseController.MetaData[logicalPath].QueryPath();
    }
}

public static class AssetBundleDecryptor
{
    public static byte[] DecryptFileToBytes(string inputFilePath, byte[] Keys)
    {
        if (string.IsNullOrEmpty(inputFilePath))
            throw new ArgumentNullException(nameof(inputFilePath));
        if (!File.Exists(inputFilePath))
            throw new FileNotFoundException("Input file not found", inputFilePath);
        if (Keys == null)
            throw new ArgumentException("Keys must not be null or empty", nameof(Keys));

        byte[] data = File.ReadAllBytes(inputFilePath);

        if (data.Length <= 256)
            return data;

        for (int i = 256; i < data.Length; ++i)
        {
            data[i] ^= Keys[i % Keys.Length];
        }

        return data;
    }
}

public class UmaAssetLoader
{
    private Dictionary<string, AssetBundle> bundleCache = new Dictionary<string, AssetBundle>();

    // =========================
    // Load Bundle
    // =========================
    public AssetBundle LoadBundle(string Path)
    {
        if (bundleCache.ContainsKey(Path))
            return bundleCache[Path];

        AssetBundle bundle = AssetBundle.LoadFromFile(Path);

        if (bundle == null)
        {
            Debug.LogError($"Failed to load bundle: {Path}");
            return null;
        }

        bundleCache[Path] = bundle;
        return bundle;
    }

    // =========================
    // Load Prefab (Auto Detect)
    // =========================
    public GameObject LoadPrefab(string Path)
    {
        AssetBundle bundle = LoadBundle(Path);
        if (bundle == null) return null;

        // Cara aman: ambil semua lalu cari GameObject
        UnityEngine.Object[] assets = bundle.LoadAllAssets();

        foreach (var obj in assets)
        {
            if (obj is GameObject go)
                return go;
        }

        Debug.LogWarning($"No GameObject found in bundle: {Path}");
        return null;
    }

    // =========================
    // Load Specific Asset
    // =========================
    public T LoadAsset<T>(string Path, string AssetName) where T : UnityEngine.Object
    {
        AssetBundle bundle = LoadBundle(Path);
        if (bundle == null) return null;

        return bundle.LoadAsset<T>(AssetName);
    }

    // =========================
    // Unload
    // =========================
    public void Unload(string Path, bool UnloadAllLoadedObjects = false)
    {
        if (!bundleCache.ContainsKey(Path)) return;

        bundleCache[Path].Unload(UnloadAllLoadedObjects);
        bundleCache.Remove(Path);
    }

    public void UnloadAll()
    {
        foreach (var bundle in bundleCache.Values)
        {
            bundle.Unload(false);
        }

        bundleCache.Clear();
    }
}