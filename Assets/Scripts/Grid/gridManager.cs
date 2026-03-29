using UnityEngine;

public class gridManager : MonoBehaviour
{
    [SerializeField] private int _width, _height;

    [SerializeField] private Tile _shipTile, _seaTile, _enemyShipTile;
    [SerializeField] private Transform _camera;

    private void Start()
    {
        GenerateGrid();
    }

    void GenerateGrid()
    {
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                bool isShip = IsShipTile(x, y);
                Tile GetTile(int x, int y)
                {
                    if (IsShipTile(x, y)) return _shipTile;
                    if (IsEnemyShipTile(x, y)) return _enemyShipTile;
                    return _seaTile;
                }
                var spawnedTile = Instantiate(GetTile(x, y), new Vector3(x, y), Quaternion.identity);
                spawnedTile.name = $"Tile {x} {y}";
                var isOffset = (x % 2 == 0 && y % 2 != 0) || (x % 2 != 0 && y % 2 == 0);
                spawnedTile.Innit(isOffset);
            }
        }
        _camera.transform.position = new Vector3((float)_width / 2 - 0.5f, (float)_height / 2 - 0.5f, -10);
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