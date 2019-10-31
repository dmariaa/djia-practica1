using System;
using System.Collections.Generic;
using Assets.Scripts;
using Assets.Scripts.DataStructures;
using UnityEngine;

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
        

        public float this[CellInfo state, Locomotion.MoveDirection action]
        {
            get
            {            
                return table[getArrayPosition(state, action)];
            }

            set
            {
                table[getArrayPosition(state, action)] = value;
            }
        }

        public List<float> getStateValues(CellInfo state)
        {
            return new List<float>
            {
                table[getArrayPosition(state, Locomotion.MoveDirection.Up)],
                table[getArrayPosition(state, Locomotion.MoveDirection.Down)],
                table[getArrayPosition(state, Locomotion.MoveDirection.Left)],
                table[getArrayPosition(state, Locomotion.MoveDirection.Right)]
            };
        }

        public int getNextSavedDirection(CellInfo state)
        {
            List<float> stateValues = getStateValues(state);
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

        public float getHighestQAction(CellInfo state)
        {
            float highestValue = 0;
            List<float> stateValues = getStateValues(state);

            for(int i=0; i < stateValues.Count; i++)
            {
                if(stateValues[i] > highestValue)
                {
                    highestValue = stateValues[i];
                }
            }

            return highestValue;
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

        private int getArrayPosition(CellInfo state, Locomotion.MoveDirection action)
        {
            int row = state.RowId * this.boardColumns + state.ColumnId;
            int column = (int)action;
            return row * numActions + column;
        }
    }
}
