using Core;
using Grid;
using UnityEngine;

public class CannonTile : Tile
{
    [SerializeField] public Faction owner;
    [SerializeField] public int requiredCrew = 2;
    [SerializeField] public int damage = 20;
    [SerializeField] public string cannonDescription = "Standard cannon";

    public bool HasFired { get; private set; } = false;
    public int CurrentCrew => CountAdjacentCrew();

    private int CountAdjacentCrew()
    {
    var neighbors = GridManager.Instance.GetNeighbors(this);
    int count = 0;
    foreach (var tile in neighbors)
    {
        if (tile.OccupiedUnit != null && tile.OccupiedUnit.Faction == owner)
            count++;
        }
            return count;
        }

    protected override void OnMouseDown()
    {
        if (GameManager.Instance.GameState != GameState.UserTurn) return;
        if (owner != Faction.User) return;
        MenuManager.Instance.ShowCannonMenu(this);
    }

    public void Fire()
    {
        if (HasFired) return;
        if (owner == Faction.User)
            ShipManager.Instance.enemyShip.TakeDamange(damage);
        else
            ShipManager.Instance.playerShip.TakeDamange(damage);
        HasFired = true;
        MenuManager.Instance.HideCannonMenu();
    }
}
