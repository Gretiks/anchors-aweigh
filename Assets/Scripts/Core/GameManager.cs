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

        private float _previousDistance = float.NaN;

        AudioManager audioManager;

        void Awake()
        {
            Instance = this;
            audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
        }

        void Start()
        {
            if (SceneManager.GetActiveScene().name == "BoardingScene")
                ChangeState(GameState.GenerateBoardingGrid);
            else if (PlayerDataManager.Instance != null && PlayerDataManager.Instance.IsNextBattleBoss())
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
                    MenuManager.Instance.ShowTurnIndicator(true);
                    audioManager.PlaySFX(audioManager.coin);
                    break;
                case GameState.EnemyTurn:
                    ResetEnemyMovment();
                    MenuManager.Instance.ShowTurnIndicator(false);
                    audioManager.PlaySFX(audioManager.coin);
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
            
            yield return ProcessEnemyCannons();
            
            var enemyShip = ShipManager.Instance.enemyShip;
            if (enemyShip != null)
                enemyShip.ExecuteShipTurnMovement();
            
            ProcessHelmMovement(Faction.Enemy);
            CheckBattleConditions();
            
            ChangeState(GameState.UserTurn);
        }

        private IEnumerator ProcessEnemyCannons()
        {
            var cannons = FindObjectsByType<CannonTile>();
            foreach (var cannon in cannons)
            {
                if (cannon.owner == Faction.Enemy)
                {
                    cannon.EnemyExecuteFire();
                    yield return new WaitForSeconds(0.25f);
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
    
    var playerShip = ShipManager.Instance.playerShip;
    var enemyShip = ShipManager.Instance.enemyShip;

    if (playerShip == null || enemyShip == null) return;

    if (playerShip.currentHealth <= 0)
    {
        Debug.Log("Twój statek zaton??! Przegrana.");
        TriggerEndGame(false); 
    }
    else if (enemyShip.currentHealth <= 0)
    {
        Debug.Log("Statek wroga zaton??! Zwyci?stwo!");
        TriggerEndGame(true); 
    }
    else
    {
        // =====================================================================
        // Logika wykrywania aborda?u (przeskoczenie lub zrównanie)
        // =====================================================================

        float playerPos = playerShip.Position;
        float enemyPos = enemyShip.Position;
        float distanceNow = playerPos - enemyPos;

        bool jumpedOver = false;

        // Sprawdzamy zmian? znaku dystansu, która oznacza wymini?cie si? statków
        if (!float.IsNaN(_previousDistance))
        {
            // Je?li znaki si? ró?ni?, statki przeci??y swoje ?cie?ki
            if (Mathf.Sign(distanceNow) != Mathf.Sign(_previousDistance) && distanceNow != 0 && _previousDistance != 0)
            {
                jumpedOver = true;
            }
        }

        // Aktualizujemy poprzedni dystans na potrzeby przysz?ych wywo?a?
        _previousDistance = distanceNow;

        // Wywo?ujemy aborda?, je?li statki s? w zasi?gu 1.5f LUB si? przeskoczy?y
        if (Mathf.Abs(distanceNow) <= 1.5f || jumpedOver)
        {
            Debug.Log("Aborda?: Statki si? zrówna?y lub przeskoczy?y!");
    
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
                if (victory)
                {
                    if(PlayerDataManager.Instance.IsNextBattleBoss())
                        PlayerDataManager.Instance.MarkBossAsDefeated();
                    
                    PlayerDataManager.Instance.IncrementBattlesWon();
                }

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