using Core;
using Grid;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class Tile : MonoBehaviour
{
    public string tileName;
    
    [SerializeField] private Color _baseColor, _offsetColor;
    [SerializeField] private Sprite _baseSprite, _offsetSprite;
    [SerializeField] protected SpriteRenderer _spriteRenderer;
    [SerializeField] private GameObject _highlight;
    [SerializeField] private GameObject _rangeHighlight;
    [SerializeField] private bool _isWalkable;

    public BaseUnit OccupiedUnit;
    public bool Walkable => _isWalkable && OccupiedUnit == null;

    private AudioManager _audioManager;
    private AudioManager AudioManagerInstance
    {
        get
        {
            // Jeœli referencja jest pusta, wyszukaj j¹
            if (_audioManager == null)
            {
                GameObject audioObject = GameObject.FindGameObjectWithTag("Audio");
                if (audioObject != null)
                {
                    _audioManager = audioObject.GetComponent<AudioManager>();
                }
                else
                {
                    Debug.LogWarning("Nie znaleziono obiektu z tagiem 'Audio' w scenie.");
                }
            }
            return _audioManager;
        }
    }

    public void Init(bool isOffset)
    {
        if (_baseSprite != null)
        {
            _spriteRenderer.sprite = isOffset ? _offsetSprite : _baseSprite;
            _spriteRenderer.color = Color.white;
        }
        else
        {
            _spriteRenderer.color = isOffset ? _offsetColor : _baseColor;
        }
    }

    public void SetRangeHighlight(bool active)
    {
        _rangeHighlight.SetActive(active);
    }

    private void OnMouseEnter()
    {
        _highlight.SetActive(true);
    }

    private void OnMouseExit()
    {
        _highlight.SetActive(false);
    }

    protected virtual void OnMouseDown()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;
        MenuManager.Instance.HideCannonMenu();
        MenuManager.Instance.HideMastMenu();
        MenuManager.Instance.HideHelmMenu();
        
        if (GameManager.Instance.GameState != GameState.UserTurn) return;

        if (OccupiedUnit != null)
        {
            if(OccupiedUnit.Faction == Faction.User) 
                UnitManager.Instance.SetSelectedHero((BaseHero)OccupiedUnit);
            else
            {
                if (UnitManager.Instance.SelectedHero != null)
                {
                    var enemy = (BaseEnemy)OccupiedUnit;
                    //attack
                    if (SceneManager.GetActiveScene().name == "BoardingScene")
                    {
                        UnitManager.Instance.AttackEnemyWithSelectedHero(enemy);
                        UnitManager.Instance.SelectedHero.UnitMovement = 0;
                    }
                    
                    UnitManager.Instance.SetSelectedHero(null);
                    MenuManager.Instance.RefreshHeroList(UnitManager.Instance._heroes);
                    MenuManager.Instance.RefreshEnemyList(UnitManager.Instance._enemies);
                }
            }
        }
        else
        {
            //movement
            if (UnitManager.Instance.SelectedHero != null && Walkable)
            {
                var hero = UnitManager.Instance.SelectedHero;
                var path = GridManager.Instance.FindPath(hero.OccupiedTile, this);
                
                if (path != null && path.Count <= hero.UnitMovement)
                {
                    int realCost = path.Count;
                    hero.UnitMovement -= realCost;
                    
                    SetUnit(hero);
                    UnitManager.Instance.SetSelectedHero(null);
                    MenuManager.Instance.RefreshHeroList(UnitManager.Instance._heroes);
                    MenuManager.Instance.RefreshEnemyList(UnitManager.Instance._enemies);
                    if (AudioManagerInstance != null)
                    {
                        AudioManagerInstance.PlaySFX(AudioManagerInstance.pawn);
                    }
                }
            }
        }
    }
    
    public void SetUnit(BaseUnit unit)
    {
        OccupiedUnit = unit;

        if (unit != null)
        {
            if (unit.OccupiedTile != null)
                unit.OccupiedTile.OccupiedUnit = null;
            
            unit.transform.position = transform.position;
            unit.OccupiedTile = this;
        }
        
        if (ShipManager.Instance.playerShip != null) ShipManager.Instance.playerShip.UpdateShipUI();
        if (ShipManager.Instance.enemyShip != null) ShipManager.Instance.enemyShip.UpdateShipUI();
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
