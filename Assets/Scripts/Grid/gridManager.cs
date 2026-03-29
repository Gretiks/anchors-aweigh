using UnityEngine;

public class gridManager : MonoBehaviour
{
    [SerializeField] private int _width, _height;

    [SerializeField] private Tile _shipTile, _seaTile;
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
                var spawnedTile = Instantiate(isShip ? _shipTile : _seaTile, new Vector3(x, y), Quaternion.identity);
                spawnedTile.name = $"Tile {x} {y}";
                var isOffset = (x % 2 == 0 && y % 2 != 0) || (x % 2 != 0 && y % 2 == 0);
                spawnedTile.Innit(isOffset);
            }
        }
        _camera.transform.position = new Vector3((float)_width / 2 - 0.5f, (float)_height / 2 - 0.5f, -10);
    }

    bool IsShipTile(int x, int y)
    {
        float centerX = Mathf.Round((_width - 1) / 4f);
        float centerY = (_height - 1) / 2f;

        // Fixed ship dimensions in tiles
        int hullHalfWidth = 4;
        int sternHalfWidth = 3;
        int bowHeight = 4;
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