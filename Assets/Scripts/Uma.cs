using UnityEngine;
using System.Linq;

public class UmaCharacterBuilder
{
    public GameObject Character { get; private set; }

    public void BuildCharacter(int characterId, string animBundlePath)
    {
        // 1️⃣ Load asset paths dari database
        var paths = UmaDatabaseController.QueryCharacterPaths(characterId);
        var bodyBundle = UmaAssetManager.LoadBundle(paths.BodyBundlePath);
        var headBundle = UmaAssetManager.LoadBundle(paths.HeadBundlePath);

        GameObject bodyObj = bodyBundle.LoadAsset<GameObject>("pfb_bdy");
        GameObject headObj = headBundle.LoadAsset<GameObject>("pfb_head");

        Texture2D[] textures = UmaAssetManager.LoadTextures(paths.TextureDirectories);

        // 2️⃣ Assemble character model
        Character = new GameObject("UmaCharacter");

        GameObject body = Object.Instantiate(bodyObj, Character.transform);
        GameObject head = Object.Instantiate(headObj, Character.transform);

        ApplyTextures(body, textures);
        ApplyTextures(head, textures);

        // unify bone hierarchy
        SkinnedMeshRenderer smrBody = body.GetComponent<SkinnedMeshRenderer>();
        SkinnedMeshRenderer smrHead = head.GetComponent<SkinnedMeshRenderer>();

        Transform[] bones = Character.GetComponentsInChildren<Transform>()
                                     .Where(t => t.name.StartsWith("bone"))
                                     .ToArray();

        smrBody.bones = bones;
        smrHead.bones = bones;

        // 3️⃣ Setup animation
        SetupAnimation(animBundlePath);

        // 4️⃣ Initialize facial morphs
        InitFacialMorphs();

        // 5️⃣ Initialize physics
        InitializePhysics();
    }

    private void ApplyTextures(GameObject obj, Texture2D[] textures)
    {
        var smr = obj.GetComponent<SkinnedMeshRenderer>();
        foreach (Material mat in smr.materials)
        {
            mat.mainTexture = FindTexture(textures, mat.name);
        }
    }

    private Texture2D FindTexture(Texture2D[] textures, string matName)
    {
        return textures.FirstOrDefault(t => t.name.Contains(matName)) ?? Texture2D.whiteTexture;
    }

    private void SetupAnimation(string animBundlePath)
    {
        var animBundle = UmaAssetManager.LoadBundle(animBundlePath);
        var clips = animBundle.LoadAllAssets<AnimationClip>();

        Animator animator = Character.AddComponent<Animator>();
        var controller = new AnimatorOverrideController(DefaultController);

        foreach (var clip in clips)
        {
            controller[clip.name] = clip;
        }

        animator.runtimeAnimatorController = controller;
    }

    private void InitFacialMorphs()
    {
        var faceController = Character.AddComponent<FaceDrivenKeyTarget>();
        var faceTargets = Character.GetComponentsInChildren<SkinnedMeshRenderer>()
                                   .SelectMany(r => r.sharedMesh.blendShapeNames)
                                   .ToArray();

        faceController.Initialize(faceTargets);
    }

    private void InitializePhysics()
    {
        var springData = Character.AddComponent<CySpringDataContainer>();
        springData.SetupDefault();
    }
}