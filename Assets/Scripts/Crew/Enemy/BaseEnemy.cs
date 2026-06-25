using System.Reflection;
using Assets.Scripts.Grid.Tiles.Modules;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Grid;
using UnityEngine.SceneManagement; 

public class BaseEnemy : BaseUnit
{
    public ShipModuleTile assignedModule;
    public bool isActionCompleted { get; private set; } = true;

    [SerializeField] private float tileStepDelay = 0.2f;

    [Header("Boarding AI (Walka wręcz)")]
    [Tooltip("Ile obrażeń zada wróg, gdy podejdzie do jednostki gracza")]
    [SerializeField] private float boardingAttackDamage = 25f;

    private AudioManager _audioManager;
    private AudioManager AudioManagerInstance
    {
        get
        {
            // Jeśli referencja jest pusta, wyszukaj ją
            if (_audioManager == null)
            {
                GameObject audioObject = GameObject.FindGameObjectWithTag("Audio");
                if (audioObject != null)
                {
                    _audioManager = audioObject.GetComponent<AudioManager>();
                }
                else
                {
                    Debug.LogWarning("Nie znaleziono obiektu z tagiem 'Audio' w scenie.");
                }
            }
            return _audioManager;
        }
    }


    public void SetTargetModule(ShipModuleTile module)
    {
        assignedModule = module;
    }

    public void StartTurnAction()
    {
        isActionCompleted = false;
        StartCoroutine(MoveAndActRoutine());
    }

    // --- GŁÓWNY ROUTER LOGIKI AI ---
    private IEnumerator MoveAndActRoutine()
    {
        yield return new WaitForSeconds(0.2f);

        if (SceneManager.GetActiveScene().name == "BoardingScene")
        {
            yield return StartCoroutine(ExecuteBoardingAI());
        }
        else
        {
            yield return StartCoroutine(ExecuteShipModulesAI());
        }

        yield return new WaitForSeconds(0.2f);
        isActionCompleted = true;
    }

    // =========================================================================
    // 1. NOWE AI: FAZA ABORDAŻU (Pościg i atak)
    // =========================================================================
    private IEnumerator ExecuteBoardingAI()
    {
        // 1. Pobieramy listę żywych bohaterów gracza
        var aliveHeroes = UnitManager.Instance._heroes
            .Where(h => h != null && h.gameObject.activeInHierarchy && h.currentHealth > 0)
            .ToList();

        if (aliveHeroes.Count == 0)
        {
            Debug.Log($"{unitName}: Brak celów do ataku na pokładzie.");
            yield break;
        }

        // Sortujemy cele od najbliższego do najdalszego
        var sortedHeroes = aliveHeroes
            .OrderBy(h => GetManhattanDistance(OccupiedTile, h.OccupiedTile))
            .ToList();

        List<Tile> bestPath = null;

        foreach (var targetHero in sortedHeroes)
        {
            if (targetHero.OccupiedTile == null) continue;

            var heroNeighbors = GridManager.Instance.GetNeighbors(targetHero.OccupiedTile);

            // A. Jeśli wróg JUŻ stoi na polu obok tego herosa -> nie ruszamy się, przechodzimy do bicia
            if (OccupiedTile != null && heroNeighbors.Contains(OccupiedTile))
            {
                bestPath = new List<Tile>(); 
                break;
            }

            // B. Szukamy drogi do wolnych kafelków wokół tego herosa
            var walkableTargetTiles = heroNeighbors
                .Where(t => t.Walkable && t.OccupiedUnit == null)
                .ToList();

            foreach (var targetTile in walkableTargetTiles)
            {
                var path = FindPathToTileBFS(targetTile);
                if (path != null && (bestPath == null || path.Count < bestPath.Count))
                {
                    bestPath = path;
                }
            }

            // Jeśli znaleźliśmy choć jedną poprawną drogę do tego herosa, nie sprawdzamy dalszych
            if (bestPath != null) break;
        }

        // 2. Realizacja ruchu (jeśli trasa istnieje i wymaga przejścia)
        if (bestPath != null && bestPath.Count > 0)
        {
            int stepsToTake = Mathf.Min(bestPath.Count, UnitMovement);
            Debug.Log($"{unitName} szarżuje na wroga. Pokona {stepsToTake} pól.");

            for (int i = 0; i < stepsToTake; i++)
            {
                Tile nextTile = bestPath[i];
                nextTile.SetUnit(this);
                UnitMovement -= 1;
                if (AudioManagerInstance != null)
                {
                    AudioManagerInstance.PlaySFX(AudioManagerInstance.pawn);
                }
                yield return new WaitForSeconds(tileStepDelay);
            }
        }

        // 3. FAZA ATAKU: Po zakończeniu ruchu sprawdzamy, kogo mamy obok siebie
        if (OccupiedTile != null)
        {
            var myNeighbors = GridManager.Instance.GetNeighbors(OccupiedTile);
            BaseHero victim = aliveHeroes.FirstOrDefault(h => h.OccupiedTile != null && myNeighbors.Contains(h.OccupiedTile));

            if (victim != null)
            {
                Debug.Log($"{unitName} wyprowadza cios wręcz w {victim.unitName} zadając {boardingAttackDamage} obrażeń!");
                if (AudioManagerInstance != null)
                {
                    AudioManagerInstance.PlaySFX(AudioManagerInstance.sword);
                }

                // Wywołanie Twojej natywnej metody odejmowania HP
                victim.TakeDamage(boardingAttackDamage);
                
                if(Core.GameManager.Instance !=null)
                    Core.GameManager.Instance.CheckBattleConditions();

                MenuManager.Instance.RefreshHeroList(UnitManager.Instance._heroes);

                yield return new WaitForSeconds(0.3f); // Krótki "hit-stop" dla lepszego feelingu
            }
        }
    }

