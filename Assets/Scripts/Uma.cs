using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UIElements;

public class UmaAssembler : MonoBehaviour
{
    public static GameObject CreateBody(int charaId, int costumeId)
    {
        var bodyLogicalPath = UmaAssetManager.QueryBodyPath(charaId, costumeId);
        var bodyPath = UmaAssetManager.ResolvePath(bodyLogicalPath);

        using (var stream = new UmaAssetBundleStream(bodyPath, UmaDatabaseController.MetaData[bodyLogicalPath].FKey))
        {
            var bundle = AssetBundle.LoadFromStream(stream);

            var body = bundle.LoadAllAssets<GameObject>().FirstOrDefault();

            bundle.Unload(false); // penting!
            return Instantiate(body);
        }
    }

    public static GameObject CreateHead(int charaId, int headId)
    {
        var headLogicalPath = UmaAssetManager.QueryHeadPath(charaId, headId);
        var headPath = UmaAssetManager.ResolvePath(headLogicalPath);
        using (var stream = new UmaAssetBundleStream(headPath, UmaDatabaseController.MetaData[headLogicalPath].FKey))
        {
            var bundle = AssetBundle.LoadFromStream(stream);
            var head = bundle.LoadAllAssets<GameObject>().FirstOrDefault();
            bundle.Unload(false); // penting!
            return Instantiate(head);
        }
    }

    public static GameObject CreateTail(int tailId)
    {
        var tailLogicalPath = UmaAssetManager.QueryTailPath(tailId);
        var tailPath = UmaAssetManager.ResolvePath(tailLogicalPath);
        using (var stream = new UmaAssetBundleStream(tailPath, UmaDatabaseController.MetaData[tailLogicalPath].FKey))
        {
            var bundle = AssetBundle.LoadFromStream(stream);
            var tail = bundle.LoadAllAssets<GameObject>().FirstOrDefault();
            bundle.Unload(false); // penting!
            return Instantiate(tail);
        }
    }

    static void MergeBone(SkinnedMeshRenderer from, Dictionary<string, Transform> targetBones, ref List<Transform> emptyBones)
    {
        if (targetBones.TryGetValue(from.rootBone.name, out Transform rootbone))
        {
            from.rootBone = rootbone;
            Transform[] tmpBone = new Transform[from.bones.Length];
            for (int i = 0; i < tmpBone.Length; i++)
            {
                if (targetBones.TryGetValue(from.bones[i].name, out Transform targetbone))
                {
                    tmpBone[i] = targetbone;
                    from.bones[i].position = targetbone.position;
                    while (from.bones[i].transform.childCount > 0)
                    {
                        from.bones[i].transform.GetChild(0).SetParent(targetbone);
                    }
                    emptyBones.Add(from.bones[i]);
                }
                else
                {
                    tmpBone[i] = from.bones[i];
                }
            }
            from.bones = tmpBone;
        }
    }

    public static GameObject Assemble(GameObject body, GameObject head, GameObject tail, string name = null)
    {
        GameObject rootObject = string.IsNullOrEmpty(name) ? new GameObject("uma_character") : new GameObject(name);
        Animator UmaAnimator;
        AnimatorOverrideController OverrideController;

        SkinnedMeshRenderer bodySkinnedMeshRenderer = body.GetComponentInChildren<SkinnedMeshRenderer>();
        var bodyBones = bodySkinnedMeshRenderer.bones.ToDictionary(bone => bone.name, bone => bone.transform);
        List<Transform> emptyBones = new List<Transform>();
        emptyBones.Add(body.transform.Find("Position/Hip/Tail_Ctrl"));
        while (body.transform.childCount > 0)
        {
            body.transform.GetChild(0).SetParent(rootObject.transform);
        }
        body.SetActive(false); //for debugging



        var headskins = head.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        foreach (SkinnedMeshRenderer headskin in headskins)
        {
            MergeBone(headskin, bodyBones, ref emptyBones);
        }
        var eyes = new GameObject("Eyes");
        eyes.transform.SetParent(rootObject.transform);
        while (head.transform.childCount > 0)
        {
            var child = head.transform.GetChild(0);
            child.SetParent(child.name.Contains("info") ? eyes.transform : rootObject.transform);
        }
        head.SetActive(false); //for debugging




        if (tail)
        {
            var tailskin = tail.GetComponentInChildren<SkinnedMeshRenderer>();
            MergeBone(tailskin, bodyBones, ref emptyBones);
            while (tail.transform.childCount > 0)
            {
                var child = tail.transform.GetChild(0);
                child.SetParent(rootObject.transform);
            }
            tail.SetActive(false); //for debugging
        }

        emptyBones.ForEach(a => { if (a) Destroy(a.gameObject); });

        /*
        //MergeAvatar
        UmaAnimator = rootObject.AddComponent<Animator>();
        UmaAnimator.avatar = AvatarBuilder.BuildGenericAvatar(rootObject, rootObject.name);
        OverrideController = Instantiate(UmaViewerBuilder.Instance.OverrideController);
        UmaAnimator.runtimeAnimatorController = OverrideController;
        */

        return rootObject;

    }

