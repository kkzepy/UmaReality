using Gallop;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using UnityEngine;
using Random = UnityEngine.Random;

public class UmaAssembler : MonoBehaviour
{
    public static GameObject CreateBody(int charaId, int costumeId, bool loadPrerequisites = true, GameObject parent = null)
    {
        var bodyLogicalPath = UmaDatabase.QueryBodyPath(charaId, costumeId);
        var bodyPath = UmaDatabase.ResolvePath(bodyLogicalPath);

        if (bodyPath == null)
        {
            Debug.LogWarning($"No body found: {charaId} {costumeId}");
            return null;
        }

        using (var stream = new UmaAssetBundleStream(bodyPath, UmaDatabase.MetaData[bodyLogicalPath].FKey))
        {
            //Prevents from loading multiple assets which Unity dont like
            AssetBundle bundle;
            if (UmaAssetManager.loadedAssets.ContainsKey(bodyLogicalPath))
            {
                bundle = UmaAssetManager.loadedAssets[bodyLogicalPath];
            }
            else
            {
                bundle = AssetBundle.LoadFromStream(stream);
                UmaAssetManager.loadedAssets[bodyLogicalPath] = bundle;
            }

            var body = bundle.LoadAllAssets<GameObject>().FirstOrDefault();

            if (parent)
            {
                body = Instantiate(body, parent.transform);
            }
            else
            {
                body = Instantiate(body);
            }

            // Set shader
            foreach (Renderer r in body.GetComponentsInChildren<Renderer>())
            {
                foreach (Material m in r.sharedMaterials)
                {
                    if (m == null) continue;
                    //BodyAlapha's shader need to change manually.
                    if (m.name.Contains("bdy") && m.name.Contains("Alpha"))
                    {
                        m.shader = UmaAssetManager.BodyAlphaShader;
                    }
                }
            }

            stream.Close();
            return body;
        }
    }

    public static GameObject CreateGenericBody(int costumeId, int bodyTypeSub, int bodySetting, int height, int shape, int bust, bool loadPrerequisites = true)
    {
        string _costumeId = costumeId.ToString();

        if (_costumeId.Length < 4)
        {
            _costumeId = new string('0', 4 - _costumeId.Length) + _costumeId;
        }

        var bodyLogicalPath = UmaDatabase.BodyPath + $"bdy{_costumeId}_00/pfb_bdy{_costumeId}_0{bodyTypeSub}_0{bodySetting}_{height}_{shape}_{bust}";
        var bodyPath = UmaDatabase.ResolvePath(bodyLogicalPath);

        if (loadPrerequisites)
        {
            UmaAssetManager.PreLoadPrerequistes(bodyLogicalPath);
            UmaAssetManager.LoadPrerequistes(bodyLogicalPath);
        }

        using (var stream = new UmaAssetBundleStream(bodyPath, UmaDatabase.MetaData[bodyLogicalPath].FKey))
        {
            //Prevents from loading multiple assets which Unity dont like
            AssetBundle bundle;
            if (UmaAssetManager.loadedAssets.ContainsKey(bodyLogicalPath))
            {
                bundle = UmaAssetManager.loadedAssets[bodyLogicalPath];
            }
            else
            {
                bundle = AssetBundle.LoadFromStream(stream);
                UmaAssetManager.loadedAssets[bodyLogicalPath] = bundle;
            }

            var body = bundle.LoadAllAssets<GameObject>().FirstOrDefault();

            body = Instantiate(body);

            // Set shader
            foreach (Renderer r in body.GetComponentsInChildren<Renderer>())
            {
                foreach (Material m in r.sharedMaterials)
                {
                    if (m == null) continue;
                    //BodyAlapha's shader need to change manually.
                    if (m.name.Contains("bdy") && m.name.Contains("Alpha"))
                    {
                        m.shader = UmaAssetManager.BodyAlphaShader;
                    }
                }
            }

            stream.Close();
            return body;
        }
    }

    public static GameObject CreateHead(int charaId, int headId, bool loadPrerequisites = true, GameObject parent = null)
    {
        var headLogicalPath = UmaDatabase.QueryHeadPath(charaId, headId);
        var headPath = UmaDatabase.ResolvePath(headLogicalPath);

        if (headPath == null)
        {
            Debug.LogWarning($"No head found: {charaId} {headId}");
            return null;
        }

        using (var stream = new UmaAssetBundleStream(headPath, UmaDatabase.MetaData[headLogicalPath].FKey))
        {
            //Prevents from loading multiple assets which Unity dont like
            AssetBundle bundle;
            if (UmaAssetManager.loadedAssets.ContainsKey(headLogicalPath))
            {
                bundle = UmaAssetManager.loadedAssets[headLogicalPath];
            }
            else
            {
                bundle = AssetBundle.LoadFromStream(stream);
                UmaAssetManager.loadedAssets[headLogicalPath] = bundle;
            }

            var head = bundle.LoadAllAssets<GameObject>().FirstOrDefault();

            // Disable cheek temporarily, used for blush purpose
            var _cheekTransform = head.transform.Find("M_Cheek");
            if (_cheekTransform)
            {
                _cheekTransform.gameObject.SetActive(false);
            }

            foreach (Renderer r in head.GetComponentsInChildren<Renderer>())
            {

                foreach (Material m in r.sharedMaterials)
                {
                    if (m == null) continue;

                    if (r.name.Contains("Hair") && r.name.Contains("Alpha"))
                    {
                        m.shader = UmaAssetManager.AlphaShader;
                    }

                    switch (m.shader.name)
                    {
                        case "Gallop/3D/Chara/MultiplyCheek":
                            m.shader = UmaAssetManager.CheekShader; ;
                            break;
                        case "Gallop/3D/Chara/ToonFace/TSER":
                            m.shader = UmaAssetManager.FaceShader;
                            m.SetFloat("_CylinderBlend", 0.25f);
                            m.SetColor("_RimColor", new Color(0, 0, 0, 0));
                            break;
                        case "Gallop/3D/Chara/ToonEye/T":
                            m.shader = UmaAssetManager.EyeShader;
                            m.SetFloat("_CylinderBlend", 0.25f);
                            break;
                        case "Gallop/3D/Chara/ToonHair/TSER":
                            m.shader = UmaAssetManager.HairShader;
                            m.SetColor("_Color", Color.white);
                            m.SetFloat("_CylinderBlend", 0.25f);
                            break;
                        case "Gallop/3D/Chara/ToonMayu":
                            m.shader = UmaAssetManager.EyebrowShader;
                            m.renderQueue += 1;
                            break;
                        default:
                            Debug.Log(m.shader.name);
                            break;
                    }

                    m.SetFloat("_StencilMask", charaId);
                }
            }

            stream.Close();
            
            if (parent)
            {
                return Instantiate(head, parent.transform);
            }
            else
            {
                return Instantiate(head);
            }
        }
    }

