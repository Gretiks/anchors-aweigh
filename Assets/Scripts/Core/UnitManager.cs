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

    public List<BaseHero> _heroes;
    

    void Awake()
    {
        Instance = this;

        _units = Resources.LoadAll<ScriptableUnit>("Units").ToList();
        // Console.WriteLine(_units.Count);
    }

    public void SpawnUnits()
    {
        var heroCount = 3;

        for (int i = 0; i < heroCount; i++)
        {
            var randomPrefab = GetRandomUnit<BaseHero>(Faction.User);
            var spawnedHero = Instantiate(randomPrefab);
            var randomSpawnTile = GridManager.Instance.GetHeroSpawnTile();

            spawnedHero.transform.position = randomSpawnTile.transform.position;
            randomSpawnTile.OccupiedUnit = spawnedHero;
            spawnedHero.OccupiedTile = randomSpawnTile;

            randomSpawnTile.SetUnit(spawnedHero);

            _heroes.Add(spawnedHero);
        }

        MenuManager.Instance.RefreshHeroList(_heroes);

        GameManager.Instance.ChangeState(GameState.SpawnEnemyCrew);
    }
    
    public void SpawnEnemy()
    {
        var enemyCount = 3;

        for (int i = 0; i < enemyCount; i++)
        {
            var randomPrefab = GetRandomUnit<BaseEnemy>(Faction.Enemy);
            var spawnedEnemy = Instantiate(randomPrefab);
            var randomSpawnTile = GridManager.Instance.GetEnemySpawnTile();

            spawnedEnemy.transform.position = randomSpawnTile.transform.position;
            randomSpawnTile.OccupiedUnit = spawnedEnemy;
            spawnedEnemy.OccupiedTile = randomSpawnTile;
            
            randomSpawnTile.SetUnit(spawnedEnemy);
        }
        GameManager.Instance.ChangeState(GameState.UserTurn);
    }
    
    private T GetRandomUnit<T>(Faction faction) where T : BaseUnit
    {
        return (T)_units.Where(u=>u.Faction == faction).OrderBy(o=>Random.value).First().UnitPrefab;
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
    
}
