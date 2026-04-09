using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
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

        return $"{HeadPath}chr{characterId}_{_headId}/pfb_bdy{characterId}_{_headId}";
    }

    public static string QueryTailPath(int tailId)
    {
        return $"{TailPath}tail{tailId}_00/pfb_tail{tailId}_00";
    }

    public static string ResolvePath(string logicalPath)
    {
        return UmaDatabaseController.MetaData[logicalPath].QueryPath();
    }
}