    public static GameObject CreateTail(int tailId, bool loadPrerequisites = true, GameObject parent = null)
    {
        var tailLogicalPath = UmaDatabase.QueryTailPath(tailId);
        var tailPath = UmaDatabase.ResolvePath(tailLogicalPath);

        if (tailPath == null)
        {
            Debug.LogWarning($"No tail found: {tailId}");
            return null;
        }

        using (var stream = new UmaAssetBundleStream(tailPath, UmaDatabase.MetaData[tailLogicalPath].FKey))
        {
            //Prevents from loading multiple assets which Unity dont like
            AssetBundle bundle;
            if (UmaAssetManager.loadedAssets.ContainsKey(tailLogicalPath))
            {
                bundle = UmaAssetManager.loadedAssets[tailLogicalPath];
            }
            else
            {
                bundle = AssetBundle.LoadFromStream(stream);
                UmaAssetManager.loadedAssets[tailLogicalPath] = bundle;
            }

            var tail = bundle.LoadAllAssets<GameObject>().FirstOrDefault();

            stream.Close();

            if (parent)
            {
                return Instantiate(tail, parent.transform);
            }
            else
            {
                return Instantiate(tail);
            }
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

        SkinnedMeshRenderer bodySkinnedMeshRenderer = body.GetComponentInChildren<SkinnedMeshRenderer>();
        var bodyBones = bodySkinnedMeshRenderer.bones.ToDictionary(bone => bone.name, bone => bone.transform);
        List<Transform> emptyBones = new List<Transform>();
        emptyBones.Add(body.transform.Find("Position/Hip/Tail_Ctrl"));

        while (body.transform.childCount > 0)
        {
            body.transform.GetChild(0).SetParent(rootObject.transform);
        }
        //body.SetActive(false); //for debugging
        Destroy(body);

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
        //head.SetActive(false); //for debugging
        Destroy(head);

        if (tail)
        {
            var tailskin = tail.GetComponentInChildren<SkinnedMeshRenderer>();
            MergeBone(tailskin, bodyBones, ref emptyBones);
            while (tail.transform.childCount > 0)
            {
                var child = tail.transform.GetChild(0);
                child.SetParent(rootObject.transform);
            }
            //tail.SetActive(false); //for debugging
            Destroy(tail);
        }

        emptyBones.ForEach(a => { if (a) Destroy(a.gameObject); });

        var animator = rootObject.AddComponent<Animator>();
        animator.avatar = AvatarBuilder.BuildGenericAvatar(rootObject, rootObject.name);

        return rootObject;

    }

    public static GameObject AssembleToExistingRoot(GameObject body, GameObject head, GameObject tail, GameObject rootObject, string name = null)
    {
        //GameObject rootObject = root;//string.IsNullOrEmpty(name) ? new GameObject("uma_character") : new GameObject(name);

        SkinnedMeshRenderer bodySkinnedMeshRenderer = body.GetComponentInChildren<SkinnedMeshRenderer>();
        var bodyBones = bodySkinnedMeshRenderer.bones.ToDictionary(bone => bone.name, bone => bone.transform);
        List<Transform> emptyBones = new List<Transform>();
        emptyBones.Add(body.transform.Find("Position/Hip/Tail_Ctrl"));

        while (body.transform.childCount > 0)
        {
            body.transform.GetChild(0).SetParent(rootObject.transform);
        }
        body.SetActive(false); //for debugging
        //Destroy(body);

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
        //head.SetActive(false); //for debugging
        //Destroy(head);

        if (tail)
        {
            var tailskin = tail.GetComponentInChildren<SkinnedMeshRenderer>();
            MergeBone(tailskin, bodyBones, ref emptyBones);
            while (tail.transform.childCount > 0)
            {
                var child = tail.transform.GetChild(0);
                child.SetParent(rootObject.transform);
            }
            //tail.SetActive(false); //for debugging
            Destroy(tail);
        }

        emptyBones.ForEach(a => { if (a) Destroy(a.gameObject); });

        var animator = rootObject.AddComponent<Animator>();
        animator.avatar = AvatarBuilder.BuildGenericAvatar(rootObject, rootObject.name);

        return rootObject;

    }

    public static GameObject CreateUma(CharaEntry entry, int costumeId = 0, int headId = 0, bool addComponents = true)
    {
        GameObject root = new GameObject();
        root.name = $"uma_{entry.Id}";
        var umachar = root.AddComponent<UmaCharacter>();

        //int costumeId = 0;
        //int headId = 0;
        umachar.charaEntry = entry;
        umachar.costumeId = costumeId;
        umachar.headId = headId;
        umachar.FaceOverrideController = Resources.Load<AnimatorOverrideController>("Animations/Face Override Controller");
        umachar.UmaFaceAnimator = Resources.Load<Animator>("Animations/Face Controller");

        var bodyLogicalPath = UmaDatabase.QueryBodyPath(entry.Id, costumeId);
        var headLogicalPath = UmaDatabase.QueryHeadPath(entry.Id, headId);
        var tailLogicalPath = UmaDatabase.QueryTailPath(entry.TailModelId);

        UmaAssetManager.PreLoadPrerequistes(bodyLogicalPath);
        UmaAssetManager.PreLoadPrerequistes(headLogicalPath);
        if (entry.TailModelId != -1) UmaAssetManager.PreLoadPrerequistes(tailLogicalPath);

        UmaAssetManager.LoadPrerequistes(bodyLogicalPath);
        UmaAssetManager.LoadPrerequistes(headLogicalPath);
        if (entry.TailModelId != -1) UmaAssetManager.LoadPrerequistes(tailLogicalPath);

        umachar.bodyInstance = UmaAssembler.CreateBody(entry.Id, costumeId, false, root);
        umachar.headInstance = UmaAssembler.CreateHead(entry.Id, headId, false, root);
        if (entry.TailModelId != -1)
        {
            umachar.tailInstance = UmaAssembler.CreateTail(entry.TailModelId, false, root);
            UmaAssembler.ApplyTailTexture(umachar.tailInstance, entry.Id);
        }

        umachar.Initialize();
        umachar.LoadPhysics();
        umachar.SetupPhysics();
        umachar.InitializeFaceMorph();

        //umachar.FaceDrivenKeyTarget.ChangeMorphWeight(umachar.FaceDrivenKeyTarget.AllMorphs.Where(a => a.name == "Mouth_5_0").FirstOrDefault(), 1);

        root = UmaAssembler.AssembleToExistingRoot(umachar.bodyInstance, umachar.headInstance, umachar.tailInstance, root);

        //if (!umachar.UmaAnimator) umachar.UmaAnimator = root.AddComponent<Animator>();

        if (addComponents)
        {
            umachar.UmaAnimator = root.GetComponent<Animator>();
            umachar.UmaAnimator.avatar = AvatarBuilder.BuildGenericAvatar(root, root.name);
            umachar.OverrideController = Resources.Load<AnimatorOverrideController>("Animations/Override Controller");
            umachar.UmaAnimator.runtimeAnimatorController = umachar.OverrideController;
        }

        return root;
    }

    public static void ApplyBodyTexture(GameObject body, int characterId, int costumeId)
    {

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

            //var mainTexLogicalPath = $"{UmaAssetManager.BodyPath}bdy{characterId}_{_costumeId}/textures/tex_bdy{characterId}_{_costumeId}_diff_wet";
            var mainTexLogicalPath = "3d/chara/body/bdy0001_00/textures/offline/tex_bdy0001_00_00_1_2_00_diff";
            var toonMapLogicalPath = $"{UmaDatabase.BodyPath}bdy{characterId}_{_costumeId}/textures/tex_bdy{characterId}_{_costumeId}_shad_c_wet";
            var tripleMaskMapLogicalPath = $"{UmaDatabase.BodyPath}bdy{characterId}_{_costumeId}/textures/tex_bdy{characterId}_{_costumeId}_base_wet";
            var optionMaskMapLogicalPath = $"{UmaDatabase.BodyPath}bdy{characterId}_{_costumeId}/textures/tex_bdy{characterId}_{_costumeId}_ctrl_wet";

            mainTex = UmaAssetManager.LoadAsset<Texture2D>(mainTexLogicalPath);
            toonMap = UmaAssetManager.LoadAsset<Texture2D>(toonMapLogicalPath);
            tripleMaskMap = UmaAssetManager.LoadAsset<Texture2D>(tripleMaskMapLogicalPath);
            optionMaskMap = UmaAssetManager.LoadAsset<Texture2D>(optionMaskMapLogicalPath);

            r.material.SetTexture("_MainTex", mainTex);
            r.material.SetTexture("_ToonMap", toonMap);
            r.material.SetTexture("_TripleMaskMap", tripleMaskMap);
            r.material.SetTexture("_OptionMaskMap", optionMaskMap);
        }
    }

