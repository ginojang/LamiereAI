using System;
using System.Collections.Generic;
using UnityEngine;

public class BaseTBone : MonoBehaviour
{
    [Serializable]
    public class SkinTarget
    {
        public SkinnedMeshRenderer renderer;
        [Min(0)] public int materialIndex = 0;
        public string colorProperty = "_BaseColor"; // HDRP/URP 표준값이 다를 수 있어 옵션화
    }

    [Header("Skin")]
    [SerializeField] private DataManager.eCHARACTER_SKINCOLLOR skinColor = DataManager.eCHARACTER_SKINCOLLOR._color_skin7;

    [Tooltip("스킨 컬러를 적용할 렌더러/머티리얼 타겟들")]
    [SerializeField] private List<SkinTarget> skinTargets = new List<SkinTarget>();

    [Header("Weapon Sockets")]
    [SerializeField] private Transform socketRightHand;
    [SerializeField] private Transform socketLeftHand;
    [SerializeField] private Transform socketBack;

    private MaterialPropertyBlock _mpb;

    // 팔레트: 기존 값 그대로 유지 (원하면 여기만 바꾸면 됨)
    private static readonly Dictionary<DataManager.eCHARACTER_SKINCOLLOR, Color32> SkinPalette
        = new Dictionary<DataManager.eCHARACTER_SKINCOLLOR, Color32>
    {
        { DataManager.eCHARACTER_SKINCOLLOR._color_skin1, new Color32(255, 255, 255, 255) },
        { DataManager.eCHARACTER_SKINCOLLOR._color_skin2, new Color32(220, 220, 220, 255) },
        { DataManager.eCHARACTER_SKINCOLLOR._color_skin3, new Color32(185, 185, 185, 255) },
        { DataManager.eCHARACTER_SKINCOLLOR._color_skin4, new Color32(175, 175, 175, 255) },
        { DataManager.eCHARACTER_SKINCOLLOR._color_skin5, new Color32(165, 165, 165, 255) },
        { DataManager.eCHARACTER_SKINCOLLOR._color_skin6, new Color32(150, 150, 150, 255) },
        { DataManager.eCHARACTER_SKINCOLLOR._color_skin7, new Color32(126, 126, 126, 255) },
        { DataManager.eCHARACTER_SKINCOLLOR._color_skin8, new Color32(100, 100, 100, 255) },
    };


    // 생성은 반드시 여기서 하라고 오류냄
    private void Awake()
    {
        _mpb = new MaterialPropertyBlock();
    }
    private void Start()
    {        
    }

    private void OnEnable()
    {
        ApplySkinColor(skinColor);
    }

    // ===== Public API =====

    public DataManager.eCHARACTER_SKINCOLLOR GetSkinColor() => skinColor;

    public Transform GetRightHandSocket() => socketRightHand;
    public Transform GetLeftHandSocket() => socketLeftHand;
    public Transform GetBackSocket() => socketBack;

    public void ApplySkinColor(DataManager.eCHARACTER_SKINCOLLOR skin)
    {
        if (skin == DataManager.eCHARACTER_SKINCOLLOR.NONE)
            return;

        if (!SkinPalette.TryGetValue(skin, out var color))
            return;

        skinColor = skin;

        for (int i = 0; i < skinTargets.Count; i++)
        {
            var t = skinTargets[i];
            if (t == null || t.renderer == null) continue;

            int idx = Mathf.Max(0, t.materialIndex);
            var mats = t.renderer.sharedMaterials;
            if (mats == null || idx >= mats.Length) continue;

            t.renderer.GetPropertyBlock(_mpb, idx);
            _mpb.SetColor(t.colorProperty, color);
            t.renderer.SetPropertyBlock(_mpb, idx);
            _mpb.Clear(); // optional
        }
    }

    /// <summary>
    /// 무기/장비를 소켓에 붙인다 (기본: local 0/identity)
    /// </summary>
    public bool AttachToSocket(GameObject itemPrefab, SocketKind socketKind)
    {
        //spawned = null;
        if (itemPrefab == null) return false;

        Transform parent = socketKind switch
        {
            SocketKind.RightHand => socketRightHand,
            SocketKind.LeftHand => socketLeftHand,
            SocketKind.Back => socketBack,
            _ => null
        };

        if (parent == null) return false;

        itemPrefab.transform.parent = parent;
        itemPrefab.transform.localPosition = Vector3.zero;
        itemPrefab.transform.localRotation = Quaternion.identity;
        itemPrefab.transform.localScale = Vector3.one;
        return true;
    }

    public enum SocketKind
    {
        RightHand,
        LeftHand,
        Back,
    }
}