    public static void ApplyBodyTexture(GameObject body, int characterId, int costumeId)
    {
        /*
        string _costumeId = costumeId.ToString();
        string _skin = skin.ToString();

        if (_costumeId.Length < 4)
        {
            _costumeId = new string('0', 4 - _costumeId.Length) + _costumeId;
        }

        if (_skin.Length == 1)
        {
            _skin = "0" + _skin;
        }
        */

        string _costumeId = costumeId.ToString();

        if (_costumeId.Length == 1)
        {
            _costumeId = "0" + _costumeId;
        }

        foreach (Renderer r in body.GetComponentsInChildren<Renderer>())
        {

            var mainTex = default(Texture2D);
            var toonMap = default(Texture2D);
            var tripleMaskMap = default(Texture2D);
            var optionMaskMap = default(Texture2D);

            var mainTexLogicalPath = $"3d/chara/body/bdy{characterId}_{_costumeId}/textures/tex_bdy{characterId}_{_costumeId}_diff_wet";
            var mainTexPath = UmaAssetManager.ResolvePath(mainTexLogicalPath);
            using (var stream = new UmaAssetBundleStream(mainTexPath, UmaDatabaseController.MetaData[mainTexLogicalPath].FKey))
            {
                var bundle = AssetBundle.LoadFromStream(stream);
                mainTex = bundle.LoadAllAssets<Texture2D>().FirstOrDefault();
                bundle.Unload(false); // penting!
                //return Instantiate(tail);
            }

            var toonMapLogicalPath = $"3d/chara/body/bdy{characterId}_{_costumeId}/textures/tex_bdy{characterId}_{_costumeId}_shad_c_wet";
            var toonMapPath = UmaAssetManager.ResolvePath(toonMapLogicalPath);
            using (var stream = new UmaAssetBundleStream(toonMapPath, UmaDatabaseController.MetaData[toonMapLogicalPath].FKey))
            {
                var bundle = AssetBundle.LoadFromStream(stream);
                toonMap = bundle.LoadAllAssets<Texture2D>().FirstOrDefault();
                bundle.Unload(false); // penting!
                //return Instantiate(tail);
            }

            var tripleMaskMapLogicalPath = $"3d/chara/body/bdy{characterId}_{_costumeId}/textures/tex_bdy{characterId}_{_costumeId}_base_wet";
            var tripleMaskMapPath = UmaAssetManager.ResolvePath(tripleMaskMapLogicalPath);
            using (var stream = new UmaAssetBundleStream(tripleMaskMapPath, UmaDatabaseController.MetaData[tripleMaskMapLogicalPath].FKey))
            {
                var bundle = AssetBundle.LoadFromStream(stream);
                tripleMaskMap = bundle.LoadAllAssets<Texture2D>().FirstOrDefault();
                bundle.Unload(false); // penting!
                //return Instantiate(tail);
            }

            var optionMaskMapLogicalPath = $"3d/chara/body/bdy{characterId}_{_costumeId}/textures/tex_bdy{characterId}_{_costumeId}_ctrl_wet";
            var optionMaskMapPath = UmaAssetManager.ResolvePath(optionMaskMapLogicalPath);
            using (var stream = new UmaAssetBundleStream(optionMaskMapPath, UmaDatabaseController.MetaData[optionMaskMapLogicalPath].FKey))
            {
                var bundle = AssetBundle.LoadFromStream(stream);
                optionMaskMap = bundle.LoadAllAssets<Texture2D>().FirstOrDefault();
                bundle.Unload(false); // penting!
                //return Instantiate(tail);
            }

            r.material.SetTexture("_MainTex", mainTex);
            r.material.SetTexture("_ToonMap", toonMap);
            r.material.SetTexture("_TripleMaskMap", tripleMaskMap);
            r.material.SetTexture("_OptionMaskMap", optionMaskMap);
        }
    }

}