    public static void ApplyTailTexture(GameObject tail, int characterId, int tailId = 1)
    {

        string _tailId = tailId.ToString();

        if (_tailId.Length < 4)
        {
            _tailId = new string('0', 4 - _tailId.Length) + _tailId;
        }

        foreach (Renderer r in tail.GetComponentsInChildren<Renderer>())
        {

            var mainTex = default(Texture2D);
            var toonMap = default(Texture2D);
            var tripleMaskMap = default(Texture2D);
            var optionMaskMap = default(Texture2D);

            var mainTexLogicalPath = $"{UmaDatabase.TailPath}tail{_tailId}_00/textures/tex_tail{_tailId}_00_{characterId}_diff";
            var toonMapLogicalPath = $"{UmaDatabase.TailPath}tail{_tailId}_00/textures/tex_tail{_tailId}_00_{characterId}_shad_c";
            var tripleMaskMapLogicalPath = $"{UmaDatabase.TailPath}tail0001_00/textures/tex_tail0001_00_0000_base";//$"{UmaAssetManager.TailPath}tail{_tailId}_00/textures/tex_tail{_tailId}_00_{characterId}_base_wet";
            var optionMaskMapLogicalPath = $"{UmaDatabase.TailPath}tail0001_00/textures/tex_tail0001_00_0000_ctrl";//$"{UmaAssetManager.TailPath}tail{_tailId}_00/textures/tex_tail{_tailId}_00_{characterId}_ctrl_wet";
            
            try
            {
                mainTex = UmaAssetManager.LoadAsset<Texture2D>(mainTexLogicalPath, false);
                toonMap = UmaAssetManager.LoadAsset<Texture2D>(toonMapLogicalPath, false);
                tripleMaskMap = UmaAssetManager.LoadAsset<Texture2D>(tripleMaskMapLogicalPath, false);
                optionMaskMap = UmaAssetManager.LoadAsset<Texture2D>(optionMaskMapLogicalPath, false);
            }
            catch (KeyNotFoundException)
            {
                Debug.LogWarning($"No tail texture found for char {characterId}");
                return;
            }
            

            r.material.SetTexture("_MainTex", mainTex);
            r.material.SetTexture("_ToonMap", toonMap);
            r.material.SetTexture("_TripleMaskMap", tripleMaskMap);
            r.material.SetTexture("_OptionMaskMap", optionMaskMap);
        }
    }

