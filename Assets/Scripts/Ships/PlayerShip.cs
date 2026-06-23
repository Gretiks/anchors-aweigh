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
	
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{

	}
}
