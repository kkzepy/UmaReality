using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
public class UmaAssetManager : MonoBehaviour
{
    public static Shader HairShader, FaceShader, EyeShader, CheekShader, EyebrowShader, AlphaShader, BodyAlphaShader, BodyBehindAlphaShader = null;

    static List<string> genericCostumeIds = null;
    public static Dictionary<string, AssetBundle> loadedAssets = new Dictionary<string, AssetBundle>();
    public static Dictionary<string, MemoryStream> prerequisitesQueue = new Dictionary<string, MemoryStream>();

    public static void LoadShaders()
    {
        string shaderLogicalPath = "shader";
        string shaderPath = UmaDatabase.ResolvePath(shaderLogicalPath);

        using (var stream = new UmaAssetBundleStream(shaderPath, UmaDatabase.MetaData[shaderLogicalPath].FKey))
        {
            var bundle = AssetBundle.LoadFromStream(stream);

            EyeShader = (Shader)bundle.LoadAsset("assets/_gallop/resources/shader/3d/character/charactertooneyet.shader");
            FaceShader = (Shader)bundle.LoadAsset("assets/_gallop/resources/shader/3d/character/charactertoonfacetser.shader");
            HairShader = (Shader)bundle.LoadAsset("assets/_gallop/resources/shader/3d/character/charactertoonhairtser.shader");
            AlphaShader = (Shader)bundle.LoadAsset("assets/_gallop/resources/shader/3d/character/characteralphanolinetoonhairtser.shader");
            CheekShader = (Shader)bundle.LoadAsset("assets/_gallop/resources/shader/3d/character/charactermultiplycheek.shader");
            EyebrowShader = (Shader)bundle.LoadAsset("assets/_gallop/resources/shader/3d/character/charactertoonmayu.shader");
            BodyAlphaShader = (Shader)bundle.LoadAsset("assets/_gallop/resources/shader/3d/character/characteralphanolinetoontser.shader");
            BodyBehindAlphaShader = (Shader)bundle.LoadAsset("assets/_gallop/resources/shader/3d/character/characteralphanolinetoonbehindtser.shader");

            bundle.Unload(false);
            stream.Close();
        }
    }

    public static List<string> QueryAvailableCostumeId(int characterId)
    {
        List<string> result = new List<string>();

        foreach (var entry in UmaDatabase.MetaData)
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

        foreach (var entry in UmaDatabase.MetaData)
        {
            var key = entry.Key;

            if (key.Contains("clothes")) { continue; }

            if (key.Contains($"pfb_bdy0") && key.StartsWith(UmaDatabase.BodyPath))
            {
                result.Add(key[^16..]); //add only the last 16 characters of the key
            }
        }

        genericCostumeIds = result;
        return result;
    }

    public static void PreLoadPrerequistes(string logicalPath, bool recursive = true)
    {
        var prerequisities = UmaDatabase.MetaData[logicalPath].Prerequisites.Split(';');

        if (prerequisities.Count() == 0)
        {
            Debug.Log("No prerequisites to preload for " + logicalPath);
            return;
        }

        foreach (var prereq in prerequisities)
        {
            if (recursive && !string.IsNullOrEmpty(UmaDatabase.MetaData[prereq].Prerequisites))
            {
                PreLoadPrerequistes(prereq);
            }
            var stream = new UmaAssetBundleStream(UmaDatabase.ResolvePath(prereq), UmaDatabase.MetaData[prereq].FKey);
            var ms = new MemoryStream();
            stream.CopyTo(ms);

            prerequisitesQueue[prereq] = ms;
        }
    }

