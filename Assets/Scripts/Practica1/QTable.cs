using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Assets.Scripts;
using Assets.Scripts.DataStructures;
using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;

namespace Practica1
{
    [Serializable]
    public class QTable
    {
        [SerializeField]
        private List<float> table;
        private int numStates;
        private int numActions;
        private int boardColumns;

        public float this[int state, int action]
        {
            get { return table[state * numActions + action]; }
        }
        
        public float this[CellInfo state, Locomotion.MoveDirection action]
        {
            get
            {            
                return table[GetArrayPosition(state, action)];
            }

            set
            {
                table[GetArrayPosition(state, action)] = value;
            }
        }
        
        public QTable()
        {
            int size = this.numStates * this.numActions;
            table = new List<float>(size);
        }

        public QTable(BoardInfo board)
        {
            this.boardColumns = board.NumColumns;
            this.numStates = board.NumColumns * board.NumRows;
            this.numActions = System.Enum.GetValues(typeof(Locomotion.MoveDirection)).Length;
            
            int size = this.numStates * this.numActions;
            table = new List<float>(size);
            for(int i=0; i < size; i++)
            {
                table.Add(0);
            }
        }
        
        public int GetNextSavedDirection(CellInfo state)
        {
            List<float> stateValues = GetStateValues(state);
            int nextDirection = 0;
            float highestValue = 0;
            
            for(int i=0; i < stateValues.Count; i++)
            {
                if(stateValues[i] > highestValue)
                {
                    highestValue = stateValues[i];
                    nextDirection = i;
                }
            }
            return nextDirection;
        }

        public float GetHighestQAction(CellInfo state)
        {
            float highestValue = 0;
            List<float> stateValues = GetStateValues(state);

            for(int i=0; i < stateValues.Count; i++)
            {
                if(stateValues[i] > highestValue)
                {
                    highestValue = stateValues[i];
                }
            }

            return highestValue;
        }
        
        private int GetArrayPosition(CellInfo state, Locomotion.MoveDirection action)
        {
            int row = state.RowId * this.boardColumns + state.ColumnId;
            int column = (int)action;
            return row * numActions + column;
        }
        
        private List<float> GetStateValues(CellInfo state)
        {
            return new List<float>
            {
                table[GetArrayPosition(state, Locomotion.MoveDirection.Up)],
                table[GetArrayPosition(state, Locomotion.MoveDirection.Down)],
                table[GetArrayPosition(state, Locomotion.MoveDirection.Left)],
                table[GetArrayPosition(state, Locomotion.MoveDirection.Right)]
            };
        }

        public void SaveToCsv(string path)
        {
            StringBuilder csvString = new StringBuilder();
            csvString.AppendFormat(";UP;DOWN;LEFT;RIGHT;NOMOVE\n");
            
            for (int i = 0; i < this.numStates; i++)
            {
                int row = (int) (i / this.boardColumns);
                int col = i % this.boardColumns;

                csvString.AppendFormat("<{0},{1}>", row, col);
                
                for (int j = 0; j < this.numActions; j++)
                {
                    csvString.AppendFormat(new CultureInfo("es-ES"), ";{0}", this[i, j]);
                }

                csvString.AppendLine();
            }
            
            StreamWriter file = System.IO.File.CreateText(path);
            file.Write(csvString);
            file.Close();
        }
    }
}
