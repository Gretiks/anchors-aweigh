using UnityEngine;

public class BossShip : EnemyShip
{
    // Całkowicie nadpisujemy logikę HP ze statku wroga, ignorując bonusy z PlayerDataManager.
    // Dzięki temu Boss zawsze ma równe 500 HP.
    public override float maxHealth => 500f;

    private void Awake()
    {
        base.Awake();
        ShipName = "Boss";
    }
}