using UnityEngine;

public class BattleSystem : MonoBehaviour
{
    [Header("Tick")]
    public float tickInterval = 0.2f;

    [Header("Refs")]
    public PlayerBattler player;
    public MonsterBattler monster;

    [Header("Test Values")]
    [Range(0f, 1f)] public float playerAtk01 = 0.08f;
    [Range(0f, 1f)] public float monsterAtk01 = 0.05f;

    private CombatResolver _resolver = new CombatResolver();
    private CombatTickDriver _tick;

    void Awake()
    {
        _tick = GetComponent<CombatTickDriver>();
        if (_tick == null) _tick = gameObject.AddComponent<CombatTickDriver>();
    }

    void Start()
    {
        // 자동 할당(씬에 1개씩 있다고 가정)
        if (player == null) player = FindFirstObjectByType<PlayerBattler>();
        if (monster == null) monster = FindFirstObjectByType<MonsterBattler>();

        _tick.Begin(tickInterval, OnTick);
    }

    private void OnTick(float nowTime)
    {
        if (player == null || monster == null) return;

        // 종료 조건 (테스트용)
        if (!player.IsAlive || !monster.IsAlive)
        {
            _tick.Stop();
            return;
        }

        // 테스트: 서로 오토어택만
        //_resolver.TryAutoAttack(player, monster, nowTime, playerAtk01);
        //_resolver.TryAutoAttack(monster, player, nowTime, monsterAtk01);
    }
}