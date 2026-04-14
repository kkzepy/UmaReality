using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using UnityEngine;

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

    public static GameObject CreateHead(int charaId, int headId)
    {
        var headLogicalPath = UmaAssetManager.QueryHeadPath(charaId, headId);
        var headPath = UmaAssetManager.ResolvePath(headLogicalPath);

        using (var stream = new UmaAssetBundleStream(headPath, UmaDatabaseController.MetaData[headLogicalPath].FKey))
        {
            var bundle = AssetBundle.LoadFromStream(stream);
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

            bundle.Unload(false);
            return Instantiate(head);
        }
    }

    public static GameObject CreateTail(int tailId)
    {
        var tailLogicalPath = UmaAssetManager.QueryTailPath(tailId);
        var tailPath = UmaAssetManager.ResolvePath(tailLogicalPath);

        //UmaAssetManager.LoadPrerequistes(tailLogicalPath);

        using (var stream = new UmaAssetBundleStream(tailPath, UmaDatabaseController.MetaData[tailLogicalPath].FKey))
        {
            var bundle = AssetBundle.LoadFromStream(stream);
            var tail = bundle.LoadAllAssets<GameObject>().FirstOrDefault();
            bundle.Unload(false);
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

        var animator = rootObject.AddComponent<Animator>();
        animator.avatar = AvatarBuilder.BuildGenericAvatar(rootObject, rootObject.name);

        return rootObject;

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
            var toonMapLogicalPath = $"{UmaAssetManager.BodyPath}bdy{characterId}_{_costumeId}/textures/tex_bdy{characterId}_{_costumeId}_shad_c_wet";
            var tripleMaskMapLogicalPath = $"{UmaAssetManager.BodyPath}bdy{characterId}_{_costumeId}/textures/tex_bdy{characterId}_{_costumeId}_base_wet";
            var optionMaskMapLogicalPath = $"{UmaAssetManager.BodyPath}bdy{characterId}_{_costumeId}/textures/tex_bdy{characterId}_{_costumeId}_ctrl_wet";

            mainTex = UmaAssetManager.LoadTexture2DAsset(mainTexLogicalPath);
            toonMap = UmaAssetManager.LoadTexture2DAsset(toonMapLogicalPath);
            tripleMaskMap = UmaAssetManager.LoadTexture2DAsset(tripleMaskMapLogicalPath);
            optionMaskMap = UmaAssetManager.LoadTexture2DAsset(optionMaskMapLogicalPath);

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

            var mainTexLogicalPath = $"{UmaAssetManager.TailPath}tail{_tailId}_00/textures/tex_tail{_tailId}_00_{characterId}_diff";
            var toonMapLogicalPath = $"{UmaAssetManager.TailPath}tail{_tailId}_00/textures/tex_tail{_tailId}_00_{characterId}_shad_c";
            var tripleMaskMapLogicalPath = $"{UmaAssetManager.TailPath}tail0001_00/textures/tex_tail0001_00_0000_base";//$"{UmaAssetManager.TailPath}tail{_tailId}_00/textures/tex_tail{_tailId}_00_{characterId}_base_wet";
            var optionMaskMapLogicalPath = $"{UmaAssetManager.TailPath}tail0001_00/textures/tex_tail0001_00_0000_ctrl";//$"{UmaAssetManager.TailPath}tail{_tailId}_00/textures/tex_tail{_tailId}_00_{characterId}_ctrl_wet";

            mainTex = UmaAssetManager.LoadTexture2DAsset(mainTexLogicalPath);
            toonMap = UmaAssetManager.LoadTexture2DAsset(toonMapLogicalPath);
            tripleMaskMap = UmaAssetManager.LoadTexture2DAsset(tripleMaskMapLogicalPath);
            optionMaskMap = UmaAssetManager.LoadTexture2DAsset(optionMaskMapLogicalPath);

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

        var faceMainTexLogicalPath = $"{UmaAssetManager.HeadPath}chr{characterId}_{_costumeId}/textures/tex_chr{characterId}_{_costumeId}_face_diff_wet";
        var faceToonMapLogicalPath = $"{UmaAssetManager.HeadPath}chr{characterId}_{_costumeId}/textures/tex_chr{characterId}_{_costumeId}_face_shad_c_wet";
        var hairMainTexLogicalPath = $"{UmaAssetManager.HeadPath}chr{characterId}_{_costumeId}/textures/tex_chr{characterId}_{_costumeId}_hair_diff_wet";
        var hairToonMapLogicalPath = $"{UmaAssetManager.HeadPath}chr{characterId}_{_costumeId}/textures/tex_chr{characterId}_{_costumeId}_hair_shad_c_wet";
        var eyeMaterialLogicalPath = $"sourceresources/3d/chara/head/chr{characterId}_{_costumeId}/materials/mtl_chr{characterId}_{_costumeId}_eye";

        faceMainTex = UmaAssetManager.LoadTexture2DAsset(faceMainTexLogicalPath);
        faceToonMap = UmaAssetManager.LoadTexture2DAsset(faceToonMapLogicalPath);
        hairMainTex = UmaAssetManager.LoadTexture2DAsset(hairMainTexLogicalPath);
        hairToonMap = UmaAssetManager.LoadTexture2DAsset(hairToonMapLogicalPath);
        eyeMaterial = UmaAssetManager.LoadTexture2DAsset(eyeMaterialLogicalPath);

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
    //public Animation UmaAnimation = null;
    public Animator UmaAnimator = null;
    private void Start()
    {
        UmaAnimator = GetComponent<Animator>();
    }

    public void PlayAnimation(AnimationClip newClip)
    {
        // Ambil controller yang sekarang sedang dipakai
        RuntimeAnimatorController currentController = UmaAnimator.runtimeAnimatorController;

        // Cek apakah controller saat ini sudah merupakan Override Controller
        AnimatorOverrideController overrideController = currentController as AnimatorOverrideController;

        if (overrideController == null)
        {
            // Jika belum, buat baru berdasarkan controller yang ada
            overrideController = new AnimatorOverrideController(currentController);
            UmaAnimator.runtimeAnimatorController = overrideController;
        }

        // Pastikan base controller-nya tidak null sebelum digunakan
        if (overrideController.runtimeAnimatorController != null)
        {
            overrideController["default"] = newClip;
        }
        else
        {
            Debug.LogError("Master Controller tidak ditemukan di Override Controller!");
        }
    }
}