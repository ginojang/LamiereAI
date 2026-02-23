using System;
using System.Collections.Generic;
using UnityEngine;

public class MortalPrefab : MonoBehaviour
{
    public enum Mode
    {
        Normal = 0,
        Combat = 1,
        // CostumeA = 2,
        // ...
    }

    [Serializable]
    public class Variant
    {
        public Mode mode;
        public GameObject root;      // 이 루트 아래에 Mesh/Armature/Animator 등을 통째로 둠
        public Animator animator;    // root에 Animator가 있으면 자동 채워도 됨(아래 OnValidate 참고)
    }

    [Header("Variants")]
    [SerializeField] private Variant[] variants;

    [Header("Default")]
    [SerializeField] private Mode startMode = Mode.Normal;

    private Dictionary<Mode, Variant> _map;
    private Variant _current;

    public Mode CurrentMode => _current != null ? _current.mode : startMode;

    private void Awake()
    {
        BuildMap();
        // 시작 모드로 세팅
        SetMode(startMode, applyActive: true);
    }

    private void BuildMap()
    {
        _map = new Dictionary<Mode, Variant>(variants?.Length ?? 0);
        if (variants == null) return;

        foreach (var v in variants)
        {
            if (v == null || v.root == null) continue;

            // animator가 비어있으면 root에서 자동으로 찾기
            if (v.animator == null)
                v.animator = v.root.GetComponentInChildren<Animator>(true);

            _map[v.mode] = v;
        }
    }

    /// <summary>
    /// 현재 활성 Variant의 Animator를 반환 (없으면 null)
    /// </summary>
    public Animator GetCurrentAnimator()
    {
        return _current != null ? _current.animator : null;
    }

    /// <summary>
    /// mode에 해당하는 Variant로 전환. (활성/비활성만 처리)
    /// </summary>
    public bool SetMode(Mode mode, bool applyActive)
    {
        if (_map == null) BuildMap();
        if (_map == null || !_map.TryGetValue(mode, out var next) || next == null)
        {
            Debug.LogError($"[MortalPrefab] Variant not found: {mode}");
            return false;
        }

        if (applyActive && variants != null)
        {
            // 모든 Variant root 비활성화
            foreach (var v in variants)
            {
                if (v?.root != null)
                    v.root.SetActive(false);
            }

            // 선택된 Variant만 활성화
            if (next.root != null)
                next.root.SetActive(true);
        }

        _current = next;
        return true;
    }

    public void SetSkinColor(DataManager.eCHARACTER_SKINCOLLOR skin)
    {
        if(_current != null)
        {
            _current.root.GetComponent<CharacterPrefab>().SetSkinColor(skin);
        }

    }


#if UNITY_EDITOR
    private void OnValidate()
    {
        // 에디터에서 animator 자동 세팅 보조
        if (variants == null) return;
        foreach (var v in variants)
        {
            if (v == null || v.root == null) continue;
            if (v.animator == null)
                v.animator = v.root.GetComponentInChildren<Animator>(true);
        }
    }
#endif
}