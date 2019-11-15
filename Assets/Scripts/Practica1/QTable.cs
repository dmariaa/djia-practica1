using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Assets.Scripts;
using Assets.Scripts.DataStructures;
using UnityEngine;

namespace Practica1
{
    /// <summary>
    /// Tabla Q para QLearning.
    /// </summary>
    public class QTable
    {
        private static readonly CultureInfo cultureInfo = new CultureInfo("es-ES");

        /// <summary>
        /// La tabla que contiene los datos
        /// </summary>
        private List<float> table;
        
        /// <summary>
        /// Número de estados
        /// </summary>
        private int numStates;
        
        /// <summary>
        /// Número de acciones
        /// </summary>
        private int numActions;
        
        /// <summary>
        /// Columnas en el board info
        /// </summary>
        private int boardColumns;

        /// <summary>
        /// Acceso mediante indice
        /// </summary>
        /// <param name="index">Índice</param>
        public float this[int index]
        {
            get { return table[index];  }
            set { table[index] = value;  }
        }

        /// <summary>
        /// Acceso mediante índice de estado e índice de acción
        /// </summary>
        /// <param name="state">Indice de estado</param>
        /// <param name="action">Indice de acción</param>
        public float this[int state, int action]
        {
            get { return table[state * numActions + action]; }
            set { table[state * numActions + action] = value;  }
        }
        
        /// <summary>
        /// Acceso mediante estado (CellInfo) y acción (MoveDirection)
        /// </summary>
        /// <param name="state">Estado</param>
        /// <param name="action">Acción</param>
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
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="board">Tablero de juego</param>
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
        
        /// <summary>
        /// Devuelve la acción con mayor valor Q para un determinado estado
        /// </summary>
        /// <param name="state">Estado</param>
        /// <returns>Índice de la acción con mayor valor Q</returns>
        public int GetHighestQDirection(CellInfo state)
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

        /// <summary>
        /// Devuelve el mayor valor Q para un determinado estado
        /// </summary>
        /// <param name="state">Estado</param>
        /// <returns>Mayor valor Q</returns>
        public float GetHighestQValue(CellInfo state)
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
        
        /// <summary>
        /// Calcula la posición dentro de la tabla para un estado y una acción
        /// </summary>
        /// <param name="state">Estado</param>
        /// <param name="action">Acción</param>
        /// <returns>Indice dentro de la tabla Q</returns>
        private int GetArrayPosition(CellInfo state, Locomotion.MoveDirection action)
        {
            int row = state.RowId * this.boardColumns + state.ColumnId;
            int column = (int)action;
            return row * numActions + column;
        }
        
        /// <summary>
        /// Devuelve los valores Q asociados a las acciones de un estado
        /// </summary>
        /// <param name="state">Estado</param>
        /// <returns>Lista de valores Q</returns>
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

        /// <summary>
        /// Almacena la tabla en un fichero csv
        /// </summary>
        /// <param name="path">Path y nombre del fichero a guardar</param>
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
                    csvString.AppendFormat(cultureInfo, ";{0}", this[i, j]);
                }

                csvString.AppendLine();
            }
            
            StreamWriter file = System.IO.File.CreateText(path);
            file.Write(csvString);
            file.Close();
        }

        /// <summary>
        /// Lee un fichero csv para generar una QTable
        /// </summary>
        /// <param name="path">El fichero a leer</param>
        /// <param name="boardInfo">Información del tablero de juego</param>
        /// <returns>QTable con los valores almacenados en el fichero csv</returns>
        public static QTable LoadFromCsv(string path, BoardInfo boardInfo)
        {
            QTable qtable = new QTable(boardInfo);
            StreamReader streamReader = new StreamReader(path);
            string line;
            float[] data;
            int nLine = 0;
            
            qtable.table.Clear();
            streamReader.ReadLine();    // Saltar cabeceras
            
            while ((line = streamReader.ReadLine()) != null)
            {
                data = qtable.GetCsvValues(line);
                qtable.table.AddRange(data);
            }

            return qtable;
        }

        /// <summary>
        /// Convierte una linea del fichero CSV en un array de valores Q
        /// </summary>
        /// <param name="line">Linea del csv</param>
        /// <returns>Array de valores Q</returns>
        private float[] GetCsvValues(string line)
        {
            ArraySegment<string> stringValues = new ArraySegment<string>(line.Split(';'), 1, 5);
            Debug.Log(String.Join(" ", stringValues));
            return Array.ConvertAll(stringValues.ToArray(),
                input => float.Parse(input, cultureInfo.NumberFormat));
        }
    }
}
