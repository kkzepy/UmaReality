using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using Debug = UnityEngine.Debug;
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
