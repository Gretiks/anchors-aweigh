using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core;
using Grid;
using UnityEngine;

namespace Assets.Scripts.Grid.Tiles.Modules
{
    public abstract class ShipModuleTile : Tile
    {
        [SerializeField] public Faction owner;
        [SerializeField] public int maxCrew = 3;

        [HideInInspector] public int currentOccupants = 0;

        public int CurrentCrew => CountAdjacentCrew();

        protected int CountAdjacentCrew()
        {
            var neighbors = GridManager.Instance.GetNeighbors(this);
            int count = 0;
            foreach (var tile in neighbors)
            {
                if (tile.OccupiedUnit != null && tile.OccupiedUnit.Faction == owner)
                    count++;
            }
            return Mathf.Min(count, maxCrew);
        }

        public new void SetRangeHighlight(bool active) { }
    }
}
