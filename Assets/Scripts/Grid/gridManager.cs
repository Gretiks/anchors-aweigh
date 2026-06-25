using System;
using UnityEngine;
using Core;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace Grid
{
    public class GridManager : MonoBehaviour
    {
        public static GridManager Instance;

        [SerializeField] private int _width, _height;
        [SerializeField] private Tile _shipTile, _seaTile, _enemyShipTile;
        [SerializeField] private Tile _cannonPlayerTile, _cannonEnemyTile;
        [SerializeField] private Tile _mastPlayerTile, _mastEnemyTile;
        [SerializeField] private Tile _helmPlayerTile, _helmEnemyTile;
        [SerializeField] private Tile _bridgeTile;

        private Dictionary<Vector2, Tile> _tiles;

        void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            // if (SceneManager.GetActiveScene().name == "BattleScene")
            // {
            //     ClearOldGrid();
            //     GameManager.Instance.ChangeState(GameState.GenerateGrid);
            // }
            // else if (SceneManager.GetActiveScene().name == "BoardingScene")
            // {
            //     ClearOldGrid();
            //     GenerateBoardingGrid();
            // }
            
                        
        }   

        public void GenerateGrid()
        {
            ClearOldGrid();
            
            _tiles = new Dictionary<Vector2, Tile>();

            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    Tile prefab;
                    if (IsHelmTile(x, y, out bool isHelmEnemy))
                        prefab = isHelmEnemy ? _helmEnemyTile : _helmPlayerTile;
                    else if (IsMastTile(x, y, out bool isMastEnemy))
                        prefab = isMastEnemy ? _mastEnemyTile : _mastPlayerTile;
                    else if (IsCannonTile(x, y, out bool isEnemy))
                        prefab = isEnemy ? _cannonEnemyTile : _cannonPlayerTile;
                    else if (IsShipTile(x, y)) prefab = _shipTile;
                    else if (IsEnemyShipTile(x, y)) prefab = _enemyShipTile;
                    else prefab = _seaTile;

                    var spawnedTile = Instantiate(prefab, new Vector3(x, y), Quaternion.identity);
                    
                    string tilePrefix = (prefab == _seaTile) ? "SeaTile" : "ShipTile";
                    spawnedTile.name = $"{tilePrefix} {x} {y}";
      
                    _tiles[new Vector2(x, y)] = spawnedTile;

                    var isOffset = (x % 2 == 0 && y % 2 != 0) || (x % 2 != 0 && y % 2 == 0);
                    spawnedTile.Init(isOffset);
                }
            }

            GameManager.Instance.ChangeState(GameState.SpawnUserCrew);
        }

        public void GenerateBoardingGrid()
        {
            ClearOldGrid();
            _tiles = new Dictionary<Vector2, Tile>();

            int bridge1Y = 5;
            int bridge11Y = 4;
            int bridge2Y = 10;
            int bridge22Y = 9;
            int bridgeStartX = 13;
            int bridgeEndX = 18;

            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    Tile prefab;

                    bool isBridge = x >= bridgeStartX && x <= bridgeEndX
                                    && (y == bridge1Y || y == bridge2Y || y == bridge11Y || y == bridge22Y);

                    if (isBridge)
                        prefab = _shipTile;
                    else if (IsHelmTile(x, y, out bool isHelmEnemy, boarding: true))
                        prefab = isHelmEnemy ? _helmEnemyTile : _helmPlayerTile;
                    else if (IsMastTile(x, y, out bool isMastEnemy, boarding: true))
                        prefab = isMastEnemy ? _mastEnemyTile : _mastPlayerTile;
                    else if (IsCannonTile(x, y, out bool isEnemy, boarding: true))
                        prefab = isEnemy ? _cannonEnemyTile : _cannonPlayerTile;
                    else if (IsShipTile(x, y, centerX: 9f)) prefab = _shipTile;
                    else if (IsEnemyShipTile(x, y, centerX: 22f)) prefab = _enemyShipTile;
                    else prefab = _seaTile;

                    var spawnedTile = Instantiate(prefab, new Vector3(x, y), Quaternion.identity);
                    spawnedTile.name = $"BoardingTile {x} {y}";
                    _tiles[new Vector2(x, y)] = spawnedTile;
                    var isOffset = (x % 2 == 0 && y % 2 != 0) || (x % 2 != 0 && y % 2 == 0);
                    spawnedTile.Init(isOffset);
                }
            }

            GameManager.Instance.ChangeState(GameState.SpawnUserCrew);
        }
        
        public void GenerateBossGrid()
        {
            ClearOldGrid();
            _tiles = new Dictionary<Vector2, Tile>();

            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    Tile prefab;

                    if (IsBossHelmTile(x, y, out bool isHelmEnemy))
                        prefab = isHelmEnemy ? _helmEnemyTile : _helmPlayerTile;
                    else if (IsBossMastTile(x, y, out bool isMastEnemy))
                        prefab = isMastEnemy ? _mastEnemyTile : _mastPlayerTile;
                    else if (IsBossCannonTile(x, y, out bool isEnemy))
                        prefab = isEnemy ? _cannonEnemyTile : _cannonPlayerTile;
                    else if (IsShipTile(x, y)) prefab = _shipTile;
                    else if (IsBossShipTile(x, y)) prefab = _enemyShipTile;
                    else prefab = _seaTile;

                    var spawnedTile = Instantiate(prefab, new Vector3(x, y), Quaternion.identity);
                    
                    string tilePrefix = (prefab == _seaTile) ? "SeaTile" : "ShipTile";
                    spawnedTile.name = $"{tilePrefix} {x} {y}";
                    
                    _tiles[new Vector2(x, y)] = spawnedTile;
                    var isOffset = (x % 2 == 0 && y % 2 != 0) || (x % 2 != 0 && y % 2 == 0);
                    spawnedTile.Init(isOffset);
                }
            }

            GameManager.Instance.ChangeState(GameState.SpawnUserCrew);
        }

        public void GenerateBossBoardingGrid()
        {
            ClearOldGrid();
            _tiles = new Dictionary<Vector2, Tile>();
        
            int bridge1Y = 5;
            int bridge11Y = 4;
            int bridge2Y = 10;
            int bridge22Y = 9;
            int bridgeStartX = 13;
            int bridgeEndX = 18;
        
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    Tile prefab;
        
                    bool isBridge = x >= bridgeStartX && x <= bridgeEndX
                                    && (y == bridge1Y || y == bridge2Y || y == bridge11Y || y == bridge22Y);
        
                    if (isBridge)
                        prefab = _shipTile;
                    else if (IsBossBoardingHelmTile(x, y, out bool isHelmEnemy))
                        prefab = isHelmEnemy ? _helmEnemyTile : _helmPlayerTile;
                    else if (IsBossBoardingMastTile(x, y, out bool isMastEnemy))
                        prefab = isMastEnemy ? _mastEnemyTile : _mastPlayerTile;
                    else if (IsBossBoardingCannonTile(x, y, out bool isEnemy))
                        prefab = isEnemy ? _cannonEnemyTile : _cannonPlayerTile;
                    else if (IsShipTile(x, y, centerX: 9f)) prefab = _shipTile;
                    else if (IsBossBoardingShipTile(x, y)) prefab = _enemyShipTile;
                    else prefab = _seaTile;
        
                    var spawnedTile = Instantiate(prefab, new Vector3(x, y), Quaternion.identity);
                    spawnedTile.name = $"BossBoardingTile {x} {y}";
                    _tiles[new Vector2(x, y)] = spawnedTile;
                    var isOffset = (x % 2 == 0 && y % 2 != 0) || (x % 2 != 0 && y % 2 == 0);
                    spawnedTile.Init(isOffset);
                }
            }
        
            GameManager.Instance.ChangeState(GameState.SpawnUserCrew);
        }
        
        private bool IsBossBoardingCannonTile(int x, int y, out bool isEnemy)
        {
            if (x == 12 && y == 7) { isEnemy = false; return true; } // gracz
            if (x == 19 && y == 6) { isEnemy = true;  return true; } // boss armata 1
            if (x == 19 && y == 9) { isEnemy = true;  return true; } // boss armata 2
            isEnemy = false;
            return false;
        }
        
        private bool IsBossBoardingMastTile(int x, int y, out bool isEnemy)
        {
            if (x == 9  && y == 7) { isEnemy = false; return true; }
            if (x == 22 && y == 7) { isEnemy = true;  return true; }
            isEnemy = false;
            return false;
        }
        
        private bool IsBossBoardingHelmTile(int x, int y, out bool isEnemy)
        {
            if (x == 9  && y == 4) { isEnemy = false; return true; }
            if (x == 22 && y == 4) { isEnemy = true;  return true; }
            isEnemy = false;
            return false;
        }
        
        bool IsBossBoardingShipTile(int x, int y)
        {
            float centerX = 22f;
            float centerY = (_height - 1) / 2f;
            int hullHalfWidth = 3;
            int sternHalfWidth = 2;
            int bowHeight = 3;
            int hullHeight = 11;
            int sternHeight = 1;
            int shipTotalHeight = bowHeight + hullHeight + sternHeight;
            int shipStartY = Mathf.RoundToInt(centerY - shipTotalHeight / 2f);
            int bowStartY = shipStartY + sternHeight + hullHeight;
        
            if (y < shipStartY || y >= shipStartY + shipTotalHeight) return false;
        
            if (y >= bowStartY)
            {
                float t = (float)(y - bowStartY) / bowHeight;
                float halfWidth = Mathf.Lerp(hullHalfWidth, 0f, t);
                return Mathf.Abs(x - centerX) < halfWidth + 0.5f;
            }
            else if (y >= shipStartY + sternHeight)
                return Mathf.Abs(x - centerX) <= hullHalfWidth;
            else
                return Mathf.Abs(x - centerX) <= sternHalfWidth;
        }
        
        // =========================================================================
        // ZMODYFIKOWANE METODY SPAWNUJĄCE (ZALEŻNE OD SCENY)
        // =========================================================================

        public Tile GetHeroSpawnTile()
        {
            bool isBoarding = SceneManager.GetActiveScene().name == "BoardingScene";
            float currentCenterX = isBoarding ? 9f : 5f; 

            var availableTiles = _tiles.AsEnumerable();

            if (isBoarding)
            {
                availableTiles = availableTiles.Where(t => (int)t.Key.x == 11 || (int)t.Key.x == 12);
            }

            return availableTiles
                .Where(t => IsShipTile((int)t.Key.x, (int)t.Key.y, currentCenterX) 
                            && t.Value.Walkable 
                            && t.Value.OccupiedUnit == null) // <-- BEZPIECZNIK GRACZA
                .OrderBy(_ => Random.value)
                .First().Value;
        }

        public Tile GetEnemySpawnTile()
        {
            string sceneName = SceneManager.GetActiveScene().name;
            bool isBoarding = sceneName == "BoardingScene";
            bool isBoss = !isBoarding && PlayerDataManager.Instance != null && PlayerDataManager.Instance.IsNextBattleBoss();

            float currentCenterX = isBoarding ? 22f : 26f; 

            var availableTiles = _tiles.AsEnumerable();

            if (isBoarding)
            {
                availableTiles = availableTiles.Where(t => (int)t.Key.x == 19 || (int)t.Key.x == 20);
            }

            return availableTiles
                .Where(t => {
                    int x = (int)t.Key.x;
                    int y = (int)t.Key.y;

                    // KLUCZOWY BEZPIECZNIK: Kafelek musi być przejezdny I fizycznie pusty!
                    bool isFree = t.Value.Walkable && t.Value.OccupiedUnit == null;

                    if (isBoss)
                    {
                        return IsBossShipTile(x, y) && isFree;
                    }
                    else
                    {
                        return IsEnemyShipTile(x, y, currentCenterX) && isFree;
                    }
                })
                .OrderBy(_ => Random.value)
                .First().Value;
        }

        // =========================================================================

        public List<Tile> GetNeighbors(Tile tile)
        {
            var neighbors = new List<Tile>();
            var pos = tile.transform.position;

            Vector2[] directions = {Vector2.up, Vector2.down, Vector2.left, Vector2.right};

            foreach (var dir in directions)
            {
                var key = new Vector2(pos.x + dir.x, pos.y + dir.y);
                if (_tiles.TryGetValue(key, out var neighbor))
                    neighbors.Add(neighbor);
            }

            return neighbors;
        }

        private bool IsCannonTile(int x, int y, out bool isEnemy, bool boarding = false)
        {
            if (!boarding)
            {
                if (x == 8 && y == 7) { isEnemy = false; return true; }
                if (x == 23 && y == 7) { isEnemy = true; return true; }
            }
            else
            {
                if (x == 12 && y == 7) { isEnemy = false; return true; }
                if (x == 19 && y == 7) { isEnemy = true; return true; }
            }
            isEnemy = false;
            return false;
        }

        private bool IsMastTile(int x, int y, out bool isEnemy, bool boarding = false)
        {
            if (!boarding)
            {
                if (x == 5 && y == 7) { isEnemy = false; return true; }
                if (x == 26 && y == 7) { isEnemy = true; return true; }
            }
            else
            {
                if (x == 9 && y == 7) { isEnemy = false; return true; }
                if (x == 22 && y == 7) { isEnemy = true; return true; }
            }
            isEnemy = false;
            return false;
        }

        private bool IsHelmTile(int x, int y, out bool isEnemy, bool boarding = false)
        {
            if (!boarding)
            {
                if (x == 5 && y == 4) { isEnemy = false; return true; }
                if (x == 26 && y == 4) { isEnemy = true; return true; }
            }
            else
            {
                if (x == 9 && y == 4) { isEnemy = false; return true; }
                if (x == 22 && y == 4) { isEnemy = true; return true; }
            }
            isEnemy = false;
            return false;
        }

        bool IsShipTile(int x, int y, float centerX = 5f)
        {
            float centerY = (_height - 1) / 2f;
    
            int hullHalfWidth = 3;
            int sternHalfWidth = 2;
            int bowHeight = 3;
            int hullHeight = 8;
            int sternHeight = 1;
    
            int shipTotalHeight = bowHeight + hullHeight + sternHeight;
    
            int shipStartY = Mathf.RoundToInt(centerY - shipTotalHeight / 2f);
            int bowStartY = shipStartY + sternHeight + hullHeight;
    
            if (y < shipStartY || y >= shipStartY + shipTotalHeight) return false;
    
            if (y >= bowStartY)
            {
                float t = (float)(y - bowStartY) / bowHeight;
                float halfWidth = Mathf.Lerp(hullHalfWidth, 0f, t);
                return Mathf.Abs(x - centerX) < halfWidth + 0.5f;
            }
            else if (y >= shipStartY + sternHeight)
            {
                return Mathf.Abs(x - centerX) <= hullHalfWidth;
            }
            else
            {
                return Mathf.Abs(x - centerX) <= sternHalfWidth;
            }
        }

        bool IsEnemyShipTile(int x, int y, float centerX = 26f)
        {
            float centerY = (_height - 1) / 2f;
    
            int hullHalfWidth = 3;
            int sternHalfWidth = 2;
            int bowHeight = 3;
            int hullHeight = 8;
            int sternHeight = 1;
    
            int shipTotalHeight = bowHeight + hullHeight + sternHeight;
    
            int shipStartY = Mathf.RoundToInt(centerY - shipTotalHeight / 2f);
            int bowStartY = shipStartY + sternHeight + hullHeight;
    
            if (y < shipStartY || y >= shipStartY + shipTotalHeight) return false;
    
            if (y >= bowStartY)
            {
                float t = (float)(y - bowStartY) / bowHeight;
                float halfWidth = Mathf.Lerp(hullHalfWidth, 0f, t);
                return Mathf.Abs(x - centerX) < halfWidth + 0.5f;
            }
            else if (y >= shipStartY + sternHeight)
            {
                return Mathf.Abs(x - centerX) <= hullHalfWidth;
            }
            else
            {
                return Mathf.Abs(x - centerX) <= sternHalfWidth;
            }
        }
        
        private bool IsBossCannonTile(int x, int y, out bool isEnemy)
        {
            if (x == 8 && y == 7)  { isEnemy = false; return true; } // armata gracza bez zmian
            if (x == 23 && y == 6) { isEnemy = true;  return true; } // armata bossa 1
            if (x == 23 && y == 9 ) { isEnemy = true;  return true; } // armata bossa 2
            isEnemy = false;
            return false;
        }

        private bool IsBossMastTile(int x, int y, out bool isEnemy)
        {
            if (x == 5  && y == 7) { isEnemy = false; return true; }
            if (x == 26 && y == 7) { isEnemy = true;  return true; }
            isEnemy = false;
            return false;
        }

        private bool IsBossHelmTile(int x, int y, out bool isEnemy)
        {
            if (x == 5  && y == 4) { isEnemy = false; return true; }
            if (x == 26 && y == 4) { isEnemy = true;  return true; }
            isEnemy = false;
            return false;
        }

        bool IsBossShipTile(int x, int y)
        {
            float centerX = 26f;
            float centerY = (_height - 1) / 2f;

            int hullHalfWidth = 3;
            int sternHalfWidth = 2;
            int bowHeight = 3;
            int hullHeight = 11; // dłuższy kadłub
            int sternHeight = 1;

            int shipTotalHeight = bowHeight + hullHeight + sternHeight;
            int shipStartY = Mathf.RoundToInt(centerY - shipTotalHeight / 2f);
            int bowStartY = shipStartY + sternHeight + hullHeight;

            if (y < shipStartY || y >= shipStartY + shipTotalHeight) return false;

            if (y >= bowStartY)
            {
                float t = (float)(y - bowStartY) / bowHeight;
                float halfWidth = Mathf.Lerp(hullHalfWidth, 0f, t);
                return Mathf.Abs(x - centerX) < halfWidth + 0.5f;
            }
            else if (y >= shipStartY + sternHeight)
                return Mathf.Abs(x - centerX) <= hullHalfWidth;
            else
                return Mathf.Abs(x - centerX) <= sternHalfWidth;
        }

        public void ShowRangeHighlights(BaseUnit unit)
        {
            ClearRangeHighlights();

            if (unit == null || unit.OccupiedTile == null || unit.UnitMovement <= 0)
                return;
            
            HashSet<Tile> reachableTiles = GetReachableTiles(unit.OccupiedTile, unit.UnitMovement);

            foreach (var tile in reachableTiles)
                tile.SetRangeHighlight(true);
        }

        public void ClearRangeHighlights()
        {
            foreach (var tile in _tiles.Values)
                tile.SetRangeHighlight(false);
        }

        private bool IsWithinRange(Tile from, Tile to, int range)
        {
            var a = from.transform.position;
            var b = to.transform.position;
            int dist = Mathf.RoundToInt(Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y));
            return dist <= range && dist > 0;
        }
        
        private void ClearOldGrid()
        {
            if (_tiles != null)
            {
                foreach (var tile in _tiles.Values)
                {
                    if (tile != null)
                    {
                        Destroy(tile.gameObject);
                    }
                }
                _tiles.Clear();
            }
            else
            {
                _tiles = new Dictionary<Vector2, Tile>();
            }
        }
        
        public List<Tile> FindPath(Tile startTile, Tile targetTile)
        {
            if (startTile == null || targetTile == null) return null;

            Queue<Tile> queue = new Queue<Tile>();
            HashSet<Tile> visited = new HashSet<Tile>();
            Dictionary<Tile, Tile> cameFrom = new Dictionary<Tile, Tile>();

            queue.Enqueue(startTile);
            visited.Add(startTile);

            bool pathFound = false;

            while (queue.Count > 0)
            {
                Tile current = queue.Dequeue();

                if (current == targetTile)
                {
                    pathFound = true;
                    break;
                }

                List<Tile> neighbors = GetNeighbors(current);

                foreach (Tile neighbor in neighbors)
                {
                    if (neighbor != null && !visited.Contains(neighbor))
                    {
                        // Ignorujemy kafelki morza
                        if (neighbor.name.Contains("Sea")) continue;

                        // Kafelek jest przejezdny jeśli: jest Walkable ORAZ stoi na nim ewentualny trup (HP <= 0)
                        bool isPassable = neighbor.Walkable || (neighbor.OccupiedUnit != null && neighbor.OccupiedUnit.currentHealth <= 0);

                        if (isPassable)
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

                while (current != startTile)
                {
                    path.Add(current);
                    current = cameFrom[current];
                }

                path.Reverse();
                return path;
            }

            return null; // Droga całkowicie zablokowana
        }
        
        public HashSet<Tile> GetReachableTiles(Tile startTile, int maxMovement)
        {
            var reachable = new HashSet<Tile>();
            var queue = new Queue<(Tile tile, int cost)>();
            var visitedCosts = new Dictionary<Tile, int>(); // Zapisujemy najtańszy koszt dotarcia do kafelka

            queue.Enqueue((startTile, 0));
            visitedCosts[startTile] = 0;

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();

                foreach (var neighbor in GetNeighbors(current.tile))
                {
                    // Ignorujemy kafelki puste oraz kafelki morza
                    if (neighbor == null || neighbor.name.Contains("Sea")) continue;

                    // Pole jest przejezdne TYLKO gdy jest Walkable LUB stoi na nim martwa jednostka
                    bool isPassable = neighbor.Walkable || (neighbor.OccupiedUnit != null && neighbor.OccupiedUnit.currentHealth <= 0);

                    if (!isPassable) continue;

                    int newCost = current.cost + 1; // Zakładamy, że krok na sąsiednie pole kosztuje 1

                    if (newCost <= maxMovement)
                    {
                        // Jeśli jeszcze tu nie byliśmy LUB nowa trasa do tego pola jest tańsza:
                        if (!visitedCosts.ContainsKey(neighbor) || newCost < visitedCosts[neighbor])
                        {
                            visitedCosts[neighbor] = newCost;
                            reachable.Add(neighbor);
                            queue.Enqueue((neighbor, newCost));
                        }
                    }
                }
            }

            return reachable;
        }
    }
}