    public static void LoadPrerequistes(string logicalPath, bool recursive = true)
    {
        var prerequisities = UmaDatabase.MetaData[logicalPath].Prerequisites.Split(';');

        if (prerequisities.Count() == 0) { 
            Debug.Log("No prerequisites to load for " + logicalPath);
            return;
        }

        foreach (var prereq in prerequisities)
        {
            if (loadedAssets.ContainsKey(prereq)) { continue; }

            if (recursive && !string.IsNullOrEmpty(UmaDatabase.MetaData[prereq].Prerequisites))
            {
                LoadPrerequistes(prereq);
            }
            
            if (prerequisitesQueue.TryGetValue(prereq, out var stream))
            {
                // Load and add asset bundle to loadedAssets list
                loadedAssets.Add(prereq, AssetBundle.LoadFromStream(stream));

                prerequisitesQueue.Remove(prereq);
            }
        }
    }

    public static T LoadAsset<T>(string logicalPath, bool loadPrerequisites = true, bool keepInMemory = false) where T : UnityEngine.Object
    {
        string path = UmaDatabase.ResolvePath(logicalPath);
        var fKey = UmaDatabase.MetaData[logicalPath].FKey;

        if (loadPrerequisites && !string.IsNullOrEmpty(UmaDatabase.MetaData[logicalPath].Prerequisites))
        {
            PreLoadPrerequistes(logicalPath);
            LoadPrerequistes(logicalPath);
        }

        using (var stream = new UmaAssetBundleStream(path, fKey))
        {
            //Prevents from loading multiple assets which Unity dont like
            AssetBundle bundle;
            if (loadedAssets.ContainsKey(logicalPath))
            {
                bundle = loadedAssets[logicalPath];
            }
            else
            {
                bundle = AssetBundle.LoadFromStream(stream);
                if (keepInMemory)
                {
                    loadedAssets[logicalPath] = bundle;
                }
            }

            //if (bundle == null) return null;

            T asset = bundle.LoadAllAssets<T>().FirstOrDefault();

            if (!keepInMemory)
            {
                bundle.Unload(false);
            }

            // Shits complicated lol

            stream.Close();
            return asset;
        }
    }

    public static T LoadSpecificAsset<T>(string logicalPath, string assetName, bool loadPrerequisites = true, bool keepInMemory = false) where T : UnityEngine.Object
    {
        string path = UmaDatabase.ResolvePath(logicalPath);
        var fKey = UmaDatabase.MetaData[logicalPath].FKey;

        if (loadPrerequisites && !string.IsNullOrEmpty(UmaDatabase.MetaData[logicalPath].Prerequisites))
        {
            PreLoadPrerequistes(logicalPath);
            LoadPrerequistes(logicalPath);
        }

        using (var stream = new UmaAssetBundleStream(path, fKey))
        {
            //Prevents from loading multiple assets which Unity dont like
            AssetBundle bundle;
            if (loadedAssets.ContainsKey(logicalPath))
            {
                bundle = loadedAssets[logicalPath];
            }
            else
            {
                bundle = AssetBundle.LoadFromStream(stream);
                if (keepInMemory)
                {
                    loadedAssets[logicalPath] = bundle;
                }
            }

            //if (bundle == null) return null;

            T asset = bundle.LoadAsset<T>(assetName);

            if (!keepInMemory)
            {
                bundle.Unload(false);
            }

            // Shits complicated lol

            stream.Close();
            return asset;
        }
    }
}

public class UmaAudio
{
    
}

public class UmaAssetBundleStream : FileStream
{
    private const int headerSize = 256;
    private readonly byte[] baseKeys = Utility.HexStringToBytes(UmaDatabase.ABKey);
    private readonly byte[] keys;

    public UmaAssetBundleStream(string filename, byte[] keys) : base(filename, FileMode.Open, FileAccess.Read)
    {
        this.keys = keys;
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        long StartPosition = Position;
        int res = base.Read(buffer, offset, count);

        for (long i = (StartPosition < headerSize ? headerSize - StartPosition : 0); i < count; i++)
        {
            buffer[i] ^= keys[(StartPosition + i) % (baseKeys.Length * 8)];
        }

        return res;
    }
}
