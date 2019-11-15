using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Assets.Scripts.DataStructures
{
    public class CellInfo : ICloneable
    {
        public PlaceableItem ItemInCell { get; set; }

        public int RowId { get; private set; }
        public int ColumnId { get; private set; }

        public string CellId { get { return "" + this.ColumnId + "," + this.RowId; } }
        public float WalkCost { get; private set; }
        public bool Walkable { get; private set; }

        public CellInfo(int col, int row)
        {
            this.WalkCost = 1.0f;
            this.Walkable = true;
            this.RowId = row;
            this.ColumnId = col;
            this.ItemInCell = null;
        }

        
        public Vector2 GetPosition
        {
            get
            {
                return new Vector2(this.ColumnId, this.RowId);
            }
        }

        public void ChangeToNoWalkable()
        {
            this.Walkable = false;
            this.WalkCost = float.MaxValue;
        }

        public object Clone()
        {
            var result = new CellInfo(this.ColumnId, this.RowId)
            {
                Walkable = this.Walkable,
                WalkCost = this.WalkCost
            };
            if (this.ItemInCell != null)
                ItemInCell = (PlaceableItem)this.ItemInCell.Clone();


            return result;
        }

        public GameObject CreateGameObject(BoardManager boardManager)
        {
            var tile = boardManager.floorTile;
            if (!this.Walkable) tile = boardManager.wallTile;
            var go =  GameObject.Instantiate(tile, new Vector3(this.ColumnId, this.RowId, 0f),
                Quaternion.identity, boardManager.transform);
            if (ItemInCell != null)
            {
                var itGO = this.ItemInCell.CreateGameObject(boardManager, go.transform);
                itGO.GetComponent<ItemLogic>().Type = this.ItemInCell.Type;
                itGO.GetComponent<ItemLogic>().PlaceableItem = this.ItemInCell;

            }
            return go;
        }

        public CellInfo[] WalkableNeighbours(BoardInfo board)
        {
            var neighbours = new CellInfo[4];
            //Up
            neighbours[0] = (this.RowId < (board.NumRows -1) && board.CellInfos[this.ColumnId, this.RowId + 1].Walkable)
                ? board.CellInfos[this.ColumnId, this.RowId + 1]
                : null;
            //Down
            neighbours[1] = (this.RowId > 0 && board.CellInfos[this.ColumnId, this.RowId - 1].Walkable)
                ? board.CellInfos[this.ColumnId, this.RowId - 1]
                : null;
            //Left
            neighbours[2] = (this.ColumnId > 0 && board.CellInfos[this.ColumnId - 1, this.RowId].Walkable)
                ? board.CellInfos[this.ColumnId - 1, this.RowId]
                : null;
            //Right
            neighbours[3] = (this.ColumnId < (board.NumColumns - 1) && board.CellInfos[this.ColumnId + 1, this.RowId].Walkable)
                ? board.CellInfos[this.ColumnId + 1, this.RowId]
                : null;
            return neighbours;
        }
    }
}