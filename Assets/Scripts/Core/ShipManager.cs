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
        [SerializeField] private EnemyShip bossShipPrefab;
        
        
        
        void Awake()
        {
            Instance = this;
            
        }

        public void InitiatePlayerShip()
        {
            playerShip = Instantiate(playerShipPrefab);
            playerShip.ShipName = "Player";
            playerShip.FindAndSetMast();
            playerShip.UpdateShipUI();
        }

        public void InitiateEnemyShip()
        {
            if (PlayerDataManager.Instance != null && PlayerDataManager.Instance.IsNextBattleBoss())
            {
                enemyShip = Instantiate(bossShipPrefab);
            }
            else
            {
                enemyShip = Instantiate(enemyShipPrefab);
            }
            enemyShip.ShipName = "Enemy";
            enemyShip.FindAndSetMast();
            enemyShip.UpdateShipUI();
            MenuManager.Instance.UpdatePositionBar();
        }


    }
}