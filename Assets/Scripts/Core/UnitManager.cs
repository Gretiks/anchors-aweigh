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
        
        var heroCount = 3;
        for (int i = 0; i < heroCount; i++)
        {
            string slotID = "Player_Hero_" + i.ToString();
            BaseHero prefabToSpawn;
            bool isDead = false;
            
            // 2. Sprawdzamy, czy wracamy z bitwy morskiej i czy ten slot ma historię
            if (PlayerDataManager.Instance != null && 
                PlayerDataManager.Instance.HasExistingSave && 
                PlayerDataManager.Instance.TryGetUnitSaveData(slotID, out var savedData))
            {
                // Odtwarzamy DOKŁADNIE ten sam prefab, który walczył wcześniej
                prefabToSpawn = GetUnitPrefabByName<BaseHero>(savedData.unitName, Faction.User);
                
                if (savedData.currentHealth <= 0)
                    isDead = true;
            }
            else
                // Pierwsza walka w grze - losujemy w ciemno
                prefabToSpawn = GetRandomUnit<BaseHero>(Faction.User);

            // 3. Tworzymy fizyczny obiekt
            var spawnedHero = Instantiate(prefabToSpawn);
            spawnedHero.uniqueID = slotID;
            

            if (PlayerDataManager.Instance != null && PlayerDataManager.Instance.HasExistingSave)
                PlayerDataManager.Instance.TryLoadUnitState(spawnedHero);
            
            if (isDead)
                spawnedHero.gameObject.SetActive(false);
            else
            {
                var randomSpawnTile = GridManager.Instance.GetHeroSpawnTile();
                randomSpawnTile.SetUnit(spawnedHero);
                spawnedHero.OccupiedTile = randomSpawnTile;
            }

            _heroes.Add(spawnedHero);
        }

        MenuManager.Instance.RefreshHeroList(_heroes);
        GameManager.Instance.ChangeState(GameState.SpawnEnemyCrew);
    }

    public void SpawnEnemy()
    {
        // 1. FIZYCZNE USUNIĘCIE starych wrogów ze sceny
        foreach (var enemy in _enemies)
        {
            if (enemy != null) Destroy(enemy.gameObject);
        }
        _enemies.Clear();
        
        var enemyCount = 3;
        for (int i = 0; i < enemyCount; i++)
        {
            // Nadajemy stały slotID przeciwnikowi, aby dało się go zapisać
            string slotID = "Enemy_Crew_" + i.ToString();
            BaseEnemy prefabToSpawn;
            bool isDead = false;

            if (PlayerDataManager.Instance != null && 
                PlayerDataManager.Instance.HasExistingSave && 
                PlayerDataManager.Instance.TryGetUnitSaveData(slotID, out var savedData))
            {
                prefabToSpawn = GetUnitPrefabByName<BaseEnemy>(savedData.unitName, Faction.Enemy);
                
                if (savedData.currentHealth <= 0)
                    isDead = true;
            }
            else
                prefabToSpawn = GetRandomUnit<BaseEnemy>(Faction.Enemy);

            var spawnedEnemy = Instantiate(prefabToSpawn);
            spawnedEnemy.uniqueID = slotID; 

            // Wczytanie HP wroga z poprzedniej sceny
            if (PlayerDataManager.Instance != null && PlayerDataManager.Instance.HasExistingSave)
                PlayerDataManager.Instance.TryLoadUnitState(spawnedEnemy);

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
        return (T)_units.Where(u=>u.Faction == faction).OrderBy(o=>Random.value).First().UnitPrefab;
    }

    private T GetUnitPrefabByName<T>(string unitName, Faction faction) where T : BaseUnit
    {
        var found = _units.FirstOrDefault(u => u.Faction == faction && u.UnitPrefab.unitName == unitName);
        if (found != null && found.UnitPrefab is T match)
        {
            return match;
        }
        
        Debug.LogWarning($"Nie odnaleziono prefabu '{unitName}'. Losowanie zastępstwa.");
        return GetRandomUnit<T>(faction);
    }
    
    public void SetSelectedHero(BaseHero hero)
    {
        SelectedHero = hero;
        MenuManager.Instance.ShowSelectedHero(hero);
        if (hero != null)
            GridManager.Instance.ShowRangeHighlights(hero);
        else
            GridManager.Instance.ClearRangeHighlights();
    }

    public void AttackEnemyWithSelectedHero(BaseEnemy enemy)
    {
        if (SelectedHero == null)
        {
            // Debug.LogWarning("Nie zaznaczono żadnego bohatera, który mógłby zaatakować!");
            return;
        }

        if (SelectedHero.hasAttacked)
            return;
        
        if (enemy == null || enemy.currentHealth <= 0) 
            return;

        if (SelectedHero.OccupiedTile == null || enemy.OccupiedTile == null) 
            return;

        var neighbors = Grid.GridManager.Instance.GetNeighbors(SelectedHero.OccupiedTile);

        if (neighbors.Contains(enemy.OccupiedTile))
        {
            float playerAttackDamage = 35f; 

            // Debug.Log($"{SelectedHero.unitName} potężnym ciosem atakuje {enemy.unitName} i zadaje {playerAttackDamage} obrażeń!");
            
            enemy.TakeDamage(playerAttackDamage);

            SelectedHero.hasAttacked = true;
            
            if (Core.GameManager.Instance != null)
                Core.GameManager.Instance.CheckBattleConditions();
        }
        
            // Debug.Log("Przeciwnik stoi za daleko! Podejdź bliżej, aby zaatakować wręcz.");
    }

}
