using Assets.Scripts;
using Assets.Scripts.DataStructures;
using UnityEngine;

namespace Practica1
{
    public class QMind : AbstractPathMind
    {
        [SerializeField]
        public bool UsePreviousLearning = false;

        [SerializeField]
        public float alpha = 0.3f;

        [SerializeField]
        public float gamma = 0.8f;

        private QTable qtable;

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


        private CellInfo getAleatNavigableCell(CellInfo currentPos, BoardInfo boardInfo, out Locomotion.MoveDirection direction)
        {
            CellInfo[] walkableNeighbours = currentPos.WalkableNeighbours(boardInfo);
            CellInfo newPos;

            do
            {
                int randomValue = Random.Range(0, walkableNeighbours.Length);
                newPos = walkableNeighbours[randomValue];
                direction = (Locomotion.MoveDirection)randomValue;
            } while (newPos == null);

            return newPos;
        }

        public override Locomotion.MoveDirection GetNextMove(BoardInfo boardInfo, CellInfo currentPos, CellInfo[] goals)
        {
            if(qtable==null)
            {
                Learn(boardInfo);
                qtable.SaveToCsv("test.csv");
            }

            // Proxima celda aleatoria
            Locomotion.MoveDirection nextDirection = (Locomotion.MoveDirection) qtable.GetNextSavedDirection(currentPos);
            return nextDirection;
        }

        public override void Repath()
        {
        }

        private void Learn(BoardInfo boardInfo)
        {
            CellInfo nextState, initialState;
            qtable = new QTable(boardInfo);
            float maxQValue = float.MinValue;
            float totalQValue = 0f;

            for (int episode = 0; episode < 10000; episode++)
            {
                initialState = boardInfo.CellInfos[
                    Random.Range(0, boardInfo.NumColumns),
                    Random.Range(0, boardInfo.NumRows)
                ];

                bool endOfEpisode = false;

                do
                {
                    // Elegimos una dirección aleatoria
                    int randomDirection = Random.Range(0, 4);

                    // Valor Q actual para la posición (estado) actual y la nueva dirección (accion) a tomar
                    float Q = qtable[initialState, (Locomotion.MoveDirection)randomDirection];
                    
                    // Calculamos recompensa para la próxima posición (estado)
                    nextState = initialState.WalkableNeighbours(boardInfo)[randomDirection];
                    float r = GetReward(nextState);
                    
                    // Máximo valor de Q para el próximo estado
                    float nextQMax = nextState != null ? qtable.GetHighestQAction(nextState) : 0;
                    
                    // Actualizamos tabla Q
                    float QValue = (1 - alpha) * Q + alpha * (r + gamma * nextQMax);
                    qtable[initialState, (Locomotion.MoveDirection)randomDirection] = QValue;
                    totalQValue += QValue;
                    maxQValue = QValue > maxQValue ? QValue : maxQValue;
                    
                    // Nos desplazamos al siguiente estado
                    initialState = nextState;
                    
                    // Condición de parada, hemos ido a una celda no navegable o hemos llegado al final
                    if (r == -1 || r == 100)
                    {
                        endOfEpisode = true;
                    }
                } while (!endOfEpisode);
                
                Debug.LogFormat("Episode {0} finished - MaxQ: {1} - TotalQ: {2}", episode, maxQValue, totalQValue);
            }
        }
    }
}
