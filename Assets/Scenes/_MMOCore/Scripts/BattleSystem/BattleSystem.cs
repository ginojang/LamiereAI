using UnityEngine;

public class BattleSystem : MonoBehaviour
{
    public CombatTickDriver driver;
    public PlantMonsterFSM plant;
    public PlayerBattler player;
    public MonsterBattler monster; // plant¿« self

    void Start()
    {
        if (!driver) driver = FindFirstObjectByType<CombatTickDriver>();
        if (!plant) plant = FindFirstObjectByType<PlantMonsterFSM>();
        if (!player) player = FindFirstObjectByType<PlayerBattler>();
        if (!monster) monster = plant.self as MonsterBattler;

        plant.target = player;

        driver.Begin(0.2f, tick => plant.OnTick(tick));
    }


}