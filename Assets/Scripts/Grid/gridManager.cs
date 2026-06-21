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

            //_camera.transform.position = new Vector3((float)_width / 2 - 0.5f, (float)_height / 2 - 0.5f, -10);
            GameManager.Instance.ChangeState(GameState.SpawnUserCrew);
        }
        
        public void GenerateBoardingGrid()
        {
            // 1. Usuwamy stary grid z poprzedniej sceny
            ClearOldGrid();
        
            // Obliczamy wymiary oryginalnych statków (na podstawie standardowej szerokości _width)
            // Zakładamy domyślny podział oryginalnej mapy (np. lewa połowa to gracz, prawa to wróg)
            int originalHalfWidth = _width / 2; 
        
            // Nowe parametry po przybliżeniu statków
            int bridgeLength = 4; // Długość mostów wskazana w zadaniu
            
            // Nowa całkowita szerokość mapy abordażu: 
            // Szerokość statku gracza + szerokość mostów + szerokość statku wroga
            int boardingWidth = originalHalfWidth + bridgeLength + originalHalfWidth;
        
            // Wyznaczamy rzędy, w których powstaną mosty łączące statki (nie mogą być obok siebie)
            int bridge1Row = 2;
            int bridge2Row = _height - 3;
        
            for (int x = 0; x < boardingWidth; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    Tile prefab = null;
        
                    // --- STREFA STATKU GRACZA (Lewa strona) ---
                    if (x < originalHalfWidth)
                    {
                        // Sprawdzamy oryginalny kształt statku gracza dla tej pozycji (x, y)
                        if (IsHelmTile(x, y, out bool isHelmEnemy) && !isHelmEnemy) prefab = _helmPlayerTile;
                        else if (IsMastTile(x, y, out bool isMastEnemy) && !isMastEnemy) prefab = _mastPlayerTile;
                        else if (IsCannonTile(x, y, out bool isEnemy) && !isEnemy) prefab = _cannonPlayerTile;
                        else if (IsShipTile(x, y)) prefab = _shipTile;
                        else prefab = _seaTile;
                    }
                    // --- STREFA MOSTÓW I MORZA POMIĘDZY STATKAMI ---
                    else if (x >= originalHalfWidth && x < originalHalfWidth + bridgeLength)
                    {
                        // Jeśli trafimy na rząd wyznaczony dla mostu, tworzymy przejście
                        if (y == bridge1Row || y == bridge2Row)
                        {
                            prefab = _shipTile; // Most tworzony z kafelków po których można chodzić
                        }
                        else
                        {
                            prefab = _seaTile;
                        }
                    }
                    // --- STREFA STATKU WROGA (Prawa strona) ---
                    else
                    {
                        // Aby statek wroga zachował identyczny kształt, musimy "cofnąć" jego pozycję X 
                        // do miejsca, w którym znajdowałby się na oryginalnej szerokiej mapie.
                        int originalX = x - bridgeLength; 
        
                        if (IsHelmTile(originalX, y, out bool isHelmEnemy) && isHelmEnemy) prefab = _helmEnemyTile;
                        else if (IsMastTile(originalX, y, out bool isMastEnemy) && isMastEnemy) prefab = _mastEnemyTile;
                        else if (IsCannonTile(originalX, y, out bool isEnemy) && isEnemy) prefab = _cannonEnemyTile;
                        else if (IsEnemyShipTile(originalX, y)) prefab = _enemyShipTile;
                        else prefab = _seaTile;
                    }
        
                    // Tworzenie kafelka na scenie
                    var spawnedTile = Instantiate(prefab, new Vector3(x, y), Quaternion.identity);
                    spawnedTile.name = $"BoardingTile {x} {y}";
        
                    _tiles[new Vector2(x, y)] = spawnedTile;
        
                    // Inicjalizacja koloru szachownicy (offset)
                    var isOffset = (x % 2 == 0 && y % 2 != 0) || (x % 2 != 0 && y % 2 == 0);
                    spawnedTile.Init(isOffset);
                }
            }
        
            // Przekazanie stanu gry do rozstawienia załogi na nowym gridzie
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

        private bool IsHelmTile(int x, int y, out bool isEnemy)
        {
            if (x == 5 && y == 4) { isEnemy = false; return true; }
            if (x == 26 && y == 4) { isEnemy = true; return true; }

            isEnemy = false;
            return false;
        }

        private bool IsMastTile(int x, int y, out bool isEnemy)
        {
            if (x == 5 && y == 7) { isEnemy = false; return true; }
            if (x == 26 && y == 7) { isEnemy = true; return true; }

            isEnemy = false;
            return false;
        }

        private bool IsCannonTile(int x, int y, out bool isEnemy)
        {
            if (x == 8 && y == 7) { isEnemy = false; return true; }
            if (x == 23 && y == 7) { isEnemy = true; return true; }

            isEnemy = false;
            return false;
        }

        bool IsShipTile(int x, int y)
        {
            float centerX = 5f;
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
    
        bool IsEnemyShipTile(int x, int y)
        {
            float centerX = _width - 6f;
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
