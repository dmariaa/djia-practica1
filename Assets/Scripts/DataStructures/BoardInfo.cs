using System;
using System.Collections.Generic;
using System.Linq;
//- using System.Security.Policy;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts.DataStructures
{
    public class BoardInfo : ICloneable
    {
        public BoardInfo(int columns, int rows, BoardManager manager)
        {
            this.NumColumns = columns;
            this.NumRows = rows;
            this.CellInfos = new CellInfo[columns, rows];
            this.manager = manager;

        }

        private BoardManager manager;
        public int NumColumns { get; private set; }
        public int NumRows { get; private set; }
        public CellInfo[,] CellInfos { get; set; }

        public List<GameObject> Enemies { get; set; }

        public List<PlaceableItem> ItemsOnBoard
        {
            get
            {
                return (from CellInfo cell in this.CellInfos where cell.ItemInCell != null select cell.ItemInCell).ToList();
            }
        }

        public CellInfo Exit
        {
            get
            {
                return (from CellInfo cell in this.CellInfos where cell.ItemInCell != null && cell.ItemInCell.Type==PlaceableItem.ItemType.Goal select cell).First();
            }
        }

        public CellInfo CellWithItem(string tag)
        {
            return (from CellInfo cell in this.CellInfos
                where cell.ItemInCell != null && cell.ItemInCell.Tag == tag
                select cell).First();
        }

        public List<CellInfo> EmptyCells
        {
            get
            {
                return (from CellInfo cell in this.CellInfos where cell.ItemInCell == null select cell).ToList();
            }
        }

        private void CleanBoard()
        {
            this.CellInfos = new CellInfo[this.NumColumns, this.NumRows];
            //Instantiate Board and set boardHolder to its transform.
            //Loop along x axis, starting from -1 (to fill corner) with floor or outerwall edge tiles.
            for (var x = 0; x < this.NumColumns; x++)
            {
                //Loop along y axis, starting from -1 to place floor or outerwall tiles.
                for (var y = 0; y < this.NumRows; y++)
                {
                    this.CellInfos[x, y] = new CellInfo(x, y);
                }
            }
            this.Enemies = new List<GameObject>();
        }

        private void LayoutWallAtRandom(int minWalls, int maxWalls)
        {
            var numWalls = UnityEngine.Random.Range(minWalls, maxWalls);
            while (numWalls > 0)
            {
                var toTestCol = UnityEngine.Random.Range(0, this.CellInfos.GetLength(0));
                var toTestRow = UnityEngine.Random.Range(0, this.CellInfos.GetLength(1));
                if (!this.CellInfos[toTestCol, toTestRow].Walkable) continue;

                this.CellInfos[toTestCol, toTestRow].ChangeToNoWalkable();
                numWalls--;
            }
        }

        private void LayoutItemsAtRandom(int minLever, int maxLever)
        {
            var emptyCells = this.EmptyCells;
            var objectCount = Random.Range(minLever, Math.Min(maxLever, emptyCells.Count - 1));

            //Instantiate objects until the randomly chosen limit objectCount is reached
            for (var i = 0; i < objectCount; i++)
            {
                var cell = emptyCells[UnityEngine.Random.Range(0, emptyCells.Count)];

                var itemInfo = new PlaceableItem("Object_" + i, PlaceableItem.ItemType.Lever);

                GeneratePrerequisites(itemInfo);

                cell.ItemInCell = itemInfo;
                emptyCells = this.EmptyCells;
            }

        }

        private void LayoutEnemiesAtRandom(int numEnemies)
        {
            var emptyCells = this.EmptyCells;
            

            //Instantiate objects until the randomly chosen limit objectCount is reached
            for (var i = 0; i < numEnemies; i++)
            {
                var cell = emptyCells[UnityEngine.Random.Range(0, emptyCells.Count)];
                
                var itemInfo = new PlaceableItem("Enemy_" + i, PlaceableItem.ItemType.Enemy);
                var enemy = GameObject.Instantiate(manager.enemyTile,cell.GetPosition,//new Vector3(cell.GetPosition.x, cell.GetPosition.y, 0),
                    Quaternion.identity);
                enemy.name = "Enemy_" + i;
                enemy.GetComponent<EnemyBehaviour>().BoardManager = manager;
                var itemlogic = enemy.GetComponentInChildren<ItemLogic>();
                itemlogic.PlaceableItem = itemInfo;

                
                emptyCells = this.EmptyCells;
            }

        }

        private void LayoutGoalAtRandom(bool forPlanner)
        {
            var emptyCells = this.EmptyCells;
            var goalCell = emptyCells[Random.Range(0, emptyCells.Count)];
            goalCell.ItemInCell = new PlaceableItem("Goal", PlaceableItem.ItemType.Goal);
            if (forPlanner)
            {
                GeneratePrerequisites(goalCell.ItemInCell);
            }
        }
        private void GeneratePrerequisites(PlaceableItem item)
        {
            var values = new HashSet<int>();
            var value = 0;
            var generatedObjects = this.ItemsOnBoard;

            do
            {
                value = UnityEngine.Random.Range(-1, generatedObjects.Count - 1);
                if (value > 0)
                {
                    values.Add(value);
                }
            } while (value > 0);
            foreach (var i in values)
            {
                item.Preconditions.Add(generatedObjects[i]);
            }
        }
        public void SetupBoard(int seed, bool forPlanner, BoardManager.Count wallCount, BoardManager.Count leverCount, int enemyCount)
        {
            Random.InitState(seed);
            //Creates the outer walls and floor.
            CleanBoard();


            //Instantiate a random number of wall tiles based on minimum and maximum, at randomized positions.
            LayoutWallAtRandom(wallCount.minimum, wallCount.maximum);

            if (forPlanner)
            {
                //Instantiate a random number of food tiles based on minimum and maximum, at randomized positions.
                LayoutItemsAtRandom(leverCount.minimum, leverCount.maximum);
            }

           

            LayoutGoalAtRandom(forPlanner);
            if (enemyCount > 0)
            {
                LayoutEnemiesAtRandom(enemyCount);
            }
        }

        public object Clone()
        {
            var info = new BoardInfo(this.NumColumns, this.NumRows, this.manager) { CellInfos = (CellInfo[,])this.CellInfos.Clone() };

            return info;
        }

        
        public GameObject CreateGameObject(BoardManager boardManager)
        {
      
            var board = new GameObject("Board");
            foreach (var cellInfo in this.CellInfos)
            {
                var cellGO = cellInfo.CreateGameObject(boardManager);
                cellGO.transform.parent = board.transform.parent;
            }
            return board;
        }
    }
}