    public static void ApplyHeadTexture(GameObject head, int characterId, int costumeId, bool wet = false)
    {
        var table = head.GetComponent<Gallop.AssetHolder>()._assetTable;

        //Texture CheekTex_0;
        //Texture CheekTex_1;

        string _costumeId = costumeId.ToString();

        if (_costumeId.Length == 1)
        {
            _costumeId = "0" + _costumeId;
        }

        var faceMainTex = default(Texture2D);
        var faceToonMap = default(Texture2D);
        var hairMainTex = default(Texture2D);
        var hairToonMap = default(Texture2D);
        var eyeMaterial = default(Texture2D);

        var faceMainTexLogicalPath = $"{UmaDatabase.HeadPath}chr{characterId}_{_costumeId}/textures/tex_chr{characterId}_{_costumeId}_face_diff_wet";
        var faceToonMapLogicalPath = $"{UmaDatabase.HeadPath}chr{characterId}_{_costumeId}/textures/tex_chr{characterId}_{_costumeId}_face_shad_c_wet";
        var hairMainTexLogicalPath = $"{UmaDatabase.HeadPath}chr{characterId}_{_costumeId}/textures/tex_chr{characterId}_{_costumeId}_hair_diff_wet";
        var hairToonMapLogicalPath = $"{UmaDatabase.HeadPath}chr{characterId}_{_costumeId}/textures/tex_chr{characterId}_{_costumeId}_hair_shad_c_wet";
        var eyeMaterialLogicalPath = $"sourceresources/3d/chara/head/chr{characterId}_{_costumeId}/materials/mtl_chr{characterId}_{_costumeId}_eye";

        faceMainTex = UmaAssetManager.LoadAsset<Texture2D>(faceMainTexLogicalPath, false);
        faceToonMap = UmaAssetManager.LoadAsset<Texture2D>(faceToonMapLogicalPath, false);
        hairMainTex = UmaAssetManager.LoadAsset<Texture2D>(hairMainTexLogicalPath, false);
        hairToonMap = UmaAssetManager.LoadAsset<Texture2D>(hairToonMapLogicalPath, false);
        eyeMaterial = UmaAssetManager.LoadAsset<Texture2D>(eyeMaterialLogicalPath, false);
        foreach (Renderer r in head.GetComponentsInChildren<Renderer>())
        {
            //Applying textures
            /*
            if (r.name.Contains("Cheek"))
            {
                r.gameObject.SetActive(false);
                
                
                CheekTex_0 = table["cheek0"] as Texture;
                CheekTex_1 = table["cheek1"] as Texture;
                
            }
            */
            if (r.name.Contains("Face"))
            {
                r.material.SetTexture("_MainTex", faceMainTex);
                r.material.SetTexture("_ToonMap", faceToonMap);
            }
            if (r.name.Contains("Hair"))
            {
                r.material.SetTexture("_MainTex", hairMainTex);
                r.material.SetTexture("_ToonMap", hairToonMap);
            }
            if (r.name.Contains("Eye"))
            {
                r.material.SetTexture("_MainTex", eyeMaterial);
                //r.material.SetTexture("_ToonMap", table["cheek1"] as Texture);
            }

            //Applying shaders
            foreach (Material m in r.materials)
            {

                switch (m.shader.name)
                {
                    case "Gallop/3D/Chara/MultiplyCheek":
                        m.shader = UmaAssetManager.CheekShader; ;
                        break;
                    case "Gallop/3D/Chara/ToonFace/TSER":
                        m.shader = UmaAssetManager.FaceShader;
                        m.SetFloat("_CylinderBlend", 0.25f);
                        m.SetColor("_RimColor", new Color(0, 0, 0, 0));
                        break;
                    case "Gallop/3D/Chara/ToonEye/T":
                        m.shader = UmaAssetManager.EyeShader;
                        m.SetFloat("_CylinderBlend", 0.25f);
                        break;
                    case "Gallop/3D/Chara/ToonHair/TSER":
                        m.shader = UmaAssetManager.HairShader;
                        m.SetFloat("_CylinderBlend", 0.25f);
                        break;
                    case "Gallop/3D/Chara/ToonMayu":
                        m.shader = UmaAssetManager.EyebrowShader;
                        m.renderQueue += 1; //fix eyebrows disappearing sometimes
                        break;
                    default:
                        Debug.Log(m.shader.name);
                        // m.shader = Shader.Find("Nars/UmaMusume/Body");
                        break;
                }
            }
        }
        //*/
    }

    private void SetMaskColor(Material mat, DataRow colordata, string prefix, bool hastoon)
    {
        mat.EnableKeyword("USE_MASK_COLOR");
        Color c1, c2, c3, c4, c5, c6, t1, t2, t3, t4, t5, t6;
        ColorUtility.TryParseHtmlString(colordata[$"{prefix}_color_r1"].ToString(), out c1);
        ColorUtility.TryParseHtmlString(colordata[$"{prefix}_color_r2"].ToString(), out c2);
        ColorUtility.TryParseHtmlString(colordata[$"{prefix}_color_g1"].ToString(), out c3);
        ColorUtility.TryParseHtmlString(colordata[$"{prefix}_color_g2"].ToString(), out c4);
        ColorUtility.TryParseHtmlString(colordata[$"{prefix}_color_b1"].ToString(), out c5);
        ColorUtility.TryParseHtmlString(colordata[$"{prefix}_color_b2"].ToString(), out c6);
        mat.SetColor("_MaskColorR1", c1);
        mat.SetColor("_MaskColorR2", c2);
        mat.SetColor("_MaskColorG1", c3);
        mat.SetColor("_MaskColorG2", c4);
        mat.SetColor("_MaskColorB1", c5);
        mat.SetColor("_MaskColorB2", c6);
        if (hastoon)
        {
            ColorUtility.TryParseHtmlString(colordata[$"{prefix}_toon_color_r1"].ToString(), out t1);
            ColorUtility.TryParseHtmlString(colordata[$"{prefix}_toon_color_r2"].ToString(), out t2);
            ColorUtility.TryParseHtmlString(colordata[$"{prefix}_toon_color_g1"].ToString(), out t3);
            ColorUtility.TryParseHtmlString(colordata[$"{prefix}_toon_color_g2"].ToString(), out t4);
            ColorUtility.TryParseHtmlString(colordata[$"{prefix}_toon_color_b1"].ToString(), out t5);
            ColorUtility.TryParseHtmlString(colordata[$"{prefix}_toon_color_b2"].ToString(), out t6);
            mat.SetColor("_MaskToonColorR1", t1);
            mat.SetColor("_MaskToonColorR2", t2);
            mat.SetColor("_MaskToonColorG1", t3);
            mat.SetColor("_MaskToonColorG2", t4);
            mat.SetColor("_MaskToonColorB1", t5);
            mat.SetColor("_MaskToonColorB2", t6);
        }
    }

