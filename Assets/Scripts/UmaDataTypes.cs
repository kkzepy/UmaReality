using UnityEngine;

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

}