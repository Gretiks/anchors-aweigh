using Assets.Scripts.Grid.Tiles.Modules;
using Core;
using UnityEngine;

public class MastTile : ShipModuleTile
{
    [SerializeField] public float evasionPerCrew = 0.1f;
    [SerializeField] public string mastDescription = "Main mast";
    
    [SerializeField] public int requiredCrew = 4;
    public override int RequiredCrew => requiredCrew;

    public float EvasionBonus => CurrentCrew * evasionPerCrew;

    protected override void OnMouseDown()
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "BoardingScene") return;
        if (GameManager.Instance.GameState != GameState.UserTurn) return;
        if (owner != Faction.User) return;
        MenuManager.Instance.ShowMastMenu(this);
    }
}