    public static void ApplyFallbackShader(GameObject obj)
    {
        Shader fallback = Shader.Find("Standard");

        foreach (var r in obj.GetComponentsInChildren<Renderer>())
        {
            foreach (var mat in r.materials)
            {
                if (mat == null) continue;

                // simpan texture lama
                Texture mainTex = null;

                if (mat.HasProperty("_MainTex"))
                    mainTex = mat.GetTexture("_MainTex");

                else if (mat.HasProperty("_BaseMap"))
                    mainTex = mat.GetTexture("_BaseMap");

                else if (mat.HasProperty("_DiffuseMap")) // kemungkinan dari Gallop
                    mainTex = mat.GetTexture("_DiffuseMap");

                // ganti shader
                mat.shader = fallback;

                // apply ulang texture
                if (mainTex != null)
                    mat.SetTexture("_MainTex", mainTex);
            }
        }
    }
}

public class UmaCharacter : MonoBehaviour
{
    public CharaEntry charaEntry;
    public int costumeId = 0;
    public int headId = 0;
    string _headId;
    string _costumeId;
    string _tailId;

    public float randomBlinkMinInterval = 1f;
    public float randomBlinkMaxInterval = 7.5f;
    Coroutine randomBlinkCoroutine;

    public float randomEarTwitchMinInterval = 1f;
    public float randomEarTwitchMaxInterval = 8f;
    public int minEarMorphIndex = 0;
    public int maxEarMorphIndex = 31;
    Coroutine randomEarTwitchCoroutine;

    AssetHolder bodyAssetHolder;
    AssetHolder headAssetHolder;

    public GameObject headInstance;
    public GameObject bodyInstance;
    public GameObject tailInstance;

    GameObject upBodyBone;
    Vector3 upBodyPosition;
    Quaternion upBodyRotation;

    [Header("Animator")]
    public Animator UmaAnimator;
    public AnimatorOverrideController OverrideController;
    public Animator UmaFaceAnimator;
    public AnimatorOverrideController FaceOverrideController;
    public bool isAnimatorControl;

    [Header("Physics")]
    public bool EnablePhysics = true;
    public List<CySpringDataContainer> cySpringDataContainers;
    public GameObject PhysicsContainer;
    public float BodyScale = 1;

    public Texture2D CheekTex_0;
    public Texture2D CheekTex_1;
    public Texture2D CheekTex_2;

    public List<GameObject> LeftMangaObject = new List<GameObject>();
    public List<GameObject> RightMangaObject = new List<GameObject>();

    [Header("Tear")]
    public GameObject StaticTear_L;
    public GameObject StaticTear_R;
    public GameObject TearPrefab_0;
    public GameObject TearPrefab_1;
    public List<TearController> TearControllers = new List<TearController>();

    [Header("Face")]
    public FaceDrivenKeyTarget FaceDrivenKeyTarget;
    public FaceEmotionKeyTarget FaceEmotionKeyTarget;
    public FaceOverrideData FaceOverrideData;
    public GameObject HeadBone;
    public Transform TrackTarget;
    public float EyeHeight;
    public bool EnableEyeTracking = true;
    public Material FaceMaterial;
    public void Initialize()
    {
        _costumeId = costumeId.ToString();

        if (_costumeId.Length == 1)
        {
            _costumeId = "0" + _costumeId;
        }

        _headId = headId.ToString();

        if (_headId.Length == 1)
        {
            _headId = "0" + _headId;
        }

        _tailId = charaEntry.TailModelId.ToString();

        if (_tailId.Length < 4)
        {
            _tailId = new string('0', 4 - _tailId.Length) + _tailId;
        }

        bodyAssetHolder = bodyInstance.GetComponent<AssetHolder>();

        upBodyBone = bodyAssetHolder._assetTable["upbody_ctrl"] as GameObject;
        upBodyPosition = upBodyBone.transform.localPosition;
        upBodyRotation = upBodyBone.transform.localRotation;

        EyeHeight = headInstance.GetComponent<AssetHolder>()._assetTableValue["head_center_offset_y"];

        UmaAnimator = GetComponent<Animator>();
        if (UmaAnimator)
        {
            UmaAnimator.avatar = AvatarBuilder.BuildGenericAvatar(gameObject, gameObject.name);
            OverrideController = new AnimatorOverrideController(UmaAnimator.runtimeAnimatorController);
        }
    
        PhysicsContainer = new GameObject("PhysicsContainer");
        PhysicsContainer.transform.SetParent(transform);
    }

