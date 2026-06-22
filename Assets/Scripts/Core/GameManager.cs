using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Grid;
using System.Collections;
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
            if (SceneManager.GetActiveScene().name == "BoardingScene")
                ChangeState(GameState.GenerateBoardingGrid);
            else
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
                case GameState.GenerateBoardingGrid:
                    GridManager.Instance.GenerateBoardingGrid();
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
                    ResetEnemyMovment();
                    StartCoroutine(EnemyTurnRoutine());
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
            }
        }

        public void EndTurn()
        {
            ChangeState(GameState.EnemyTurn);
        }

        private IEnumerator EnemyTurnRoutine()
        {
            Debug.Log("-----TURA AI-----");
            
            BaseEnemy.AssignEnemiesToModules();
            
            foreach (var enemy in UnitManager.Instance._enemies)
            {
                if (enemy == null) continue;

                enemy.StartTurnAction();
                
                while (!enemy.isActionCompleted)
                {
                    yield return null;
                }
            }
            
            ProcessEnemyCannons();
            
            var enemyShip = ShipManager.Instance.enemyShip;
            if (enemyShip != null)
                enemyShip.ExecuteShipTurnMovement();
            
            
            
            ProcessHelmMovement();
            CheckBattleConditions();
            
            ChangeState(GameState.UserTurn);
        }

        private void ProcessEnemyCannons()
        {
            var cannons = FindObjectsByType<CannonTile>();
            foreach (var cannon in cannons)
            {
                if (cannon.owner == Faction.Enemy)
                {
                    cannon.EnemyExecuteFire();
                }
            }
        }
        
        private void ResetHeroMovement()
        {
            foreach (var hero in UnitManager.Instance._heroes)
                hero.ResetMovement();
            MenuManager.Instance.RefreshHeroList(UnitManager.Instance._heroes);
        }

        private void ResetEnemyMovment()
        {
            foreach (var enemy in UnitManager.Instance._enemies)
                enemy.ResetMovement();
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

        public void CheckBattleConditions()
        {
            float playerPos = ShipManager.Instance.playerShip.Position;
            float enemyPos = ShipManager.Instance.enemyShip.Position;

            // ZA?O?ENIE LOGICZNE:
            // Je?li statek wroga dop?yn?? do granicy (zbieg?) lub nasz zaton?? -> PRZEGRANA
            // Je?li statek wroga zaton?? lub my osi?gn?li?my cel -> ZWYCI?STWO
            // Dostosuj te warunki do dok?adnych regu? Twojego projektu (np. HP statku, pozycja)

            // Pobieramy instancje statków z ShipManagera
            var playerShip = ShipManager.Instance.playerShip;
            var enemyShip = ShipManager.Instance.enemyShip;

            // Upewniamy si?, ?e statki istniej?, aby unikn?? b??dów NullReferenceException
            if (playerShip == null || enemyShip == null) return;

            // WARUNKI KO?CA GRY OPARTY O HP STATKÓW:
            // 1. Je?li nasz statek ma 0 lub mniej HP -> PRZEGRANA
            if (playerShip.currentHealth <= 0)
            {
                Debug.Log("Twój statek zaton??! Przegrana.");
                TriggerEndGame(false); // false = przegrana
            }
            // 2. Je?li statek wroga ma 0 lub mniej HP -> ZWYCI?STWO
            else if (enemyShip.currentHealth <= 0)
            {
                Debug.Log("Statek wroga zaton??! Zwyci?stwo!");
                TriggerEndGame(true); // true = zwyci?stwo
            }
            
            else if (Mathf.Abs(playerPos - enemyPos) < 1f) 
            {
                Debug.Log("Boarding phase");
                SceneManager.LoadScene("BoardingScene");
            }
        }

        // NOWA METODA POMOCNICZA:
        private void TriggerEndGame(bool victory)
        {
            // 1. Ustawiamy statyczn? flag? w skrypcie ekranu ko?cowego
            EndGameManager.IsVictory = victory;

            // 2. ?adujemy scen? ko?cow? po nazwie wpisanej w Build Settings
            SceneManager.LoadScene("EndGameScene");
        }
    }

    public enum GameState
    {
        GenerateGrid = 0,
        GenerateBoardingGrid = 1,
        SpawnUserCrew = 2,
        SpawnEnemyCrew = 3,
        UserTurn = 4,
        EnemyTurn = 5
    }
}