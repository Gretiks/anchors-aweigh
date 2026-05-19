using Assets.Scripts.Grid.Tiles.Modules;
using Core;
using Grid;
using UnityEngine;
using System.Collections;

public class CannonTile : ShipModuleTile
{
    [SerializeField] public int requiredCrew = 2;
    [SerializeField] public int damage = 20;
    [SerializeField] public string cannonDescription = "Standard cannon";
    public bool HasFired { get; private set; } = false;

    protected override void OnMouseDown()
    {
        if (GameManager.Instance.GameState != GameState.UserTurn) return;
        if (owner != Faction.User) return;
        MenuManager.Instance.ShowCannonMenu(this);
    }

    public void Fire()
    {
        if (HasFired) return;
        BaseShip targetShip = owner == Faction.User ? (BaseShip)ShipManager.Instance.enemyShip : (BaseShip)ShipManager.Instance.playerShip;

        bool hit = Random.value > targetShip.evasion;
        if (hit) targetShip.TakeDamange(damage);
        HasFired = true;
        MenuManager.Instance.ShowHitPopup(hit);
        MenuManager.Instance.HideCannonMenu();
    }

    
    //AI Shooting
    
    public bool IsFiringCompleted { get; private set; } = true;

    public void EnemyExecuteFire()
    {
        if (HasFired) return;
        
        if (CurrentCrew >= requiredCrew) 
        {
            
            BaseShip targetShip = (BaseShip)ShipManager.Instance.playerShip;

            bool hit = Random.value > targetShip.evasion;
            if (hit) targetShip.TakeDamange(damage);
        
            HasFired = true;
            
            MenuManager.Instance.ShowHitPopup(hit);
        }
    }
    
    
    public void ResetFired() { HasFired = false; }
}