    public void InitializeFaceMorph()
    {
        GameObject locator;
        locator = Instantiate(UmaAssetManager.LoadAsset<GameObject>("3d/animator/drivenkeylocator", true, true), transform);
        locator.name = "DrivenKeyLocator";

        HeadBone = (GameObject)headInstance.GetComponent<AssetHolder>()._assetTable["head"];
        var eyeLocator_L = HeadBone.transform.Find("Eye_target_locator_L");
        var eyeLocator_R = HeadBone.transform.Find("Eye_target_locator_R");

        TrackTarget = new GameObject("TrackTarget").transform;
        TrackTarget.SetParent(transform);
        TrackTarget.position = HeadBone.transform.TransformPoint(0, 0, 10);

        foreach (var manga in new List<string> {
            "3d/effect/charaemotion/pfb_eff_chr_emo_eye_000",
            "3d/effect/charaemotion/pfb_eff_chr_emo_eye_001",
            "3d/effect/charaemotion/pfb_eff_chr_emo_eye_002",
            "3d/effect/charaemotion/pfb_eff_chr_emo_eye_003"
        })
        {
            GameObject obj = UmaAssetManager.LoadAsset<GameObject>(manga, true, true);
            obj.SetActive(false);

            GameObject leftObj = Instantiate(obj, eyeLocator_L.transform);
            new List<Renderer>(leftObj.GetComponentsInChildren<Renderer>(true)).ForEach(a => {
                a.material.SetFloat("_StencilMask", charaEntry.Id);
                a.material.SetFloat("_StencilComp", (float)UnityEngine.Rendering.CompareFunction.Equal);
                a.material.SetFloat("_StencilOp", (float)UnityEngine.Rendering.StencilOp.Keep);
                a.material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
            });
            LeftMangaObject.Add(leftObj);

            GameObject rightObj = Instantiate(obj, eyeLocator_R.transform);
            if (rightObj.TryGetComponent<AssetHolder>(out var holder))
            {
                if (holder._assetTableValue["invert"] > 0)
                    rightObj.transform.localScale = new Vector3(-1, 1, 1);
            }
            new List<Renderer>(rightObj.GetComponentsInChildren<Renderer>(true)).ForEach(a => {
                a.material.SetFloat("_StencilMask", charaEntry.Id);
                a.material.SetFloat("_StencilComp", (float)UnityEngine.Rendering.CompareFunction.Equal);
                a.material.SetFloat("_StencilOp", (float)UnityEngine.Rendering.StencilOp.Keep);
                a.material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
            });
            RightMangaObject.Add(rightObj);
        }

        foreach (var tear in new List<string> {
            "3d/chara/common/tear/tear000/pfb_chr_tear000",
            "3d/chara/common/tear/tear001/pfb_chr_tear001"
        })
        {
            LoadTear(tear);
        }

        if (TearPrefab_0 && TearPrefab_1)
        {
            var p0 = TearPrefab_0;
            var p1 = TearPrefab_1;
            var t = HeadBone.transform;
            TearControllers.Add(new TearController(charaEntry.Id, t, Instantiate(p0, t), Instantiate(p1, t), 0, 1));
            TearControllers.Add(new TearController(charaEntry.Id, t, Instantiate(p0, t), Instantiate(p1, t), 1, 1));
            TearControllers.Add(new TearController(charaEntry.Id, t, Instantiate(p0, t), Instantiate(p1, t), 0, 0));
            TearControllers.Add(new TearController(charaEntry.Id, t, Instantiate(p0, t), Instantiate(p1, t), 1, 0));
        }

        var firsehead = headInstance;
        var faceDriven = Instantiate(firsehead.GetComponent<AssetHolder>()._assetTable["facial_target"]) as FaceDrivenKeyTarget;

        //Need Instantiate or not?
        var earDriven = firsehead.GetComponent<AssetHolder>()._assetTable["ear_target"] as DrivenKeyTarget;
        var faceOverride = firsehead.GetComponent<AssetHolder>()._assetTable["face_override"] as FaceOverrideData;

        //faceDriven.Container = this;
        faceDriven._earTarget = earDriven._targetFaces;
        FaceDrivenKeyTarget = faceDriven;
        FaceDrivenKeyTarget.Container = this;
        FaceOverrideData = faceOverride;
        //faceOverride?.SetEnable(UI.ModelSettings.EnableFaceOverride);
        faceDriven.DrivenKeyLocator = locator.transform;
        faceDriven.Initialize(Utility.ConvertArrayToDictionary(firsehead.GetComponentsInChildren<Transform>()));

        var emotionDriven = ScriptableObject.CreateInstance<FaceEmotionKeyTarget>();
        emotionDriven.name = $"char{charaEntry.Id}_{_costumeId}_emotion_target";
        FaceEmotionKeyTarget = emotionDriven;
        emotionDriven.FaceDrivenKeyTarget = faceDriven;
        emotionDriven.FaceEmotionKey = UmaDatabase.FaceTypeData;
        emotionDriven.Initialize();
    }

    void LoadTear(string entry)
    {
        GameObject go = UmaAssetManager.LoadAsset<GameObject>(entry);
        if (go.name.EndsWith("000"))
        {
            TearPrefab_0 = go;
        }
        else if (go.name.EndsWith("001"))
        {
            TearPrefab_1 = go;
        }
    }
    public void UpBodyReset()
    {
        if (upBodyBone)
        {
            upBodyBone.transform.localPosition = upBodyPosition;
            upBodyBone.transform.localRotation = upBodyRotation;
            //Debug.Log("UpBody reset to default position and rotation.");
        }
        else
        {
            Debug.LogWarning("UpBody bone not found!");
        }
    }
    public void LoadPhysics()
    {
        // Prevents null reference exception when loading physics before setting costumeId
        if (_costumeId == null)
        {
            _costumeId = costumeId.ToString();

            if (_costumeId.Length == 1)
            {
                _costumeId = "0" + _costumeId;
            }
        }

        // Physics instantiating
        string clothesLogicalPath = UmaDatabase.BodyPath + $"bdy{charaEntry.Id}_{_costumeId}/clothes/pfb_bdy{charaEntry.Id}_{_costumeId}_cloth00";
        string bustClothesLogicalPath = UmaDatabase.BodyPath + $"bdy{charaEntry.Id}_{_costumeId}/clothes/pfb_bdy{charaEntry.Id}_{_costumeId}_bust_cloth00";
        string tailClothesLogicalPath = "";
        if (charaEntry.TailModelId != -1) tailClothesLogicalPath = UmaDatabase.TailPath + $"tail{_tailId}_00/clothes/pfb_tail{_tailId}_00_cloth00";
        string headClothesLogicalPath = UmaDatabase.HeadPath + $"chr{charaEntry.Id}_{_headId}/clothes/pfb_chr{charaEntry.Id}_{_headId}_cloth00";

        //Debug.Log($"{clothesLogicalPath}\n{bustClothesLogicalPath}\n{tailClothesLogicalPath}\n{headClothesLogicalPath}");

        Instantiate(UmaAssetManager.LoadAsset<GameObject>(clothesLogicalPath, false), PhysicsContainer.transform);
        Instantiate(UmaAssetManager.LoadAsset<GameObject>(bustClothesLogicalPath, false), PhysicsContainer.transform);
        if (charaEntry.TailModelId != -1) Instantiate(UmaAssetManager.LoadAsset<GameObject>(tailClothesLogicalPath, false), PhysicsContainer.transform);
        Instantiate(UmaAssetManager.LoadAsset<GameObject>(headClothesLogicalPath, false), PhysicsContainer.transform);
    }
    public void SetupPhysics()
    {
        // Initial physics setup
        cySpringDataContainers = new List<CySpringDataContainer>(PhysicsContainer.GetComponentsInChildren<CySpringDataContainer>());
        var bones = new Dictionary<string, Transform>();
        foreach (var bone in GetComponentsInChildren<Transform>())
        {
            if (!bones.ContainsKey(bone.name))
                bones.Add(bone.name, bone);
        }

        var colliders = new Dictionary<string, Transform>();

        for (int i = 0; i < cySpringDataContainers.Count; i++)
        {
            colliders = Utility.MergeDictionaries(colliders, cySpringDataContainers[i].InitiallizeCollider(bones));
        }

        for (int i = 0; i < cySpringDataContainers.Count; i++)
        {
            cySpringDataContainers[i].InitializePhysics(bones, colliders);
        }

        EnablePhysics = true;
        foreach (CySpringDataContainer cySpring in cySpringDataContainers)
        {
            cySpring.EnablePhysics(true );
        }
    }
    
