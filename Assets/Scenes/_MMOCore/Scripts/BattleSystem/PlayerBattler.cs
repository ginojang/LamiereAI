using UnityEngine;

public class PlayerBattler : BattlerBase
{
    [SerializeField] private Transform baseArmature;

    private void Awake()
    {
        if (baseArmature == null)
        {
            baseArmature = FindChildRecursive(transform, "Armature");

            if (baseArmature == null)
            {
                Debug.LogWarning($"[PlayerBattler] Armature not found in children of {name}");
                baseArmature = transform; // fallback
            }
        }
    }

    public override Transform CombatTransform => baseArmature != null ? baseArmature : transform;


    // 재귀 탐색 (안전하게)
    private Transform FindChildRecursive(Transform parent, string targetName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == targetName)
                return child;

            var result = FindChildRecursive(child, targetName);
            if (result != null)
                return result;
        }
        return null;
    }


    // 나중에 입력, 스킬, 타겟팅 연결
}

