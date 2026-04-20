using UnityEngine;
using System.Collections.Generic;

namespace Core
{
    
    public class ShipManager : MonoBehaviour
    {
        public static ShipManager Instance;
        private List<BaseShip> _ships;
        
        public PlayerShip playerShip;
        public EnemyShip enemyShip;
        
        [SerializeField] private PlayerShip playerShipPrefab;
        [SerializeField] private EnemyShip enemyShipPrefab;
        
        
        
        void Awake()
        {
            Instance = this;
            
        }

        public void InitiatePlayerShip()
        {
            playerShip = Instantiate(playerShipPrefab);
            playerShip.ShipName = "Player";
        }

        public void InitiateEnemyShip()
        {
            enemyShip = Instantiate(enemyShipPrefab);
            enemyShip.ShipName = "Enemy";
        }
        
        
    }
}