    public void SetBlush(bool active)
    {
        var cheek = transform.Find("M_Cheek");
        if (cheek)
        {
            cheek.gameObject.SetActive(active);
        }
    }

    public void PlayAnimation(string animPath)
    {
        PlayAnimation(UmaDatabase.MetaData.FirstOrDefault(x => x.Key == animPath).Value);
    }
    public void PlayAnimation(UmaDatabaseEntry animEntry)
    {
        //Debug.Log(UmaDatabase.MetaData.FirstOrDefault(x => x.Value == animEntry).Key);
        AnimationClip animation = UmaAssetManager.LoadAsset<AnimationClip>(UmaDatabase.MetaData.FirstOrDefault(x => x.Value == animEntry).Key);
        animation.name = animEntry.Name;

        // Start anim
        if (animation.name.EndsWith("_s"))
        {
            OverrideController["clip_s"] = animation;
        }

        // End anim
        else if (animation.name.EndsWith("_e"))
        {
            OverrideController["clip_e"] = animation;
        }

        // Tail anim
        else if (animation.name.Contains("tail"))
        {
            UpBodyReset();
            OverrideController["clip_t"] = animation;
            UmaAnimator.Play("motion_t");
        }

        // Pose anim
        else if (animation.name.EndsWith("_pos"))
        {
            OverrideController["clip_p"] = animation;
            UmaAnimator.Play("motion_p", 2, 0);
        }

        // Face anim
        else if (animation.name.EndsWith("_face"))
        {
            Debug.Log($"Playing face anim: {animation.name}");
            PlayFaceAnimation(animation);
        }

        // Loop anim
        else if (animation.name.Contains("_loop"))
        {
            UpBodyReset();
            if (isAnimatorControl && FaceDrivenKeyTarget)
            {
                FaceDrivenKeyTarget.ResetLocator();
                isAnimatorControl = false;
            }

            //Debug.Log($"{animation.name.Replace("/body", "/facial")}_face");

            if (UmaDatabase.MetaData.TryGetValue($"{animation.name.Replace("/body", "/facial")}_face", out UmaDatabaseEntry entry))
            {
                PlayAnimation(entry);
            }

            if (UmaDatabase.MetaData.TryGetValue($"{animation.name.Replace("/body", "/facial")}_ear", out entry))
            {
                PlayAnimation(entry);
            }

            UmaDatabaseEntry motion_e = null, motion_s = null;
            if (UmaDatabase.MetaData.TryGetValue(animation.name.Replace("_loop", "_s"), out motion_s))
            {
                PlayAnimation(motion_s);
            }

            if (OverrideController["clip_2"].name.Contains("_loop"))
            {
                if (!OverrideController["clip_2"].name.Contains("hom_"))//home end animation not for interpolation
                {
                    if (UmaDatabase.MetaData.TryGetValue(OverrideController["clip_2"].name.Replace("_loop", "_e"), out motion_e))
                    {
                        PlayAnimation(motion_e);
                    }
                }
            }

            //Builder.SetPreviewCamera(null);
            OverrideController["clip_1"] = OverrideController["clip_2"];
            OverrideController["clip_2"] = animation;
            UmaAnimator.Play("motion_1", 0, 0);
            UmaAnimator.SetTrigger((motion_s != null && motion_e != null) ? "next_e" : ((motion_s != null) ? "next_s" : "next"));
        }

    }
    public void PlayFaceAnimation(AnimationClip animation)
    {
        if (animation.name.Contains("_s_"))
        {
            FaceOverrideController["clip_s"] = animation;
        }
        else if (animation.name.Contains("_e_"))
        {
            FaceOverrideController["clip_e"] = animation;
        }
        else if (animation.name.Contains("_loop"))
        {
            isAnimatorControl = true;
            FaceDrivenKeyTarget.ResetLocator();
            UmaDatabaseEntry motion_e = null;
            UmaDatabaseEntry motion_s = null;
            if (UmaDatabase.MetaData.TryGetValue(animation.name.Replace("_loop", "_s"), out motion_s))
            {
                PlayAnimation(motion_s);
            }

            if (FaceOverrideController["clip_2"].name.Contains("_loop"))
            {
                if (!FaceOverrideController["clip_2"].name.Contains("hom_"))//home end animation not for interpolation
                {
                    if (UmaDatabase.MetaData.TryGetValue(FaceOverrideController["clip_2"].name.Replace("_loop", "_e"), out motion_e))
                    {
                        PlayAnimation(motion_e);
                    }
                }
            }

            FaceOverrideController["clip_1"] = FaceOverrideController["clip_2"];
            FaceOverrideController["clip_2"] = animation;
            UmaFaceAnimator.Play("motion_1", 0, 0);
            UmaFaceAnimator.SetTrigger((motion_s != null && motion_e != null) ? "next_e" : ((motion_s != null) ? "next_s" : "next"));
        }
        else
        {
            isAnimatorControl = true;
            FaceDrivenKeyTarget.ResetLocator();
            FaceOverrideController["clip_2"] = animation;
            UmaFaceAnimator.Play("motion_2", 0, 0);
        }
    }

