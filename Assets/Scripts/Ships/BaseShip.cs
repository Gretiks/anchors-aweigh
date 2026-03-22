using UnityEngine;

public abstract class BaseShip : MonoBehaviour
{
    [SerializerField] protected int hitPoints;
    [SerializerField] protected string shipType;
    [SerializerField] protected int maxCrew;

    public virtual void TakeDamage(int damage Amount)
    {
        hitPoints -= damage;
        if (hitPoints <= 0)
            DestroyShip()
    }
    
    protected abstract void DestroyShip();
}
