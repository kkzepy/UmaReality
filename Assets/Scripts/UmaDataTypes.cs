using System.IO;
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