    public void PlayMorph(string morphName, float startWeight, float targetWeight, float duration)
    {
        FacialMorph morph = FaceDrivenKeyTarget.AllMorphs.FirstOrDefault(x => x.name == morphName);

        StartCoroutine(AnimateMorph(morph, startWeight, targetWeight, duration));
    }
    public void PlayMorph(FacialMorph morph, float startWeight, float targetWeight, float duration)
    {
        StartCoroutine(AnimateMorph(morph, startWeight, targetWeight, duration));
    }
    public IEnumerator AnimateMorph(FacialMorph morph, float startWeight, float targetWeight, float duration)
    {
        float time = 0f;

        while (time < duration)
        {
            float t = time / duration;
            float currentWeight = Mathf.Lerp(startWeight, targetWeight, t);

            FaceDrivenKeyTarget.ChangeMorphWeight(morph, currentWeight);

            time += Time.deltaTime;
            yield return null;
        }

        // Pastikan nilai akhir tepat
        FaceDrivenKeyTarget.ChangeMorphWeight(morph, targetWeight);
    }
    public void Blink(float duration = .2f)
    {
        PlayMorph("Eye_2_R", 0f, 1f, duration);
        PlayMorph("Eye_2_L", 0f, 1f, duration);

        PlayMorph("Eye_2_R", 1f, 0f, duration);
        PlayMorph("Eye_2_L", 1f, 0f, duration);
    }
    IEnumerator RandomBlinkCoroutine()
    {
        while (true)
        {
            if (FaceDrivenKeyTarget)
            {
                yield return new WaitForSeconds(Random.Range(randomBlinkMinInterval, randomBlinkMaxInterval));

                Blink();
            }

        }
    }
    IEnumerator RandomEarTwitchCoroutine()
    {
        while (true)
        {
            if (FaceDrivenKeyTarget)
            {
                yield return new WaitForSeconds(Random.Range(randomEarTwitchMinInterval, randomEarTwitchMaxInterval));

                FacialMorph morph = FaceDrivenKeyTarget.AllMorphs[Random.Range(minEarMorphIndex, maxEarMorphIndex)];

                PlayMorph(morph, 1f, 0f, .4f);
                yield return new WaitForSeconds(Random.Range(randomEarTwitchMinInterval, randomEarTwitchMaxInterval));
                PlayMorph(morph, 1f, 0f, .4f);
            }

        }
    }
    public void SetRandomBlink(bool state)
    {
        if (state)
        {
            if (randomBlinkCoroutine != null) { return; }
            randomBlinkCoroutine = StartCoroutine(RandomBlinkCoroutine());
            
            return;
        }

        if (randomBlinkCoroutine != null) { StopCoroutine(randomBlinkCoroutine); randomBlinkCoroutine = null; }
        return;
    }
    public void SetRandomEarTwitch(bool state)
    {
        if (state)
        {
            if (randomEarTwitchCoroutine != null) { return; }
            randomEarTwitchCoroutine = StartCoroutine(RandomEarTwitchCoroutine());

            return;
        }

        if (randomEarTwitchCoroutine != null) { StopCoroutine(randomEarTwitchCoroutine); randomEarTwitchCoroutine = null; }
        return;
    }
    public void SetSmile(bool state, float weight = 1f, float duration = .1f, bool withBlush = true, bool closeEyes = false, bool includeEyeBrows = true)
    {
        // Smile: Mouth_12_0
        // EyeBrow: EyeBrow_13_R, EyeBrow_13_L
        // Eye: Eye_5_R, Eye_L_R

        if (FaceDrivenKeyTarget)
        {
            if (state)
            {
                PlayMorph("Mouth_12_0", 0f, 1f, duration);

                if (withBlush)
                {
                    SetBlush(true);
                }

                if (closeEyes)
                {
                    // Takes 30% of base duration
                    PlayMorph("Eye_5_R", 0f, 1f, (30f / 100f) * duration);
                    PlayMorph("Eye_5_L", 0f, 1f, (30f / 100f) * duration);
                }

                if (includeEyeBrows)
                {
                    // Takes 30% of base duration
                    PlayMorph("EyeBrow_13_R", 0f, 1f, (30f / 100f) * duration);
                    PlayMorph("EyeBrow_13_L", 0f, 1f, (30f / 100f) * duration);
                }
                return;
            }

            else
            {
                PlayMorph("Mouth_12_0", 1f, 0f, duration);

                if (withBlush)
                {
                    SetBlush(false);
                }

                if (closeEyes)
                {
                    // Takes 30% of base duration
                    PlayMorph("Eye_5_R", 1f, 0f, (30f / 100f) * duration);
                    PlayMorph("Eye_5_L", 1f, 0f, (30f / 100f) * duration);
                }

                if (includeEyeBrows)
                {
                    // Takes 30% of base duration
                    PlayMorph("EyeBrow_13_R", 1f, 0f, (30f / 100f) * duration);
                    PlayMorph("EyeBrow_13_L", 1f, 0f, (30f / 100f) * duration);
                }
                return;
            }
        }
    }
    public void PlaySignatureAnimation()
    {
        if (UmaDatabase.MetaData.TryGetValue($"3d/motion/event/body/chara/chr{charaEntry.Id}_00/anm_eve_chr{charaEntry.Id}_00_idle01_loop", out UmaDatabaseEntry anim))
        {
            PlayAnimation(anim);
            return;
        }

        Debug.LogWarning($"No signature animation for char {charaEntry.Id}");
    }


    private void FixedUpdate()
    {
        if (EnableEyeTracking && TrackTarget)
        {
            if (!HeadBone)
            {
                HeadBone = (GameObject)bodyInstance.GetComponent<AssetHolder>()._assetTable["head"];
            }
            var finalRotation = FaceDrivenKeyTarget.GetEyeTrackRotation(TrackTarget.transform.position);
            FaceDrivenKeyTarget.SetEyeTrack(finalRotation);

            var cam = Camera.main;
            //LookAt Camera
            TrackTarget.position = Vector3.Lerp(TrackTarget.position, cam.transform.position, Time.fixedDeltaTime * 3);
        }
    }
}

