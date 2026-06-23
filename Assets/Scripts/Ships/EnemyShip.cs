using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum EnemyStrategy
{
    Shooting,
    Meele
}

public class EnemyShip : BaseShip
{
    protected override Faction GetFaction() => Faction.Enemy;
    public EnemyStrategy ShipStrategy { get; private set; }

    private Assets.Scripts.Grid.Tiles.Modules.HelmTile _enemyHelm;
    

    private void Awake()
    {
        base.Awake();
        InitializeShipStrategy();
        FindAndSetEnemyHelp();
    }

    private void InitializeShipStrategy()
    {
        if(SceneManager.GetActiveScene().name == "BoardingScene")
            return;
        
        int randomValue = UnityEngine.Random.Range(0, 2);

        if (randomValue == 0)
        {
            ShipStrategy = EnemyStrategy.Shooting;
            Debug.Log($"[STATEK] {gameObject.name} przyjął strategię: STRZELANIA");
        }
        else
        {
            ShipStrategy = EnemyStrategy.Meele;
            Debug.Log($"[STATEK] {gameObject.name} przyjął strategię: Abordazu");
        }
    }

    private void FindAndSetEnemyHelp()
    {
        var allHelms = FindObjectsByType<Assets.Scripts.Grid.Tiles.Modules.HelmTile>();
        foreach (var helm in allHelms)
            if (helm.owner == Faction.Enemy)
            {
                _enemyHelm = helm;
                break;
            }
    }

    public void ExecuteShipTurnMovement()
    {
        if (_enemyHelm == null)
        {
            Debug.LogWarning($"[STATEK] {gameObject.name} nie ma przypisanego steru");
            return;
        }
        
        if(ShipStrategy == EnemyStrategy.Meele)
            _enemyHelm.SetOrder(Assets.Scripts.Grid.Tiles.Modules.HelmTile.HelmOrder.Approach);
        else if(ShipStrategy == EnemyStrategy.Shooting)
            _enemyHelm.SetOrder(Assets.Scripts.Grid.Tiles.Modules.HelmTile.HelmOrder.Stop);
        
            
    }
    
    
}
	
