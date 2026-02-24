using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyCharacterCore : MonoBehaviour
{
    public static MyCharacterCore Instance = null;

    GameObject _mortalPrefab = null;

    [SerializeField] GameObject[] _characterPrefabs;
    [SerializeField] DataManager.eCHARACTER_TYPE[] _characterTypes;

    [SerializeField] DataManager.eCHARACTER_TYPE c_type = DataManager.eCHARACTER_TYPE.MIN_HO;
    [SerializeField] DataManager.eCHARACTER_FATNESS fatness = DataManager.eCHARACTER_FATNESS.MIDDLE_SLIM;
    [SerializeField] DataManager.eCHARACTER_SKINCOLLOR skin_color = DataManager.eCHARACTER_SKINCOLLOR._color_skin7;

    Dictionary<DataManager.eCHARACTER_TYPE, GameObject> dicCharacterPrefabs;

    public DataManager.eCHARACTER_TYPE GetCharacterType() => c_type;

    
    [SerializeField] private MortalPrefab.Mode initialMode = MortalPrefab.Mode.Normal;


    [SerializeField] private string[] NormalAttackTriggerNames;
    private int[] _normalAttackTriggerHashes;


    Transform armature;
    Animator targetAnimator;
    MortalPrefab mortal;


    private void CacheAttackTriggers()
    {
        if (NormalAttackTriggerNames == null) return;

        _normalAttackTriggerHashes = new int[NormalAttackTriggerNames.Length];
        for (int i = 0; i < NormalAttackTriggerNames.Length; i++)
        {
            _normalAttackTriggerHashes[i] = Animator.StringToHash(NormalAttackTriggerNames[i]);
        }
    }


    public GameObject GetAvatarPrefab(DataManager.eCHARACTER_TYPE c_type)
    {
        if (dicCharacterPrefabs.ContainsKey(c_type))
            return dicCharacterPrefabs[c_type];
        return null;
    }

    private void Awake()
    {
        Instance = this;

        dicCharacterPrefabs = new Dictionary<DataManager.eCHARACTER_TYPE, GameObject>();

        if (_characterTypes == null || _characterPrefabs == null || _characterTypes.Length != _characterPrefabs.Length)
        {
            Debug.LogError("[MyCharacterCore] _characterTypes / _characterPrefabs length mismatch or null.");
            return;
        }

        for (int i = 0; i < _characterTypes.Length; i++)
        {
            if (_characterPrefabs[i] == null)
            {
                Debug.LogError($"[MyCharacterCore] Prefab is null at index {i} (type={_characterTypes[i]}).");
                continue;
            }

            dicCharacterPrefabs[_characterTypes[i]] = _characterPrefabs[i];
        }
    }


    public void RestoreLocalZero()
    {
        if (_mortalPrefab == null) return;
        _mortalPrefab.transform.localPosition = Vector3.zero;
        _mortalPrefab.transform.localRotation = Quaternion.identity;
    }


    public GameObject PrefabInstantiate(DataManager.eCHARACTER_TYPE type)
    {
        if (c_type == type && _mortalPrefab != null)
            return _mortalPrefab;

        if (_mortalPrefab != null)
        {
            Destroy(_mortalPrefab);
            _mortalPrefab = null;
        }

        c_type = type;

        var prefab = GetAvatarPrefab(type);
        if (prefab == null)
        {
            Debug.LogError($"[MyCharacterCore] Prefab not found for type: {type}");
            return null;
        }

        _mortalPrefab = Instantiate(prefab);
        _mortalPrefab.transform.SetParent(transform, false);
        _mortalPrefab.transform.localPosition = Vector3.zero;
        _mortalPrefab.transform.localRotation = Quaternion.identity;

        return _mortalPrefab;
    }



    public void SetFatness(DataManager.eCHARACTER_FATNESS fat)
    {
        if (_mortalPrefab == null) return;

        float scale = 1.0f;
        fatness = fat;

        switch (fatness)
        {
            case DataManager.eCHARACTER_FATNESS.FAT:
                scale = (float)(DataManager.eCHARACTER_FATNESS.FAT + 8) / 10.0f;
                break;
            case DataManager.eCHARACTER_FATNESS.MIDDLE_FAT:
                scale = (float)(DataManager.eCHARACTER_FATNESS.MIDDLE_FAT + 8) / 10.0f;
                break;
            case DataManager.eCHARACTER_FATNESS.MIDDLE_SLIM:
                scale = (float)(DataManager.eCHARACTER_FATNESS.MIDDLE_SLIM + 8) / 10.0f;
                break;
            case DataManager.eCHARACTER_FATNESS.SLIM:
                scale = (float)(DataManager.eCHARACTER_FATNESS.SLIM + 8) / 10.0f;
                break;
        }

        _mortalPrefab.transform.localScale = new Vector3(scale, 1, scale);
    }

    public void SetSkinColor(DataManager.eCHARACTER_SKINCOLLOR skin)
    {
        if (_mortalPrefab == null) return;
        
        skin_color = skin;
        _mortalPrefab.GetComponent<MortalPrefab>().SetSkinColor(skin_color);
    }

    public void SetWeapon()
    {
        if (_mortalPrefab == null) return;
        _mortalPrefab.GetComponent<MortalPrefab>().SetWeapon();
    }




    /// //////////////////////////////////////////////////////////////////////////////////////////////////
    // 반드시 호출되어야 할 시작... 함수.
    public void CreateMyCharacter()
    {
        armature = gameObject.transform.Find("Armature");
        if (armature == null)
        {
            Debug.LogError("[MyCharacterCore] Armature not found under MyCharactor.");
            return;
        }

        // 결과가 _mortalPrefab 에 세팅 된다.
        if (PrefabInstantiate(GetCharacterType()) == null)
            return;

        var geometry = armature.transform.Find("Geometry");
        if (geometry == null)
        {
            Debug.LogError("[MyCharacterCore] Geometry not found under playerArmature.");
            return;
        }

        _mortalPrefab.transform.SetParent(geometry, false);
        _mortalPrefab.transform.localPosition = Vector3.zero;
        _mortalPrefab.transform.localRotation = Quaternion.identity;

        targetAnimator = armature.GetComponent<Animator>();
        if (targetAnimator == null)
        {
            Debug.LogError("[MyCharacterCore] targetAnimator missing on Armature.");
            return;
        }

        mortal = _mortalPrefab.GetComponent<MortalPrefab>();
        if (mortal == null)
        {
            Debug.LogError("[MyCharacterCore] MortalPrefab missing on avatar prefab.");
            return;
        }

        // 시작 모드 세팅 + 스왑 적용
        ApplyMortalMode(targetAnimator, mortal, initialMode);

        // 외형 파라미터 재적용
        SetSkinColor(skin_color);
        SetFatness(fatness);
        SetWeapon();

        //
        CacheAttackTriggers();

        // InputController 세팅  -> 추후  ThirdPerson, StartAssetsInput, WowPcInputsBridge 통합한   전용 InpuntController 작업 필요.
        //   그녀석에게 지금 이 Component 알려준다.

        WowPcInputsBridge InputBridge = armature.GetComponent<WowPcInputsBridge>();
        InputBridge.SetParentCharacterCore(this);


   

        Debug.Log(">> Player Avatar Swapped (Rebind) >>>>>>>>>>>>>>>>>");
    }

    public bool ApplyMortalMode(Animator targetAnimator, MortalPrefab mortal, MortalPrefab.Mode mode)
    {
        if (targetAnimator == null || mortal == null) return false;

        if (!mortal.SetMode(mode, applyActive: true))
            return false;

        var sourceAnimator = mortal.GetCurrentAnimator();
        if (sourceAnimator == null)
        {
            Debug.LogError("[MyCharacterCore] sourceAnimator is null after mode switch.");
            return false;
        }

        if (sourceAnimator.avatar == null || sourceAnimator.runtimeAnimatorController == null)
        {
            Debug.LogError("[MyCharacterCore] Source Animator missing Avatar or Controller.");
            return false;
        }

        // (선택) 토글로 꼬임 방지
        bool prevEnabled = targetAnimator.enabled;
        targetAnimator.enabled = false;

        targetAnimator.runtimeAnimatorController = sourceAnimator.runtimeAnimatorController;
        targetAnimator.avatar = sourceAnimator.avatar;

        targetAnimator.enabled = prevEnabled;

        targetAnimator.Rebind();
        targetAnimator.Update(0f);

        return true;
    }

    private bool IsReady()
    {
        return _mortalPrefab != null && armature != null && targetAnimator != null && mortal != null;
    }



    // 외부에서 모드 변경 호출용
    public void SetCombatMode(bool isCombat)
    {
        if (!IsReady()) return;

        var mode = isCombat ? MortalPrefab.Mode.Combat : MortalPrefab.Mode.Normal;
        if (ApplyMortalMode(targetAnimator, mortal, mode))
        {
            // 모드 바뀔 때 재적용 필요한 것들
            SetSkinColor(skin_color);
            SetFatness(fatness);
            SetWeapon();
        }
    }

    public void ToggleCombatMode()
    {
        if (!IsReady()) return;

        // 현재 모드 확인
        var currentMode = mortal.CurrentMode;

        MortalPrefab.Mode nextMode =
            currentMode == MortalPrefab.Mode.Combat
            ? MortalPrefab.Mode.Normal
            : MortalPrefab.Mode.Combat;

        if (ApplyMortalMode(targetAnimator, mortal, nextMode))
        {
            SetSkinColor(skin_color);
            SetFatness(fatness);
            SetWeapon();

            Debug.Log($"[MyCharacterCore] Mode Toggled → {nextMode}");
        }
    }



    private int _comboIndex = 0;
    public void SetTriggerNormalAttack()
    {
        if (!IsReady()) return;
        if (mortal.CurrentMode != MortalPrefab.Mode.Combat) return;
        if (_normalAttackTriggerHashes == null || _normalAttackTriggerHashes.Length == 0) return;

        
        targetAnimator.SetTrigger(_normalAttackTriggerHashes[_comboIndex]);
        _comboIndex++;
        if (_comboIndex >= _normalAttackTriggerHashes.Length)
            _comboIndex = 0;

    }



    // 애니메이션 Behaviour API 함수
    public void OnBackArmedAnimationStart()
    {
        if (!IsReady()) return;

        mortal.OnBackArmedAnimationStart();
    }
}
