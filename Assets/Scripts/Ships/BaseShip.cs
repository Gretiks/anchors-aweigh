using UnityEngine;

public abstract class BaseShip : MonoBehaviour
{
    [SerializeField] public int hitPoints;
    //[SerializeField] protected string shipType;
    // [SerializeField] protected int maxCrew;
    
    public abstract void TakeDamage(int damage);
    protected abstract void DestroyShip();
}
