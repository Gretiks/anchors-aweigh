using Assets.Scripts.Grid.Tiles.Modules;
using Core;
using UnityEngine;

public class MastTile : ShipModuleTile
{
    [SerializeField] public float evasionPerCrew = 0.1f;
    [SerializeField] public string mastDescription = "Main mast";

    public float EvasionBonus => CurrentCrew * evasionPerCrew;

    protected override void OnMouseDown()
    {
        if (GameManager.Instance.GameState != GameState.UserTurn) return;
        if (owner != Faction.User) return;
        MenuManager.Instance.ShowMastMenu(this);
    }
}
