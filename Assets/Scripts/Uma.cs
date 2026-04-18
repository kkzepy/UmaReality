using Codice.CM.Common.Merge;
using Gallop;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using UnityEngine;
using static UnityEngine.Rendering.VirtualTexturing.Debugging;

public class UmaAssembler : MonoBehaviour
{
    public static GameObject CreateBody(int charaId, int costumeId, bool loadPrerequisites = true)
    {
        var bodyLogicalPath = UmaDatabase.QueryBodyPath(charaId, costumeId);
        var bodyPath = UmaDatabase.ResolvePath(bodyLogicalPath);

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

            return body;
        }
    }

    public static GameObject CreateHead(int charaId, int headId, bool loadPrerequisites = true)
    {
        var headLogicalPath = UmaDatabase.QueryHeadPath(charaId, headId);
        var headPath = UmaDatabase.ResolvePath(headLogicalPath);

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

            return Instantiate(head);
        }
    }

    public static GameObject CreateTail(int tailId, bool loadPrerequisites = true)
    {
        var tailLogicalPath = UmaDatabase.QueryTailPath(tailId);
        var tailPath = UmaDatabase.ResolvePath(tailLogicalPath);

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

    public static GameObject CreateCharacter(CharaEntry entry, int costumeId = 0, int headId = 0, bool loadPrerequisites = true, bool addComponents = true)
    {
        if (loadPrerequisites)
        {
            var bodyLogicalPath = UmaDatabase.QueryBodyPath(entry.Id, costumeId);
            var headLogicalPath = UmaDatabase.QueryHeadPath(entry.Id, headId);
            var tailLogicalPath = UmaDatabase.QueryTailPath(entry.TailModelId);

            UmaAssetManager.PreLoadPrerequistes(bodyLogicalPath);
            UmaAssetManager.PreLoadPrerequistes(headLogicalPath);
            UmaAssetManager.PreLoadPrerequistes(tailLogicalPath);

            UmaAssetManager.LoadPrerequistes(bodyLogicalPath);
            UmaAssetManager.LoadPrerequistes(headLogicalPath);
            UmaAssetManager.LoadPrerequistes(tailLogicalPath);
        }

        var bodyInstance = CreateBody(entry.Id, costumeId, false);
        var headInstance = CreateHead(entry.Id, headId, false);
        var tailInstance = CreateTail(entry.TailModelId, false);

        ApplyTailTexture(tailInstance, entry.Id, entry.TailModelId);

        var chara = Assemble(bodyInstance, headInstance, tailInstance);
        
        if (addComponents)
        {
            var umaCharacter = chara.AddComponent<UmaCharacter>();
            umaCharacter.SetAssetHolder(bodyInstance.GetComponent<AssetHolder>());
            umaCharacter.charaEntry = entry;
            umaCharacter.costumeId = costumeId;
            umaCharacter.Initialize();
            //chara.AddComponent<AnimationLoader>();
        }

        Debug.Log($"Loaded char: {entry.Id}");

        return chara;
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

            mainTex = UmaAssetManager.LoadAsset<Texture2D>(mainTexLogicalPath, false);
            toonMap = UmaAssetManager.LoadAsset<Texture2D>(toonMapLogicalPath, false);
            tripleMaskMap = UmaAssetManager.LoadAsset<Texture2D>(tripleMaskMapLogicalPath, false);
            optionMaskMap = UmaAssetManager.LoadAsset<Texture2D>(optionMaskMapLogicalPath, false);

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
    public int costumeId;
    string _costumeId;
    string _tailId;
    AssetHolder assetHolder;

    GameObject upBodyBone;
    Vector3 upBodyPosition;
    Quaternion upBodyRotation;

    Animator UmaAnimator;
    AnimatorOverrideController UmaControllerOverride;

    public GameObject PhysicsContainer;
    List<CySpringDataContainer> cySpringDataContainers;

    public void Initialize()
    {
        _costumeId = costumeId.ToString();

        if (_costumeId.Length == 1)
        {
            _costumeId = "0" + _costumeId;
        }

        _tailId = charaEntry.TailModelId.ToString();

        if (_tailId.Length < 4)
        {
            _tailId = new string('0', 4 - _tailId.Length) + _tailId;
        }

        if (assetHolder == null)
        {

            // Try to get AssetHolder component from the GameObject
            assetHolder = GetComponent<AssetHolder>();

            if (assetHolder == null)
            {
                Debug.LogWarning("AssetHolder component not found on UmaCharacter!");
            }
            else
            {
                upBodyBone = assetHolder._assetTable["upbody_ctrl"] as GameObject;
                upBodyPosition = upBodyBone.transform.localPosition;
                upBodyRotation = upBodyBone.transform.localRotation;
            }
        }
        else
        {
            upBodyBone = assetHolder._assetTable["upbody_ctrl"] as GameObject;
            upBodyPosition = upBodyBone.transform.localPosition;
            upBodyRotation = upBodyBone.transform.localRotation;
        }
        

        UmaAnimator = GetComponent<Animator>();
        UmaAnimator.avatar = AvatarBuilder.BuildGenericAvatar(gameObject, gameObject.name);
        UmaControllerOverride = new AnimatorOverrideController(UmaAnimator.runtimeAnimatorController);
    
        PhysicsContainer = new GameObject("PhysicsContainer");
        PhysicsContainer.transform.SetParent(transform);
    }

    public void SetAssetHolder(AssetHolder assetHolder)
    {
        this.assetHolder = assetHolder;

        upBodyBone = assetHolder._assetTable["upbody_ctrl"] as GameObject;
        upBodyPosition = upBodyBone.transform.localPosition;
        upBodyRotation = upBodyBone.transform.localRotation;
    }

    public void UpBodyReset()
    {
        if (upBodyBone)
        {
            upBodyBone.transform.localPosition = upBodyPosition;
            upBodyBone.transform.localRotation = upBodyRotation;
            Debug.Log("UpBody reset to default position and rotation.");
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
        string tailClothesLogicalPath = UmaDatabase.TailPath + $"tail{_tailId}_00/clothes/pfb_tail{_tailId}_00_cloth00";
        string headClothesLogicalPath = UmaDatabase.HeadPath + $"chr{charaEntry.Id}_{_costumeId}/clothes/pfb_chr{charaEntry.Id}_{_costumeId}_cloth00";

        //Debug.Log($"{clothesLogicalPath}\n{bustClothesLogicalPath}\n{tailClothesLogicalPath}\n{headClothesLogicalPath}");

        Instantiate(UmaAssetManager.LoadAsset<GameObject>(clothesLogicalPath, false), PhysicsContainer.transform);
        Instantiate(UmaAssetManager.LoadAsset<GameObject>(bustClothesLogicalPath, false), PhysicsContainer.transform);
        Instantiate(UmaAssetManager.LoadAsset<GameObject>(tailClothesLogicalPath, false), PhysicsContainer.transform);
        Instantiate(UmaAssetManager.LoadAsset<GameObject>(headClothesLogicalPath, false), PhysicsContainer.transform);


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
    }

    public void SetBlush(bool active)
    {
        var cheek = transform.Find("M_Cheek");
        if (cheek)
        {
            cheek.gameObject.SetActive(active);
        }
    }

    public void ToggleBlush()
    {
        var cheek = transform.Find("M_Cheek");
        if (cheek)
        {
            cheek.gameObject.SetActive(!cheek.gameObject.activeSelf);
        }
    }
}