    private int GetManhattanDistance(Tile a, Tile b)
    {
        if (a == null || b == null) return 9999;
        return Mathf.RoundToInt(Mathf.Abs(a.transform.position.x - b.transform.position.x) + 
                                Mathf.Abs(a.transform.position.y - b.transform.position.y));
    }

    // =========================================================================
    // 2. STARE AI: FAZA BITWY MORSKIEJ (Bieganie do modułów)
    // =========================================================================
    private IEnumerator ExecuteShipModulesAI()
    {
        if (assignedModule != null)
        {
            var neighbors = GridManager.Instance.GetNeighbors(assignedModule);

            if (OccupiedTile != null && neighbors.Contains(OccupiedTile))
            {
                // Debug.Log($"{unitName} stoi już na stanowisku przy module: {assignedModule.GetType().Name}");
            }
            else
            {
                var walkableTargets = neighbors.Where(t => t.Walkable && t.OccupiedUnit == null).ToList();
                List<Tile> bestPath = null;

                foreach (var targetTile in walkableTargets)
                {
                    var path = FindPathToTileBFS(targetTile);
                    if (path != null && (bestPath == null || path.Count < bestPath.Count))
                    {
                        bestPath = path;
                    }
                }

                if (bestPath != null && bestPath.Count > 0)
                {
                    int stepsToTake = Mathf.Min(bestPath.Count, UnitMovement);
                    // Debug.Log($"{unitName} planuje podróż do {assignedModule.GetType().Name}. Droga: {bestPath.Count} kratek. Wykona: {stepsToTake} kroków (Zasięg: {UnitMovement})");

                    for (int i = 0; i < stepsToTake; i++)
                    {
                        Tile nextTile = bestPath[i];
                        nextTile.SetUnit(this);
                        UnitMovement -= 1;
                        if (AudioManagerInstance != null)
                        {
                            AudioManagerInstance.PlaySFX(AudioManagerInstance.pawn);
                        }
                        yield return new WaitForSeconds(tileStepDelay);
                    }
                }
                else
                {
                    Debug.Log($"{unitName} chce iść do {assignedModule.GetType().Name}, ale droga jest całkowicie zablokowana.");
                }
            }
        }
        else
        {
            Debug.Log("No target module found");
        }
    }

    // Współdzielony algorytm szukania najkrótszej ścieżki
    private List<Tile> FindPathToTileBFS(Tile targetTile)
    {
        if (OccupiedTile == null || targetTile == null) return null;

        Queue<Tile> queue = new Queue<Tile>();
        HashSet<Tile> visited = new HashSet<Tile>();
        Dictionary<Tile, Tile> cameFrom = new Dictionary<Tile, Tile>();

        queue.Enqueue(OccupiedTile);
        visited.Add(OccupiedTile);

        bool pathFound = false;

        while (queue.Count > 0)
        {
            Tile current = queue.Dequeue();

            if (current == targetTile)
            {
                pathFound = true;
                break;
            }

            List<Tile> neighbors = GridManager.Instance.GetNeighbors(current);

            foreach (Tile neighbor in neighbors)
            {
                if (neighbor != null && !visited.Contains(neighbor))
                {
                    if (neighbor.name.Contains("Sea")) continue;

                    if (neighbor.Walkable || neighbor == targetTile)
                    {
                        visited.Add(neighbor);
                        cameFrom[neighbor] = current;
                        queue.Enqueue(neighbor);
                    }
                }
            }
        }

        if (pathFound)
        {
            List<Tile> path = new List<Tile>();
            Tile current = targetTile;

            while (current != OccupiedTile)
            {
                path.Add(current);
                current = cameFrom[current];
            }

            path.Reverse(); 
            return path;
        }

        return null; 
    }

