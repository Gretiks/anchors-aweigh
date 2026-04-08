using UnityEngine;
using Core;
using System.Collections.Generic;
using System.Linq;

namespace Grid
{

    public class GridManager : MonoBehaviour
    {
        public static GridManager Instance;

        [SerializeField] private int _width, _height;
        [SerializeField] private Tile _shipTile, _seaTile, _enemyShipTile;
        [SerializeField] private Transform _camera;

        private Dictionary<Vector2, Tile> _tiles;

        void Awake()
        {
            Instance = this;
        }

        public void GenerateGrid()
        {
            _tiles = new Dictionary<Vector2, Tile>();

            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    Tile prefab;
                    if (IsShipTile(x, y)) prefab = _shipTile;
                    else if (IsEnemyShipTile(x, y)) prefab = _enemyShipTile;
                    else prefab = _seaTile;

                    var spawnedTile = Instantiate(prefab, new Vector3(x, y), Quaternion.identity);
                    spawnedTile.name = $"Tile {x} {y}";
                    _tiles[new Vector2(x, y)] = spawnedTile;

                    var isOffset = (x % 2 == 0 && y % 2 != 0) || (x % 2 != 0 && y % 2 == 0);
                    spawnedTile.Init(isOffset);
                }
            }

            _camera.transform.position = new Vector3((float)_width / 2 - 0.5f, (float)_height / 2 - 0.5f, -10);
            GameManager.Instance.ChangeState(GameState.SpawnUserCrew);
        }

        public Tile GetHeroSpawnTile()
        {
            return _tiles
                .Where(t => IsShipTile((int)t.Key.x, (int)t.Key.y))
                .OrderBy(_ => Random.value)
                .First().Value;
        }

        public Tile GetEnemySpawnTile()
        {
            return _tiles
                .Where(t => IsEnemyShipTile((int)t.Key.x, (int)t.Key.y))
                .OrderBy(_ => Random.value)
                .First().Value;
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
    }
}
