using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyCharacterCore : MonoBehaviour
{
    public static MyCharacterCore Instance = null;

    GameObject _characterPrefab = null;

    [SerializeField] GameObject playerArmature;

    [SerializeField] GameObject[] _characterPrefabs;
    [SerializeField] DataManager.eCHARACTER_TYPE[] _characterTypes;

    [SerializeField] DataManager.eCHARACTER_TYPE c_type = DataManager.eCHARACTER_TYPE.BOY_04;
    [SerializeField] DataManager.eCHARACTER_FATNESS fatness = DataManager.eCHARACTER_FATNESS.MIDDLE_SLIM;
    [SerializeField] DataManager.eCHARACTER_SKINCOLLOR skin_color = DataManager.eCHARACTER_SKINCOLLOR._color_skin7;

    Dictionary<DataManager.eCHARACTER_TYPE, GameObject> dicCharacterPrefabs;

    public DataManager.eCHARACTER_TYPE GetCharacterType() => c_type;

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
        for (int i = 0; i < _characterTypes.Length; i++)
            dicCharacterPrefabs[_characterTypes[i]] = _characterPrefabs[i];
    }

    public void RestorePositionZero()
    {
        if (_characterPrefab == null) return;

        _characterPrefab.transform.parent = gameObject.transform;
        _characterPrefab.transform.position = gameObject.transform.position;
        _characterPrefab.transform.rotation = Quaternion.identity;
    }

    public GameObject PrefabInstantiate(DataManager.eCHARACTER_TYPE type)
    {
        if (c_type == type && _characterPrefab != null)
        {
            _characterPrefab.transform.parent = gameObject.transform;
            _characterPrefab.transform.position = Vector3.zero;
            _characterPrefab.transform.rotation = Quaternion.identity;
            return _characterPrefab;
        }

        if (_characterPrefab)
            Destroy(_characterPrefab);

        c_type = type;

        var prefab = GetAvatarPrefab(type);
        if (prefab == null)
        {
            Debug.LogError($"[MyCharacterCore] Prefab not found for type: {type}");
            return null;
        }

        _characterPrefab = Instantiate(prefab);
        _characterPrefab.transform.parent = gameObject.transform;

        return _characterPrefab;
    }

    public void SetFatness(DataManager.eCHARACTER_FATNESS fat)
    {
        if (_characterPrefab == null) return;

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

        _characterPrefab.transform.localScale = new Vector3(scale, 1, scale);
    }

    public void SetSkinColor(DataManager.eCHARACTER_SKINCOLLOR skin)
    {
        if (_characterPrefab == null) return;

        skin_color = skin;
        _characterPrefab.GetComponent<CharacterPrefab>().SetSkinColor(skin_color);
    }




    // ✅ Rebind 기반으로 아바타 교체 안정화
    public void CreateMyCharacter(DataManager.eCHARACTER_TYPE charType)
    {
        var avatarGO = PrefabInstantiate(charType);
        if (avatarGO == null) return;

        var geometry = playerArmature.transform.Find("Geometry");
        if (geometry == null)
        {
            Debug.LogError("[MyCharacterCore] Geometry not found under playerArmature.");
            return;
        }

        avatarGO.transform.SetParent(geometry, false);
        avatarGO.transform.localPosition = Vector3.zero;

        var targetAnimator = playerArmature.GetComponent<Animator>();
        var sourceAnimator = avatarGO.GetComponent<Animator>();

        if (targetAnimator == null || sourceAnimator == null)
        {
            Debug.LogError("[MyCharacterCore] Animator missing on playerArmature or avatar prefab.");
            return;
        }

        if (sourceAnimator.avatar == null)
        {
            Debug.LogError("[MyCharacterCore] Source avatar animator has no humanoid Avatar assigned.");
            return;
        }

        // 1) Avatar 교체
        targetAnimator.avatar = sourceAnimator.avatar;

        // 2) 정석: 바인딩/상태 재초기화
        targetAnimator.Rebind();

        // 3) 즉시 1프레임 갱신(첫 프레임 T-pose/정지 방지)
        targetAnimator.Update(0f);

        Debug.Log(">> Player Avatar Swapped (Rebind) >>>>>>>>>>>>>>>>>");

        //
        c_type = charType;
    }
}
