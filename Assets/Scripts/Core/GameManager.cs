using System;
using UnityEngine;
using Grid;
using Assets.Scripts.Grid.Tiles.Modules;

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
                    ResetHeroMovement();
                    ResetCannonsFired();
                    break;
                case GameState.EnemyTurn:
                    ProcessHelmMovement();
                    CheckBattleConditions();
                    ChangeState(GameState.UserTurn);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
            }
        }

        public void EndTurn()
        {
            ChangeState(GameState.EnemyTurn);
        }

        private void ResetHeroMovement()
        {
            foreach (var hero in UnitManager.Instance._heroes)
                hero.ResetMovement();
            MenuManager.Instance.RefreshHeroList(UnitManager.Instance._heroes);
        }

        private void ResetCannonsFired()
        {
            var cannons = FindObjectsByType<CannonTile>();
            foreach (var cannon in cannons)
                cannon.ResetFired();
        }

        private void ProcessHelmMovement()
        {
            var helms = FindObjectsByType<HelmTile>();
            foreach (var helm in helms)
            {
                if (!helm.HasCrew) continue;
                if (helm.owner == Faction.User)
                    ShipManager.Instance.playerShip.MoveShip(helm.GetDirectionForShip());
                else
                    ShipManager.Instance.enemyShip.MoveShip(helm.GetDirectionForShip());
            }
        }

        private void CheckBattleConditions()
        {
            float playerPos = ShipManager.Instance.playerShip.Position;
            float enemyPos = ShipManager.Instance.enemyShip.Position;

            if (playerPos <= -10f || enemyPos >= 10f)
                Debug.Log("Ucieczka!");
            else if (Mathf.Abs(playerPos - enemyPos) < 1f)
                Debug.Log("Aborda¿!");
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