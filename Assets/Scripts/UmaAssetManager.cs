using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Plastic.Newtonsoft.Json.Serialization;
using UnityEngine;
public class UmaAssetManager : MonoBehaviour
{
    public static string BodyPath = "3d/chara/body/";
    public static string MiniBodyPath = "3d/chara/mini/body/";
    public static string HeadPath = "3d/chara/head/";
    public static string TailPath = "3d/chara/tail/";
    public static string MotionPath = "3d/motion/";
    public static string CharaPath = "3d/chara/";
    public static string EffectPath = "3d/effect/";
    public static string CostumePath = "outgame/dress/";

    public static Shader HairShader, FaceShader, EyeShader, CheekShader, EyebrowShader, AlphaShader, BodyAlphaShader, BodyBehindAlphaShader = null;

    static List<string> genericCostumeIds = null;
    public static List<AssetBundle> loadedBundles = new List<AssetBundle>();
    public static Dictionary<string, MemoryStream> loadedAssets = new Dictionary<string, MemoryStream>();

    public static void LoadShaders()
    {
        string shaderLogicalPath = "shader";
        string shaderPath = UmaAssetManager.ResolvePath(shaderLogicalPath);

        using (var stream = new UmaAssetBundleStream(shaderPath, UmaDatabaseController.MetaData[shaderLogicalPath].FKey))
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
        }
    }

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
        return UmaDatabaseController.MetaData[logicalPath]?.QueryPath();
    }

    public static Texture2D LoadTexture2DAsset(string logicalPath)
    {
        string path = ResolvePath(logicalPath);
        using (var stream = new UmaAssetBundleStream(path, UmaDatabaseController.MetaData[logicalPath].FKey))
        {
            var bundle = AssetBundle.LoadFromStream(stream);
            var obj = bundle.LoadAllAssets<Texture2D>().FirstOrDefault();
            bundle.Unload(false); // penting!
            return obj;
        }
    }

    public static AnimationClip LoadAnim(string logicalPath)
    {
        string path = ResolvePath(logicalPath);
        using (var stream = new UmaAssetBundleStream(path, UmaDatabaseController.MetaData[logicalPath].FKey))
        {
            var bundle = AssetBundle.LoadFromStream(stream);
            var obj = bundle.LoadAllAssets<AnimationClip>().FirstOrDefault();
            bundle.Unload(false); // penting!
            return obj;
        }
    }

    public static void PreLoadPrerequistes(string logicalPath, bool recursive = true)
    {
        var prerequisities = UmaDatabaseController.MetaData[logicalPath].Prerequisites.Split(';');

        if (prerequisities.Count() == 0)
        {
            Debug.Log("No prerequisites to preload for " + logicalPath);
            return;
        }

        foreach (var prereq in prerequisities)
        {
            if (recursive && !string.IsNullOrEmpty(UmaDatabaseController.MetaData[prereq].Prerequisites))
            {
                PreLoadPrerequistes(prereq);
            }
            var stream = new UmaAssetBundleStream(ResolvePath(prereq), UmaDatabaseController.MetaData[prereq].FKey);
            var ms = new MemoryStream();
            stream.CopyTo(ms);

            loadedAssets[prereq] = ms;
        }
    }

    public static void LoadPrerequistes(string logicalPath, bool recursive = true)
    {
        var prerequisities = UmaDatabaseController.MetaData[logicalPath].Prerequisites.Split(';');

        if (prerequisities.Count() == 0) { 
            Debug.Log("No prerequisites to load for " + logicalPath);
            return;
        }

        foreach (var prereq in prerequisities)
        {
            if (recursive && !string.IsNullOrEmpty(UmaDatabaseController.MetaData[prereq].Prerequisites))
            {
                LoadPrerequistes(prereq);
            }
            
            if (loadedAssets.TryGetValue(prereq, out var stream))
            {
                AssetBundle.LoadFromStream(stream);
                loadedAssets.Remove(prereq);
            }
        }
    }
}

public class UmaAssetBundleStream : FileStream
{
    private const int headerSize = 256;
    private readonly byte[] baseKeys = Utility.HexStringToBytes(UmaDatabaseController.ABKey);
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
