using System.Reflection;
using Assets.Scripts.Grid.Tiles.Modules;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Grid;

public class BaseEnemy : BaseUnit
{
    public ShipModuleTile assignedModule;
    public bool isActionCompleted { get; private set; } = true;

    // Prędkość przeskakiwania między pojedynczymi kafelkami ścieżki
    [SerializeField] private float tileStepDelay = 0.4f;

    public void SetTargetModule(ShipModuleTile module)
    {
        assignedModule = module;
    }

    public void StartTurnAction()
    {
        isActionCompleted = false;
        StartCoroutine(MoveAndActRoutine());
    }

    private IEnumerator MoveAndActRoutine()
    {
        // Krótkie opóźnienie przed rozpoczęciem akcji przez tę jednostkę
        yield return new WaitForSeconds(0.2f);

        if (assignedModule != null)
        {
            var neighbors = GridManager.Instance.GetNeighbors(assignedModule);

            // 1. Sprawdzamy, czy stoimy już obok przydzielonego modułu
            if (OccupiedTile != null && neighbors.Contains(OccupiedTile))
            {
                Debug.Log($"{unitName} stoi już na stanowisku przy module: {assignedModule.GetType().Name}");
            }
            else
            {
                // 2. Znajdujemy wolne kafelki wokół modułu docelowego
                var walkableTargets = neighbors.Where(t => t.Walkable && t.OccupiedUnit == null).ToList();
                List<Tile> bestPath = null;

                // 3. Szukamy najkrótszej poprawnej drogi do któregokolwiek z wolnych stanowisk przy module
                foreach (var targetTile in walkableTargets)
                {
                    var path = FindPathToTileBFS(targetTile);
                    if (path != null && (bestPath == null || path.Count < bestPath.Count))
                    {
                        bestPath = path;
                    }
                }

                // 4. Jeśli znaleźliśmy drogę, ruszamy w trasę z uwzględnieniem ograniczenia zasięgu
                if (bestPath != null && bestPath.Count > 0)
                {
                    // Ustalamy ile kratek realnie możemy przejść w tej turze (limit ruchu)
                    int stepsToTake = Mathf.Min(bestPath.Count, UnitMovement);
                    
                    Debug.Log($"{unitName} planuje podróż do {assignedModule.GetType().Name}. Droga: {bestPath.Count} kratek. Wykona: {stepsToTake} kroków (Zasięg: {UnitMovement})");

                    // Animacja ruchu: Przechodzimy kafelek po kafelku
                    for (int i = 0; i < stepsToTake; i++)
                    {
                        Tile nextTile = bestPath[i];
                        
                        // Używamy natywnej metody ustawiania jednostki z Tile.cs
                        nextTile.SetUnit(this);
                        
                        // Odejmujemy 1 punkt ruchu za każdą pokonaną kratkę (zgodnie z logikę Manhattan)
                        UnitMovement -= 1;

                        // Odczekujemy chwilę przed kolejnym krokiem, aby ruch był płynny dla oka
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

        yield return new WaitForSeconds(0.2f);
        isActionCompleted = true;
    }

    // Algorytm BFS generujący pełną listę kafelków (ścieżkę) od pozycji wroga do celu
    private List<Tile> FindPathToTileBFS(Tile targetTile)
    {
        if (OccupiedTile == null || targetTile == null) return null;

        Queue<Tile> queue = new Queue<Tile>();
        HashSet<Tile> visited = new HashSet<Tile>();
        
        // Słownik do rekonstrukcji ścieżki: klucz = kafelek, wartość = skąd na niego przyszliśmy
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

                    // Do budowania ścieżki ruchu dopuszczamy tylko kafelki Walkable (czyli wolne i przejezdne)
                    // Wyjątek robimy dla kafelka docelowego (gdyby zawierał jakieś specyficzne parametry)
                    if (neighbor.Walkable || neighbor == targetTile)
                    {
                        visited.Add(neighbor);
                        cameFrom[neighbor] = current;
                        queue.Enqueue(neighbor);
                    }
                }
            }
        }

        // Odtwarzanie ścieżki od tyłu (od celu do startu)
        if (pathFound)
        {
            List<Tile> path = new List<Tile>();
            Tile current = targetTile;

            while (current != OccupiedTile)
            {
                path.Add(current);
                current = cameFrom[current];
            }

            path.Reverse(); // Odwracamy listę, aby szła od Startu do Celu
            return path;
        }

        return null; // Brak wolnej ścieżki
    }

    public static void AssignEnemiesToModules()
    {
        var allModules = FindObjectsByType<ShipModuleTile>().Where(m => m.owner == Faction.Enemy).ToList();

        EnemyShip enemyShip = FindFirstObjectByType<EnemyShip>();
        EnemyStrategy currentStrategy = EnemyStrategy.Shooting; //domyslna wartosc awaryjna

        if (enemyShip != null)
            currentStrategy = enemyShip.ShipStrategy;
        else
            Debug.LogWarning("Nie znaleziony EnemyShip, wartosc domyslna");
        
        var sortedModules = allModules.OrderBy(m => GetModulePriority(m, currentStrategy)).ToList();

        HashSet<BaseEnemy> lockedEnemies = new HashSet<BaseEnemy>();

        foreach (var enemy in UnitManager.Instance._enemies)
        {
            if (enemy == null)
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
            int currentLockedCrew = UnitManager.Instance._enemies.Count(e => e != null && e.assignedModule == module && lockedEnemies.Contains(e));
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
        //lower value higher priority
        
        if (strategy == EnemyStrategy.Shooting)
        {
            // shooting: armaty > maszt
            if (module is CannonTile) return 1;
            if (module is MastTile) return 2;
            // if (module is HelmTile) return 3; // Ster na końcu
        }
        else if (strategy == EnemyStrategy.Meele)
        {
            // meele: ster > maszt
            if (module is HelmTile) return 1;
            if (module is MastTile) return 2;
            // if (module is CannonTile) return 3; // Armaty na końcu
        }

        return 4; // Dla pozostałych, nieokreślonych modułów
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