    public static void AssignEnemiesToModules()
    {
        // [KLUCZOWY BEZPIECZNIK]: W scenie abordażu całkowicie blokujemy tę metodę
        if (SceneManager.GetActiveScene().name == "BoardingScene") 
            return;

        var allModules = FindObjectsByType<ShipModuleTile>().Where(m => m.owner == Faction.Enemy).ToList();

        EnemyShip enemyShip = FindFirstObjectByType<EnemyShip>();
        EnemyStrategy currentStrategy = EnemyStrategy.Shooting; 

        if (enemyShip != null)
            currentStrategy = enemyShip.ShipStrategy;
        else
            Debug.LogWarning("Nie znaleziony EnemyShip, wartosc domyslna");
        
        var sortedModules = allModules.OrderBy(m => GetModulePriority(m, currentStrategy)).ToList();

        HashSet<BaseEnemy> lockedEnemies = new HashSet<BaseEnemy>();

        foreach (var enemy in UnitManager.Instance._enemies)
        {
            if (enemy == null || !enemy.gameObject.activeInHierarchy || enemy.currentHealth <= 0)
                continue;

            if (enemy.assignedModule != null)
            {
                var neighbors = GridManager.Instance.GetNeighbors(enemy.assignedModule);
                
                if (enemy.OccupiedTile != null && neighbors.Contains(enemy.OccupiedTile))
                    lockedEnemies.Add(enemy);
                else
                    enemy.assignedModule = null;
            }
        }
        
        foreach (var module in sortedModules)
        {
            int currentLockedCrew = UnitManager.Instance._enemies.Count(e => 
                e != null && e.gameObject.activeInHierarchy && e.currentHealth > 0 && e.assignedModule == module && lockedEnemies.Contains(e));
            int neededCrew = module.RequiredCrew - currentLockedCrew;

            for (int i = 0; i < neededCrew; i++)
            {
                BaseEnemy closestFreeEnemy = FindClosestEnemy(module, lockedEnemies);

                if (closestFreeEnemy != null)
                {
                    closestFreeEnemy.assignedModule = module;
                    lockedEnemies.Add(closestFreeEnemy);
                }
                else
                    break;
            }
        }
    }

    private static int GetModulePriority(ShipModuleTile module, EnemyStrategy strategy)
    {
        if (strategy == EnemyStrategy.Shooting)
        {
            if (module is CannonTile) return 1;
            if (module is MastTile) return 2;
        }
        else if (strategy == EnemyStrategy.Meele)
        {
            if (module is HelmTile) return 1;
            if (module is MastTile) return 2;
        }

        return 4; 
    }

    private static BaseEnemy FindClosestEnemy(Tile startTile, HashSet<BaseEnemy> lockedEnemies)
    {
        Queue<Tile> queue = new Queue<Tile>();
        HashSet<Tile> visited = new HashSet<Tile>();
        
        queue.Enqueue(startTile);
        visited.Add(startTile);
        
        while (queue.Count > 0)
        {
            Tile current = queue.Dequeue();
            
            if (current != startTile && current.OccupiedUnit != null && current.OccupiedUnit is BaseEnemy)
            {
                BaseEnemy enemy = (BaseEnemy)current.OccupiedUnit;
                
                if (enemy.assignedModule == null && !lockedEnemies.Contains(enemy))
                {
                    return enemy;
                }
            }
            
            List<Tile> neighbors = GridManager.Instance.GetNeighbors(current);

            foreach (Tile neighbor in neighbors)
            {
                if (neighbor != null && !visited.Contains(neighbor))
                {
                    if (neighbor.name.Contains("Sea")) continue;

                    visited.Add(neighbor);
                    queue.Enqueue(neighbor);
                }
            }
        }

        return null;
    }
}