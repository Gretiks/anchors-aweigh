using UnityEngine;

public abstract class BaseShip : MonoBehaviour
{
    [SerializeField] public float maxHealth = 100f;

    public float currentHealth;

    public string ShipName;
    //[SerializeField] protected string shipType;
    // [SerializeField] protected int maxCrew;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamange(float amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        UpdateShipUI();
    }

    public void UpdateShipUI()
    {
        MenuManager.Instance.ShowShipStats(ShipName, currentHealth, maxHealth);
    }
}
