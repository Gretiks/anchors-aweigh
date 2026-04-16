using UnityEngine;

public abstract class BaseShip : MonoBehaviour
{
    [SerializeField] protected int hitPoints;
    //[SerializeField] protected string shipType;
    [SerializeField] protected int maxCrew;

    public virtual void TakeDamage(int damage)
    {
        hitPoints -= damage;
        if (hitPoints <= 0)
            DestroyShip();
    }
    
    protected abstract void DestroyShip();
}
