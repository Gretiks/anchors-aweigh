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

        public void SetOrder(HelmOrder order)
        {
            CurrentOrder = order;
        }

        public int GetDirectionForShip()
        {
            if (CurrentOrder == HelmOrder.Stop) return 0;
            bool isApproach = CurrentOrder == HelmOrder.Approach;
            return owner == Faction.User ? (isApproach ? 1 : -1) : (isApproach ? -1 : 1);
        }

        public bool HasCrew => CurrentCrew > 0;

        protected override void OnMouseDown()
        {
            if (GameManager.Instance.GameState != GameState.UserTurn) return;
            if (owner != Faction.User) return;
            MenuManager.Instance.ShowHelmMenu(this);
        }
    }
}
