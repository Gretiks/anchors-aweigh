using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Core;
using Grid;
using Random = UnityEngine.Random;

public class UnitManager : MonoBehaviour
{
    public static UnitManager Instance;

    private List<ScriptableUnit> _units;

    public BaseHero SelectedHero;

    public List<BaseHero> _heroes = new List<BaseHero>();
    public List<BaseEnemy> _enemies = new List<BaseEnemy>();

    private float meleeBonus = 0;
    
    void Awake()
    {
        Instance = this;
        _units = Resources.LoadAll<ScriptableUnit>("Units").ToList();
    }

    public void SpawnUnits()
    {
        foreach (var hero in _heroes)
            if(hero != null) Destroy(hero.gameObject);
        
        _heroes.Clear();
        
        int currentPlayerSlots = PlayerDataManager.Instance != null ? PlayerDataManager.Instance.PlayerSlotsCount : 3;

        for (int i = 0; i < currentPlayerSlots; i++)
        {
            string slotID = "Player_Hero_" + i.ToString();
            BaseHero prefabToSpawn = null;
            bool shouldSpawn = false;
            bool isFromSave = false;

            if (PlayerDataManager.Instance != null && PlayerDataManager.Instance.HasExistingSave)
            {
                if (PlayerDataManager.Instance.TryGetUnitSaveData(slotID, out var savedData))
                {
                    if (savedData.currentHealth > 0)
                    {
                        prefabToSpawn = GetUnitPrefabByAssetName<BaseHero>(savedData.prefabName, Faction.User);
                        shouldSpawn = true;
                        isFromSave = true;
                    }
                    else
                    {
                        PlayerDataManager.Instance.RemoveUnitSaveData(slotID);
                        shouldSpawn = false;
                    }
                }
                else
                    shouldSpawn = false;
            }
            else
            {
                if (i < 3)
                {
                    prefabToSpawn = GetRandomUnit<BaseHero>(Faction.User);
                    shouldSpawn = true;
                    isFromSave = false;
                }
                else
                    shouldSpawn = false; 
            }

            if (shouldSpawn && prefabToSpawn != null)
            {
                var spawnedHero = Instantiate(prefabToSpawn);
                spawnedHero.uniqueID = slotID;

                if (isFromSave)
                {
                    PlayerDataManager.Instance.TryLoadUnitState(spawnedHero);
                }
                else
                {
                    spawnedHero.prefabName = prefabToSpawn.name; 
                    spawnedHero.unitName = PlayerDataManager.Instance != null ? PlayerDataManager.Instance.GetRandomPolishName() : "Pirat"; 
                }

                var randomSpawnTile = GridManager.Instance.GetHeroSpawnTile();
                randomSpawnTile.SetUnit(spawnedHero);
                spawnedHero.OccupiedTile = randomSpawnTile;

                _heroes.Add(spawnedHero);
            }
        }

        MenuManager.Instance.RefreshHeroList(_heroes);
        GameManager.Instance.ChangeState(GameState.SpawnEnemyCrew);
    }

    public void SpawnEnemy()
    {
        foreach (var enemy in _enemies)
            if (enemy != null) Destroy(enemy.gameObject);
        
        _enemies.Clear();
        
        bool isBoardingScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "BoardingScene";

        int currentEnemyCount = PlayerDataManager.Instance != null ? PlayerDataManager.Instance.EnemySlotsCount : 3;

        if (PlayerDataManager.Instance != null && PlayerDataManager.Instance.IsNextBattleBoss())
            currentEnemyCount = 7; // tylko na walke z bossem
        
        for (int i = 0; i < currentEnemyCount; i++)
        {
            string slotID = "Enemy_Crew_" + i.ToString();
            BaseEnemy prefabToSpawn = null;
            bool isDead = false;
            bool isFromSave = false;

            if (isBoardingScene && 
                PlayerDataManager.Instance != null && 
                PlayerDataManager.Instance.HasExistingSave &&
                PlayerDataManager.Instance.TryGetUnitSaveData(slotID, out var savedData))
            {
                prefabToSpawn = GetUnitPrefabByAssetName<BaseEnemy>(savedData.prefabName, Faction.Enemy);
                isFromSave = true;

                if (savedData.currentHealth <= 0)
                    isDead = true;
            }
            else
            {
                prefabToSpawn = GetRandomUnit<BaseEnemy>(Faction.Enemy);
                isFromSave = false;
            }

            var spawnedEnemy = Instantiate(prefabToSpawn);
            spawnedEnemy.uniqueID = slotID; 

            if (isFromSave)
            {
                PlayerDataManager.Instance.TryLoadUnitState(spawnedEnemy);
            }
            else
            {
                spawnedEnemy.prefabName = prefabToSpawn.name;
                spawnedEnemy.unitName = PlayerDataManager.Instance != null ? PlayerDataManager.Instance.GetRandomPolishName() : "Wrog";
            }

            if (isDead)
                spawnedEnemy.gameObject.SetActive(false);
            else
            {
                var randomSpawnTile = GridManager.Instance.GetEnemySpawnTile();
                randomSpawnTile.SetUnit(spawnedEnemy);
                spawnedEnemy.OccupiedTile = randomSpawnTile;
            }

            _enemies.Add(spawnedEnemy);
        }
        MenuManager.Instance.RefreshEnemyList(_enemies);
        GameManager.Instance.ChangeState(GameState.UserTurn);
    }

    private T GetRandomUnit<T>(Faction faction) where T : BaseUnit
    {
        var validUnits = _units.Where(u => u.Faction == faction && u.UnitPrefab != null && u.UnitPrefab is T).ToList();
        if (validUnits.Count == 0) return null;
        return (T)validUnits[Random.Range(0, validUnits.Count)].UnitPrefab;
    }

    private T GetUnitPrefabByAssetName<T>(string assetName, Faction faction) where T : BaseUnit
    {
        var found = _units.FirstOrDefault(u => u.Faction == faction && u.UnitPrefab.name == assetName);
        if (found != null && found.UnitPrefab is T match) return match;
        return GetRandomUnit<T>(faction);
    }
    
    public void SetSelectedHero(BaseHero hero)
    {
        SelectedHero = hero;
        MenuManager.Instance.ShowSelectedHero(hero);
        if (hero != null) GridManager.Instance.ShowRangeHighlights(hero);
        else GridManager.Instance.ClearRangeHighlights();
    }

    public void AttackEnemyWithSelectedHero(BaseEnemy enemy)
    {
        if (SelectedHero == null || SelectedHero.hasAttacked || enemy == null || enemy.currentHealth <= 0) return;
        if (SelectedHero.OccupiedTile == null || enemy.OccupiedTile == null) return;

        meleeBonus = PlayerDataManager.Instance == null ? 0 : PlayerDataManager.Instance.BonusMeleeDamage;
        
        var neighbors = Grid.GridManager.Instance.GetNeighbors(SelectedHero.OccupiedTile);
        if (neighbors.Contains(enemy.OccupiedTile))
        {
            enemy.TakeDamage(35f + meleeBonus);
            SelectedHero.hasAttacked = true;
            if (Core.GameManager.Instance != null) Core.GameManager.Instance.CheckBattleConditions();
        }
    }
}