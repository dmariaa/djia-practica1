using System;
using System.Collections;
using System.Globalization;
using System.IO;
using Assets.Scripts;
using Assets.Scripts.DataStructures;
using UnityEngine;
using UnityEngine.Internal.Experimental.UIElements;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Practica1
{
    /// <summary>
    /// Aprendizaje QLearning para Practica 1 de DJIA
    /// </summary>
    public class QMind : AbstractPathMind
    {
        private static readonly CultureInfo cultureInfo = new CultureInfo("es-ES");
        
        // Parametros del algoritmo
        public bool forgetPreviousLearning = false;
        public float alpha = 0.3f;
        public float gamma = 0.8f;
        public float epsilon = 1.0f;
        public float epsilonDecayRate = 0.99995f;
        public float epsilonMinimumValue = 0.005f;
        public int numberOfEpisodes = 10000;
        
        // Para mostrar el progreso del aprendizaje
        public GameObject loadingPanel;
        private Slider progressBar;
        private Text progressText;
        private Text iterationsText;
        private Text subTitle;
        private bool startedLearn = false;    // Ha comenzado el algoritmo a aprender?
        
        private QTable qtable;                // Tabla Q
        private int episode = 0;              // Episodio actual
        
        /// <summary>
        /// Inicializa los componentes del panel de carga si este está asignado
        /// </summary>
        private void Start()
        {
            if (loadingPanel)
            {
                progressBar = loadingPanel.transform.Find("ProgressBar").GetComponent<Slider>();
                progressText = progressBar.transform.Find("PctText").GetComponent<Text>();
                iterationsText = progressBar.transform.Find("IterationsText").GetComponent<Text>();
                subTitle = loadingPanel.transform.Find("Subtitle").GetComponent<Text>();
            }
        }
        
        /// <summary>
        /// Función de recompensa para una celda.
        /// </summary>
        /// <param name="cell">Celda</param>
        /// <returns>Valor de recompensa</returns>
        private float GetReward(CellInfo cell)
        {
            // Ha intentado moverse a una celda no transitable
            if(cell==null || !cell.Walkable)
            {
                return -1.0f;
            }

            if(cell.ItemInCell != null && cell.ItemInCell.Type==PlaceableItem.ItemType.Goal)
            {
                return 100.0f;
            }

            return 0.0f;
        }
        
        /// <summary>
        /// Función para aprender mediante QLearning.
        /// </summary>
        /// <param name="boardInfo">Tablero de juego</param>
        /// <returns></returns>
        private IEnumerator Learn(BoardInfo boardInfo)
        {
            if(subTitle) subTitle.text = String.Format(cultureInfo, "Número de episodios: {0:n0}", numberOfEpisodes);
            if(loadingPanel) loadingPanel.SetActive(true);
            yield return null;
            
            CellInfo nextState, currentState;            // Estado actual y próximo
            Locomotion.MoveDirection direction;          // Dirección proximo movimiento
            float Q, r;                                  // Valor Q y recompensa
            qtable = new QTable(boardInfo);              // Nueva tabla Q
            float maxQValue = float.MinValue;            // valor máximo Q en toda la tabla
            float totalQValue = 0f;                      // Suma de todos los valores Q de la tabla
            var epsilon = this.epsilon;                  // Epsilon clone

            for (episode = 0; episode < numberOfEpisodes; episode++)    // Episodios
            {
                // Elección de una celda de inicio aleatoria para el episodio
                currentState = boardInfo.CellInfos[
                    Random.Range(0, boardInfo.NumColumns),
                    Random.Range(0, boardInfo.NumRows)
                ];

                bool endOfEpisode = false;

                do
                {
                    // Elige una nueva dirección de forma aleatoria o mediante los valores Q del estado actual
                    // en función de epsilon, el ratio de aprendizaje-exploracion
                    if (Random.Range(0.0f, 1.0f) < epsilon)
                    {
                        // Elegimos una dirección aleatoria
                        direction = (Locomotion.MoveDirection)Random.Range(0, 4);
                    }
                    else
                    {
                        // Elegimos la mejor posición según la tabla Q
                        direction = (Locomotion.MoveDirection)qtable.GetHighestQDirection(currentState);
                    }
                    
                    // Valor Q actual para la posición (estado) actual y la nueva dirección (accion) a tomar
                    Q = qtable[currentState, direction];
                    
                    // Calculamos recompensa para la próxima posición (estado)
                    nextState = currentState.WalkableNeighbours(boardInfo)[(int)direction];
                    r = GetReward(nextState);
                    
                    // Máximo valor de Q para el próximo estado
                    float nextQMax = nextState != null ? qtable.GetHighestQValue(nextState) : 0;
                    
                    // Actualizamos tabla Q
                    float QValue = (1 - alpha) * Q + alpha * (r + gamma * nextQMax);
                    qtable[currentState, direction] = QValue;
                    totalQValue += QValue;
                    maxQValue = QValue > maxQValue ? QValue : maxQValue;
                    
                    // Nos desplazamos al siguiente estado
                    currentState = nextState;
                    
                    // Condición de parada, hemos ido a una celda no navegable o hemos llegado al final
                    if (r == -1 || r == 100)
                    {
                        endOfEpisode = true;
                    }
                } while (!endOfEpisode);
                
                // Reducimos epsilon, para cada vez explorar menos y usar mas lo aprendido
                if(epsilon >= epsilonMinimumValue) epsilon *= epsilonDecayRate; 

                // Actualizamos avance
                float pct = (episode + 1.0f) / numberOfEpisodes;
                if (progressBar != null) progressBar.value = pct;
                if (progressText != null) progressText.text = (int)(pct * 100) + "%";
                if (iterationsText != null) iterationsText.text = "Episodios: " + (episode + 1);
                
                if(episode % 100 == 0)
                {
                    yield return null;
                }
            }

            if(loadingPanel) loadingPanel.SetActive(false);
            qtable.SaveToCsv("qtable.csv");
        }
        
        public override Locomotion.MoveDirection GetNextMove(BoardInfo boardInfo, CellInfo currentPos, CellInfo[] goals)
        {
            if (!startedLearn)
            {
                if (File.Exists("qtable.csv") && !forgetPreviousLearning)
                {
                    qtable = QTable.LoadFromCsv("qtable.csv", boardInfo);
                    episode = numberOfEpisodes;
                }
                else
                {
                    StartCoroutine(Learn(boardInfo));
                }

                startedLearn = true;
            }
            
            if(episode==numberOfEpisodes)
            {
                // Proxima celda aleatoria
                Locomotion.MoveDirection nextDirection = (Locomotion.MoveDirection) qtable.GetHighestQDirection(currentPos);
                return nextDirection;
            }

            return Locomotion.MoveDirection.None;    
        }

        public override void Repath()
        {
        }
    }
}
