using UnityEngine;
using System.Collections.Generic;

namespace Core
{
    
    public class ShipManager : MonoBehaviour
    {
        public static ShipManager Instance;
        private List<BaseShip> _ships;
        public BaseShip SelectedShip;
        
        
        
        void Awake()
        {
            Instance = this;
            
        }

        public void InitiatePlayerShip()
        {
            var playerShip = Instantiate(new PlayerShip());
            
            MenuManager.Instance.RefreshShipHealth(playerShip);
        }

        public void InitiateEnemyShip()
        {
            var enemyShip = Instantiate(new EnemyShip());
            
            MenuManager.Instance.RefreshShipHealth(enemyShip);
        }
        
        
    }
}