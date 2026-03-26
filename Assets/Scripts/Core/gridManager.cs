using UnityEngine;

public class gridManager : MonoBehaviour
{
    [SerializeField] private int _width, _height;
    
    [SerializeField] private Tile _tilePrefab;
    
    void GenerateGrid()
    {
        for (int i = 0; i < _width; i++)
        {
            for (int j = 0; j < _height; j++)
            {
                
            }
        }
    }
    
}