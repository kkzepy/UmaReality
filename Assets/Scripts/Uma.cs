using UnityEngine;
using System.Linq;

public class UmaCharacterBuilder
{
    private GameObject CharacterRoot;

    public GameObject BuildCharacter(int characterId, int costumeId, int headId)
    {
        CharacterRoot = new GameObject($"UmaCharacter_{characterId}");

        // 1. Load semua part
        GameObject body = LoadPart(UmaAssetManager.QueryBodyPath(characterId, costumeId), "pfb_bdy");
        GameObject head = LoadPart(UmaAssetManager.QueryHeadPath(characterId, headId), "pfb_head");
        GameObject tail = LoadPart(UmaAssetManager.QueryTailPath(characterId), "pfb_tail");

        // 2. Attach ke root
        body.transform.SetParent(CharacterRoot.transform, false);
        head.transform.SetParent(CharacterRoot.transform, false);
        tail.transform.SetParent(CharacterRoot.transform, false);

        // 3. Gabungkan skeleton
        BindSkeleton(body, head);
        BindSkeleton(body, tail);

        // 4. Apply texture
        ApplyTextures(characterId, body);
        ApplyTextures(characterId, head);
        ApplyTextures(characterId, tail);

        // 5. Setup animasi
        SetupAnimator(CharacterRoot);

        return CharacterRoot;
    }

    // =========================
    // LOAD PART
    // =========================
    private GameObject LoadPart(string logicalPath, string assetName)
    {
        string realPath = UmaAssetManager.ResolvePath(logicalPath);

        AssetBundle bundle = AssetBundle.LoadFromFile(realPath);
        GameObject prefab = bundle.LoadAsset<GameObject>(assetName);

        return GameObject.Instantiate(prefab);
    }

    // =========================
    // SKELETON BINDING
    // =========================
    private void BindSkeleton(GameObject body, GameObject part)
    {
        var bodyBones = body.GetComponentsInChildren<Transform>();

        var smrParts = part.GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (var smr in smrParts)
        {
            Transform[] newBones = new Transform[smr.bones.Length];

            for (int i = 0; i < smr.bones.Length; i++)
            {
                string boneName = smr.bones[i].name;

                Transform match = bodyBones.FirstOrDefault(b => b.name == boneName);
                if (match != null)
                    newBones[i] = match;
            }

            smr.bones = newBones;
        }
    }

    // =========================
    // TEXTURE APPLY
    // =========================
    private void ApplyTextures(int characterId, GameObject part)
    {
        string[] texturePaths = UmaAssetManager.QueryTexturePath(characterId);

        Texture2D[] textures = texturePaths
            .Select(p => LoadTexture(p))
            .Where(t => t != null)
            .ToArray();

        var renderers = part.GetComponentsInChildren<SkinnedMeshRenderer>();

        foreach (var smr in renderers)
        {
            foreach (var mat in smr.materials)
            {
                Texture2D tex = FindTexture(textures, mat.name);
                if (tex != null)
                {
                    mat.mainTexture = tex;
                }
            }
        }
    }

    private Texture2D LoadTexture(string logicalPath)
    {
        string realPath = UmaAssetManager.ResolvePath(logicalPath);

        AssetBundle bundle = AssetBundle.LoadFromFile(realPath);
        return bundle.LoadAsset<Texture2D>("_MainTex");
    }

    private Texture2D FindTexture(Texture2D[] textures, string materialName)
    {
        return textures.FirstOrDefault(t => t.name.Contains(materialName));
    }

    // =========================
    // ANIMATION SETUP
    // =========================
    private void SetupAnimator(GameObject character)
    {
        Animator animator = character.AddComponent<Animator>();

        // Minimal setup (placeholder controller)
        RuntimeAnimatorController controller =
            Resources.Load<RuntimeAnimatorController>("DefaultUmaController");

        animator.runtimeAnimatorController = controller;
    }
}