using UnityEngine;

public abstract class BaseShip : MonoBehaviour
{
    [SerializeField] public float maxHealth = 100f;

    public float currentHealth;

    public string ShipName;
    //[SerializeField] protected string shipType;
    // [SerializeField] protected int maxCrew;

    [SerializeField] public float baseEvasion = 0.3f;
    [SerializeField] public float startPosition;
    [SerializeField] public float speed = 1f;
    
    private MastTile _mast;
    public float Position { get; private set; }


    public float evasion => baseEvasion + (_mast != null ? _mast.EvasionBonus : 0f);

    public void SetMast(MastTile mast) { _mast = mast; }

    protected void Awake()
    {
        currentHealth = maxHealth;
        Position = startPosition;
    }

    public void FindAndSetMast()
    {
        var allMasts = FindObjectsByType<MastTile>();
        foreach (var mast in allMasts)
        {
            if (mast.owner == GetFaction())
            {
                _mast = mast;
                break;
            }
        }
    }
    public void MoveShip(int direction)
    {
        Position += direction * speed;
        UpdateShipUI();
    }

    protected abstract Faction GetFaction();

    public void TakeDamange(float amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        UpdateShipUI();
    }

    public void UpdateShipUI()
    {
        MenuManager.Instance.ShowShipStats(ShipName, currentHealth, maxHealth, evasion);
        MenuManager.Instance.UpdatePositionBar();
    }
}
