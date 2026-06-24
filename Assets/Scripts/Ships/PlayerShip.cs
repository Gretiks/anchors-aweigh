using System;
using UnityEngine;

public class PlayerShip : BaseShip
{
    protected override Faction GetFaction() => Faction.User;

    [SerializeField] private float playerMaxHealth = 100f;

    public override float maxHealth
    {
        get
        {
            float bonus = 0f;
            if (PlayerDataManager.Instance != null)
                bonus = PlayerDataManager.Instance.BonusHp;

            return playerMaxHealth + bonus;
        }
    }

    // =====================================================================
    // Nadpisanie uniku statku gracza (teraz zadziała bezbłędnie)
    // =====================================================================
    public override float evasion
    {
        get
        {
            float extraEvasion = PlayerDataManager.Instance != null ? PlayerDataManager.Instance.BonusEvasion : 0f;
            float reduceEvasion = 0; //debuff przez przecwnika
            return base.evasion + extraEvasion - reduceEvasion;
        }
    }

    // =====================================================================
    // Nadpisanie dodatkowej prędkości statku gracza
    // =====================================================================
    public override float ExtraSpeed
    {
        get
        {
            return PlayerDataManager.Instance != null ? PlayerDataManager.Instance.BonusShipSpeed : 0f;
        }
    }
	
    void Start()
    {
    }

    void Update()
    {
    }
}