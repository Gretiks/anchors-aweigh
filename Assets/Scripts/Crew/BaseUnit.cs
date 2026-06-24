using UnityEngine;

public class BaseUnit : MonoBehaviour
{
    [Header("Identyfikacja Zapisu")]
    [Tooltip("Wpisz unikalne ID (np. 'Warrior_1'). Dla wrogów zostaw puste.")]
    public string uniqueID;

    public string prefabName;
    public string unitName;
    
    [Header("Statystyki")]
    public float maxHealth = 100f;
    public float currentHealth;
    public int UnitMovement = 5;
    public int baseMovement = 5;

    public bool hasAttacked;

    public Tile OccupiedTile;
    public Faction Faction;

    protected virtual void Awake()
    {
        currentHealth = maxHealth;
    }
    
    protected virtual void Start()
    {
        if(PlayerDataManager.Instance != null && PlayerDataManager.Instance.TryLoadUnitState(this))
            if (currentHealth <= 0)
                gameObject.SetActive(false);
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        if (currentHealth <= 0)
            Die();
    }

    protected virtual void Die()
    {
        if (OccupiedTile != null)
        {
            OccupiedTile.SetUnit(null);
            OccupiedTile = null;
        }
        
        gameObject.SetActive(false);
        
        if(Core.GameManager.Instance != null)
            Core.GameManager.Instance.CheckBattleConditions();
    }
    
    public void ResetMovement()
    {
        UnitMovement = baseMovement;
        hasAttacked = false;
    }
    
    
}
