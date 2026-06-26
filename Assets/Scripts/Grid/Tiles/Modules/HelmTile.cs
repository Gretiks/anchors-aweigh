using Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Grid.Tiles.Modules
{
    public class HelmTile : ShipModuleTile
    {
        public enum HelmOrder { Approach, Stop, Flee }

        [SerializeField] public string helmDescription = "Ship's helm";
        public HelmOrder CurrentOrder { get; private set; } = HelmOrder.Stop;
        
        [SerializeField] public int requiredCrew = 1;
        public override int RequiredCrew => requiredCrew;

        public void SetOrder(HelmOrder order)
        {
            CurrentOrder = order;
        }

        public int GetDirectionForShip()
        {
            if (CurrentOrder == HelmOrder.Stop) return 0;
            bool isApproach = CurrentOrder == HelmOrder.Approach;
            
            if(owner == Faction.User)
                return isApproach ? 1 : -1;

            if (ShipManager.Instance == null ||
                ShipManager.Instance.playerShip == null ||
                ShipManager.Instance.enemyShip == null)
                return 0;
            
            float playerPos = ShipManager.Instance.playerShip.Position;
            float enemyPos = ShipManager.Instance.enemyShip.Position;
            
            float positionDifference = playerPos - enemyPos;

            if (Mathf.Abs(positionDifference) < 0.001f)
                return 0;

            int approachDirection = positionDifference > 0 ? 1 : -1;
            
            return isApproach ? approachDirection : -approachDirection;
        }

        public bool HasCrew => CurrentCrew > 0;

        protected override void OnMouseDown()
        {
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "BoardingScene") return;
            if (GameManager.Instance.GameState != GameState.UserTurn) return;
            if (owner != Faction.User) return;
            MenuManager.Instance.ShowHelmMenu(this);
        }
    }
}
