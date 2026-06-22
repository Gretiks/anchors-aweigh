using System;
using UnityEngine;
using Core;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.XR;
using Random = UnityEngine.Random;

namespace Grid
{

    public class GridManager : MonoBehaviour
    {
        public static GridManager Instance;
        public static GameManager gameManager;

        [SerializeField] private int _width, _height;
        [SerializeField] private Tile _shipTile, _seaTile, _enemyShipTile;
        //[SerializeField] private Transform _camera;
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
            if (SceneManager.GetActiveScene().name == "BattleScene")
            {
                ClearOldGrid();
                gameManager.ChangeState(GameState.GenerateGrid);
            }
            else if (SceneManager.GetActiveScene().name == "BoardingScene")
            {
                ClearOldGrid();
                GenerateBoardingGrid();
            }


        }   

        public void GenerateGrid()
        {
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
                    spawnedTile.name = $"Tile {x} {y}";
      
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
            int bridge2Y = 10;
            int bridgeStartX = 13;
            int bridgeEndX = 18;

            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    Tile prefab;

                    bool isBridge = x >= bridgeStartX && x <= bridgeEndX
                                    && (y == bridge1Y || y == bridge2Y);

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


        public Tile GetHeroSpawnTile()
        {
            return _tiles
                .Where(t => IsShipTile((int)t.Key.x, (int)t.Key.y) && t.Value.Walkable)
                .OrderBy(_ => Random.value)
                .First().Value;
        }

        public Tile GetEnemySpawnTile()
        {
            return _tiles
                .Where(t => IsEnemyShipTile((int)t.Key.x, (int)t.Key.y) && t.Value.Walkable)
                .OrderBy(_ => Random.value)
                .First().Value;
        }

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

        public void ShowRangeHighlights(BaseUnit unit)
        {
            foreach (var tile in _tiles.Values)
            {
                bool inRange = unit.OccupiedTile != null
                    && IsWithinRange(unit.OccupiedTile, tile, unit.UnitMovement)
                    && tile.Walkable;
                tile.SetRangeHighlight(inRange);
            }
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
                        Destroy(tile.gameObject); // Fizyczne usunięcie obiektu ze sceny
                    }
                }
                _tiles.Clear(); // Wyczyszczenie słownika
            }
            else
            {
                _tiles = new Dictionary<Vector2, Tile>();
            }
        }

    }
}
