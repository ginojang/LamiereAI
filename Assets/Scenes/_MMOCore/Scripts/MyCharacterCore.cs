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

    // ===== Normal Attack Queue Settings =====
    [SerializeField] private string attackStateTag = "Attack";   // 공격 상태에 Tag로 지정
    [SerializeField] private float comboResetDelay = 0.8f;        // 이 시간 이상 공격 끊기면 콤보 0으로 리셋

    private float _lastAttackTime = -999f;


    Transform armature;
    Animator targetAnimator;
    MortalPrefab mortal;



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


    private bool _wasAttacking = false;
    private void Update()
    {
        if (!IsReady()) return;

        bool attacking = IsAttackingNow();

        // ✅ 공격이 "끝난" 순간
        if (_wasAttacking && !attacking)
        {
            _lastAttackTime = Time.time;
            // Debug.Log($"Attack End -> _lastAttackTime = {_lastAttackTime}");
        }

        _wasAttacking = attacking;
    }




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

    #region  모드 변경 -  본 변경

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
    #endregion


    #region  전투 관련
    private int _comboIndex = 0;
    [SerializeField, Range(0f, 1f)] private float firstAttackVariantBChance = 0.5f;

    // 0타(첫타) 변형 인덱스: 0 또는 1
    private int _firstAttackVariantIndex = 0; // 0 or 1


    public bool IsAttackingNow()
    {
        // Transition 중엔 Next도 체크하는 게 안정적
        var cur = targetAnimator.GetCurrentAnimatorStateInfo(0);
        if (cur.IsTag(attackStateTag)) return true;

        if (targetAnimator.IsInTransition(0))
        {
            var next = targetAnimator.GetNextAnimatorStateInfo(0);
            if (next.IsTag(attackStateTag)) return true;
        }

        return false;
    }

    private void DecideFirstAttackVariantIfNeeded()
    {
        // 콤보 사이클 시작(= 0타 들어갈 때) 1번만 결정
        if (_comboIndex != 0) return;

        _firstAttackVariantIndex = (Random.value < firstAttackVariantBChance) ? 1 : 0;
    }


    private void SetNormalAttackTrigger()
    {
        if (_comboIndex == 0)
        {
            // 여기서 50퍼센트 확률로  0, 1 둘중 하나로 하자.
            DecideFirstAttackVariantIfNeeded();
            targetAnimator.SetTrigger(_normalAttackTriggerHashes[_firstAttackVariantIndex]); // 0 or 1
        }
        else
        {
            // 2타(피니시)는 묵직한 고정 (예: index 2)
            targetAnimator.SetTrigger(_normalAttackTriggerHashes[2]);
        }
    }
    private void FireNextComboAttack()
    {
        SetNormalAttackTrigger();

        // 콤보 진행/리셋
        if (Time.time - _lastAttackTime < comboResetDelay)
        {
            _comboIndex++;
            if (_comboIndex >= 2) _comboIndex = 0; // 0->1->0 (2연타 사이클)
        }
        else
        {
            _comboIndex = 0;
            // 리셋된 상태에서 바로 다음 입력이 들어오면 첫타 변형이 다시 뽑히게 됨
        }

        _lastAttackTime = Time.time;
    }


    public void RequestNormalAttack()
    {
        if (!IsReady()) return;
        if (mortal.CurrentMode != MortalPrefab.Mode.Combat) return;
        if (_normalAttackTriggerHashes == null || _normalAttackTriggerHashes.Length == 0) return;

        // 공격 중이 아니면 즉시 발사
        if (!IsAttackingNow())
        {
            FireNextComboAttack();
            return;
        }
    }

    #endregion



    #region 애니메이션 Behaviour API 함수

    public void OnBackArmedAnimationStart()
    {
        if (!IsReady()) return;
        mortal.OnBackArmedAnimationStart();
    }


    public void OnNormalAttackEnter(int animIndex)
    {
        if (!IsReady()) return;
        mortal.OnNormalAttackEnter(animIndex, armature);
    }
    public void OnSwordSlashStart(int animIndex)
    {
        if (!IsReady()) return;
        mortal.OnSwordSlashStart(animIndex, armature);
    }
    public void OnSwordSlashHit(int animIndex)
    {
        if (!IsReady()) return;
        mortal.OnSwordSlashHit(animIndex, armature);
    }
    public void OnSwordSlashEnd(int animIndex)
    {
        if (!IsReady()) return;
        mortal.OnSwordSlashEnd(animIndex, armature);
    }
    public void OnNormalAttackExit(int animIndex)
    {
        if (!IsReady()) return;
        mortal.OnNormalAttackExit(animIndex, armature);
    }

    #endregion
}
