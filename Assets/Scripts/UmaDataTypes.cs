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
    public string Name;
    public string EnName;
    public Sprite Icon;
    public int Id;
    public string ThemeColor;
    public bool IsMob;
    public string GetName()
    {
        return string.IsNullOrEmpty(EnName) ? Name : EnName;
    }
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
                var baseKeys = Utility.HexStringToBytes(UmaDatabaseController.ABKey);
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
        return Path.Combine(UmaDatabaseController.persistentPath.Replace("\\", "/"), $"dat/{Url.Substring(0, 2)}/{Url}");
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
