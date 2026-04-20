using System;
using UnityEngine;
using Grid;

namespace Core
{

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public GameState GameState;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        ChangeState(GameState.GenerateGrid);
    }

    public void ChangeState(GameState newState)
    {
        GameState = newState;
        switch (newState)
        {
            case GameState.GenerateGrid:
                GridManager.Instance.GenerateGrid();
                break;
            case GameState.SpawnUserCrew:
                UnitManager.Instance.SpawnUnits();
                ShipManager.Instance.InitiatePlayerShip();
                
                ShipManager.Instance.playerShip.UpdateShipUI();
                break;
            case GameState.SpawnEnemyCrew:
                UnitManager.Instance.SpawnEnemy();
                ShipManager.Instance.InitiateEnemyShip();
                
                ShipManager.Instance.enemyShip.UpdateShipUI();
                
                break;
            case GameState.UserTurn:
                break;
            case GameState.EnemyTurn:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
            
        }
    }
}

public enum GameState
{
    GenerateGrid = 0,
    SpawnUserCrew = 1,
    SpawnEnemyCrew = 2,
    UserTurn = 3,
    EnemyTurn = 4
}
}

