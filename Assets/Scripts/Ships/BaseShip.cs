using UnityEngine;

public abstract class BaseShip : MonoBehaviour
{
    [Header("Ustawienia zapisu")] [SerializeField]
    public bool isPlayerShip = false;
    
    public abstract float maxHealth { get; }
    public float currentHealth;
    public string ShipName;

    [SerializeField] public float baseEvasion = 0.3f;
    [SerializeField] public float startPosition;
    [SerializeField] public float speed = 1f;
    
    private MastTile _mast;
    public float Position { get; private set; }

    // =====================================================================
    // [ZMIANA 1]: Dodanie słowa 'virtual', co zezwala klasom pochodnym na override
    // =====================================================================
    public virtual float evasion => baseEvasion + (_mast != null ? _mast.EvasionBonus : 0f);

    // =====================================================================
    // [ZMIANA 2]: Wirtualna właściwość z dodatkową prędkością (domyślnie 0 dla AI)
    // =====================================================================
    public virtual float ExtraSpeed => 0f;

    public void SetMast(MastTile mast) { _mast = mast; }

    protected virtual void Awake()
    {
        currentHealth = maxHealth;
        Position = startPosition;
    }

    protected virtual void Start()
    {
        UpdateShipUI();
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
        // =================================================================
        // [ZMIANA 3]: Bezpieczne, czyste dodanie ExtraSpeed do prędkości statku
        // =================================================================
        Position += direction * (speed + ExtraSpeed);
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