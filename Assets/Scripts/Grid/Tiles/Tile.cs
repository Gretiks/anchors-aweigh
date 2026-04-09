using Core;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public string tileName;
    
    [SerializeField] private Color _baseColor, _offsetColor;
    [SerializeField] protected SpriteRenderer _spriteRenderer;
    [SerializeField] private GameObject _highlight;
    [SerializeField] private GameObject _rangeHighlight;
    [SerializeField] private bool _isWalkable;

    public BaseUnit OccupiedUnit;
    public bool Walkable => _isWalkable && OccupiedUnit == null;
    
    public void Init(bool isOffset) { 
        _spriteRenderer.color = isOffset ? _offsetColor : _baseColor;
    }

    public void SetRangeHighlight(bool active)
    {
        _rangeHighlight.SetActive(active);
    }

    private void OnMouseEnter()
    {
        _highlight.SetActive(true);
        MenuManager.Instance.ShowTileInfo(this);
    }

    private void OnMouseExit()
    {
        _highlight.SetActive(false);
        MenuManager.Instance.ShowTileInfo(null);
    }

    private void OnMouseDown()
    {
        if (GameManager.Instance.GameState != GameState.UserTurn) return;

        if (OccupiedUnit != null)
        {
            if(OccupiedUnit.Faction == Faction.User) UnitManager.Instance.SetSelectedHero((BaseHero)OccupiedUnit);
            else
            {
                if (UnitManager.Instance.SelectedHero != null)
                {
                    var enemy = (BaseEnemy)OccupiedUnit;
                    //attack
                    Destroy(enemy.gameObject);
                    UnitManager.Instance.SetSelectedHero(null);
                }
            }
        }
        else
        {
            //movement
            if (UnitManager.Instance.SelectedHero != null && Walkable)
            {
                var hero = UnitManager.Instance.SelectedHero;
                var dist =  CalculateDistance(hero);
                
                if (IsWithinMoveRange(hero, dist))
                {
                    hero.UnitMovement -= dist;
                    SetUnit(UnitManager.Instance.SelectedHero);
                    UnitManager.Instance.SetSelectedHero(null);
                    MenuManager.Instance.RefreshHeroList(UnitManager.Instance._heroes);
                }
            }
        }
    }
    
    public void SetUnit(BaseUnit unit)
    {
        if(unit.OccupiedTile != null) unit.OccupiedTile.OccupiedUnit = null;
        unit.transform.position = transform.position;
        OccupiedUnit = unit;
        unit.OccupiedTile = this;
    }

    private int CalculateDistance(BaseUnit unit)
    {

        var from = unit.OccupiedTile.transform.position;
        var to = transform.position;

        return Mathf.RoundToInt(Mathf.Abs(from.x - to.x) + Mathf.Abs(from.y - to.y));
    }
    
    private bool IsWithinMoveRange(BaseUnit unit, int dist)
    {
        if (unit.OccupiedTile == null) return true;

        // var from = unit.OccupiedTile.transform.position;
        // var to = transform.position;
        //
        // // Manhattan distance
        // int dist = Mathf.RoundToInt(Mathf.Abs(from.x - to.x) + Mathf.Abs(from.y - to.y));

        return dist <= unit.UnitMovement;
    }
}
