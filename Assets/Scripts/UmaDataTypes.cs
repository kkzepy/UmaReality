using Gallop;
using System;
using System.Collections.Generic;
using UnityEngine;
using Path = System.IO.Path;

public enum UmaFileType
{
    _3d_cutt,
    announce,
    atlas,
    bg,
    chara,
    font,
    fontresources,
    gacha,
    gachaselect,
    guide,
    heroes,
    home,
    imageeffect,
    item,
    lipsync,
    live,
    loginbonus,
    manifest,
    manifest2,
    manifest3,
    mapevent,
    master,
    minigame,
    mob,
    movie,
    outgame,
    paddock,
    race,
    shader,
    single,
    sound,
    story,
    storyevent,
    supportcard,
    uianimation,
    transferevent,
    teambuilding,
    challengematch,
    collectevent,
    ratingrace,
    jobs,
    manualdownloadatlas,
}


public class CharaEntry
{
    public int Id;

    public int BirthYear;
    public int BirthMonth;
    public int BirthDay;
    public int LastYear;
    public int Sex;

    public int Height;
    public int Bust;
    public int Scale;
    public int Skin;
    public int Shape;
    public int Socks;
    public int TailModelId;
}
public class UmaDatabaseEntry
{
    public UmaFileType Type;
    public string Name;
    public string Url;
    public string Checksum;
    public string Prerequisites;
    public long Key;
    private byte[] fKey;
    public bool IsEncrypted => Key != 0;

    public byte[] FKey
    {
        get
        {
            if (fKey == null && IsEncrypted)
            {
                var baseKeys = Utility.HexStringToBytes(UmaDatabase.ABKey);
                int baseLen = baseKeys.Length;

                var keys = new byte[baseLen * 8];

                byte[] keyBytes = BitConverter.GetBytes(Key);
                if (!BitConverter.IsLittleEndian)
                {
                    Array.Reverse(keyBytes);
                }
                for (int i = 0; i < baseLen; ++i)
                {
                    byte b = baseKeys[i];
                    int baseOffset = i << 3; // i * 8
                    for (int j = 0; j < 8; ++j)
                    {
                        keys[baseOffset + j] = (byte)(b ^ keyBytes[j]);
                    }
                }

                fKey = keys;
            }
            return fKey;
        }
    }

    public string QueryPath()
    {
        return Path.Combine(UmaDatabase.persistentPath.Replace("\\", "/"), $"dat/{Url.Substring(0, 2)}/{Url}");
    }
}

public class CharacterData
{
    public string BasePrefab;
    public string BodyMesh;
    public string HeadMesh;
    public string HairMesh;
    public string TailMesh;

    public string[] Accessories;

    public string AnimatorController;

    public MaterialData Material;
}

public class MaterialData
{
    public string Albedo;
    public string Normal;
    public string Mask;
}

[System.Serializable]
public class EmotionKey
{
    public FacialMorph morph;
    public float weight;
}

[System.Serializable]
public class FaceTypeData
{
    public string label, eyebrow_l, eyebrow_r, eye_l, eye_r, mouth, inverce_face_type;
    public int mouth_shape_type, set_face_group;
    public FaceEmotionKeyTarget target;
    public List<EmotionKey> emotionKeys;
    public float weight;
}