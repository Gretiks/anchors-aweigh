using UnityEngine;

public class BaseUnit : MonoBehaviour
{
    public Tile OccupiedTile;
    public Faction Faction;
    public string unitName;
    public int UnitMovement = 5;
    public int baseMovement = 5;

    public void ResetMovement()
    {
        UnitMovement = baseMovement;
    }
    
    
}
