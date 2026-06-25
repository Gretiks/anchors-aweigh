using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Grid;
using System.Collections;
using System.Linq;
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
            else if (SceneManager.GetActiveScene().name == "BossScene")
                ChangeState(GameState.GenerateBossGrid);
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
                case GameState.GenerateBossGrid:
                    GridManager.Instance.GenerateBossGrid();
                    break;
                case GameState.SpawnUserCrew:
                    UnitManager.Instance.SpawnUnits();
                    ShipManager.Instance.InitiatePlayerShip();
                    
                    if(ShipManager.Instance.playerShip != null &&
                       PlayerDataManager.Instance != null)
                        PlayerDataManager.Instance.TryLoadingShipState(ShipManager.Instance.playerShip);
                    
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
            if(GameState == GameState.UserTurn)
                ChangeState(GameState.EnemyTurn);
        }

        private IEnumerator EnemyTurnRoutine()
        {
            Debug.Log("-----TURA AI-----");
            
            ProcessHelmMovement(Faction.User);
            CheckBattleConditions();
            
            BaseEnemy.AssignEnemiesToModules();
            
            foreach (var enemy in UnitManager.Instance._enemies)
            {
                if (enemy == null || !enemy.gameObject.activeInHierarchy || enemy.currentHealth <= 0) 
                    continue;

                enemy.StartTurnAction();
                
                while (!enemy.isActionCompleted)
                    yield return null;
            }
            
            ProcessEnemyCannons();
            
            var enemyShip = ShipManager.Instance.enemyShip;
            if (enemyShip != null)
                enemyShip.ExecuteShipTurnMovement();
            
            ProcessHelmMovement(Faction.Enemy);
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
            MenuManager.Instance.RefreshEnemyList(UnitManager.Instance._enemies);
        }

        private void ResetCannonsFired()
        {
            var cannons = FindObjectsByType<CannonTile>();
            foreach (var cannon in cannons)
                cannon.ResetFired();
        }

        private void ProcessHelmMovement(Faction targetFaction)
        {
            var helms = FindObjectsByType<HelmTile>();
            foreach (var helm in helms)
            {
                if (!helm.HasCrew) continue;

                if (helm.owner == targetFaction)
                {
                    if (helm.owner == Faction.User)
                        ShipManager.Instance.playerShip.MoveShip(helm.GetDirectionForShip());
                    else
                        ShipManager.Instance.enemyShip.MoveShip(helm.GetDirectionForShip());
                }
            }
        }

        public void CheckBattleConditions()
        {
            if (SceneManager.GetActiveScene().name == "BoardingScene")
            {
                if (UnitManager.Instance == null) return;

                int aliveHeroes = UnitManager.Instance._heroes.Count(h => h != null && h.gameObject.activeInHierarchy && h.currentHealth > 0);
                int aliveEnemies = UnitManager.Instance._enemies.Count(e => e != null && e.gameObject.activeInHierarchy && e.currentHealth > 0);

                if (aliveHeroes == 0)
                {
                    Debug.Log("Wszyscy Twoi piraci polegli! Przegrana bitwa aborda?owa.");
                    TriggerEndGame(false); 
                    return;
                }
                else if (aliveEnemies == 0)
                {
                    Debug.Log("Wroga za?oga zosta?a wyci?ta w pie?! Zwyci?stwo w aborda?u!");
                    TriggerEndGame(true); 
                    return;
                }

                return; 
            }
            
            float playerPos = ShipManager.Instance.playerShip.Position;
            float enemyPos = ShipManager.Instance.enemyShip.Position;
    
            // Pobieramy te? pozycj? z poprzedniego klatki/ruchu, je?li j? przechowujesz, 
            // LUB stosujemy prost? logik? "przeci?cia" w tej turze:
    
            var playerShip = ShipManager.Instance.playerShip;
            var enemyShip = ShipManager.Instance.enemyShip;

            if (playerShip == null || enemyShip == null) return;

            if (playerShip.currentHealth <= 0)
            {
                Debug.Log("Tw?j statek zaton??! Przegrana.");
                TriggerEndGame(false); // false = przegrana
            }
            // 2. Je?li statek wroga ma 0 lub mniej HP -> ZWYCI?STWO
            else if (enemyShip.currentHealth <= 0)
            {
                Debug.Log("Statek wroga zaton??! Zwyci?stwo!");
                TriggerEndGame(true); // true = zwyci?stwo
            }
            else
            {
                // =====================================================================
                // [ZMIANA]: Logika wykrywania aborda?u (przeskoczenie lub zrównanie)
                // =====================================================================
        
                // Obliczamy odleg?o?? teraz
                float distanceNow = playerPos - enemyPos;
        
                // Pobieramy pr?dko?? (uwzgl?dniamy bonus pr?dko?ci z PlayerShip)
                float playerSpeed = playerShip.speed + playerShip.ExtraSpeed;
                float enemySpeed = enemyShip.speed; // (je?li enemy te? ma bonus, dodaj go tutaj)
        
                // Je?li statki zmieni?y relatywn? pozycj? tak, ?e "przeskoczy?y" siebie
                // lub znalaz?y si? w zasi?gu 1f, wywo?ujemy aborda?.
                if (Mathf.Abs(distanceNow) <= 1.5f) // Zwi?kszy?em margines na 1.5f dla pewno?ci
                {
                    Debug.Log("Aborda?: Statki si? zrówna?y lub przeskoczy?y!");
            
                    // Zapis stanu przed wej?ciem w aborda?
                    PlayerDataManager.Instance.SaveShipState(playerShip);
            
                    foreach(var hero in UnitManager.Instance._heroes)
                        if(hero != null) PlayerDataManager.Instance.SaveUnitState(hero);
            
                    foreach(var enemy in UnitManager.Instance._enemies)
                        if(enemy != null) PlayerDataManager.Instance.SaveUnitState(enemy);
            
                    SceneManager.LoadScene("BoardingScene");
                }
                
            }

        }

        private void TriggerEndGame(bool victory)
        {
            if (PlayerDataManager.Instance != null)
            {
                if (victory) PlayerDataManager.Instance.IncrementBattlesWon();

                if (ShipManager.Instance != null && ShipManager.Instance.playerShip != null)
                {
                    PlayerDataManager.Instance.SaveShipState(ShipManager.Instance.playerShip);
                }

                if (UnitManager.Instance != null)
                {
                    foreach (var hero in UnitManager.Instance._heroes)
                    {
                        if (hero != null) PlayerDataManager.Instance.SaveUnitState(hero);
                    }
                    foreach (var enemy in UnitManager.Instance._enemies)
                    {
                        if (enemy != null) PlayerDataManager.Instance.SaveUnitState(enemy);
                    }
                }
            }
            
            EndGameManager.IsVictory = victory;
            SceneManager.LoadScene("EndGameScene");
        }
    }

    public enum GameState
    {
        GenerateGrid = 0,
        GenerateBoardingGrid = 1,
        GenerateBossGrid =2,
        SpawnUserCrew = 3,
        SpawnEnemyCrew = 4,
        UserTurn = 5,
        EnemyTurn = 6
    }
}