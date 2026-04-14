using UnityEngine;

public class TestButtonHandler : MonoBehaviour
{
    public static int id = 1032;
    public static string animLogicalPath = $"3d/motion/event/body/chara/chr{id}_00/anm_eve_chr{id}_00_idle01_loop";
    //public static string animPath = UmaAssetManager.ResolvePath(animLogicalPath);

    public void OnButtonClick()
    {
        //Debug.Log("The button was clicked!");
        
        Debug.Log(UmaDatabaseController.MetaData[animLogicalPath].Prerequisites);

        //var controller = Main.uma.GetComponent<UmaCharacter>();
        //controller.PlayAnimation(Main.clip);
    }
}