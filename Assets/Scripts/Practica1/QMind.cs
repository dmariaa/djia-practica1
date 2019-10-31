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
            }

            // Proxima celda aleatoria
            Locomotion.MoveDirection nextDirection = (Locomotion.MoveDirection) qtable.getNextSavedDirection(currentPos);
            
            /*
            CellInfo nextCell = getAleatNavigableCell(currentPos, boardInfo, out nextDirection);

            // Recalculo de Q
            float r = GetReward(nextCell);
            float Q = qtable[currentPos, nextDirection];
            float nextQMax = qtable.getHighestQAction(nextCell);
            qtable[currentPos, nextDirection] = (1 - alpha) * Q + alpha * (r + gamma * nextQMax);
            */
            
            return nextDirection;
        }

        public override void Repath()
        {
        }

        private void Learn(BoardInfo boardInfo)
        {
            CellInfo nextState, initialState;
            qtable = new QTable(boardInfo);

            for (int episode = 0; episode < 10000; episode++)
            {
                initialState = boardInfo.CellInfos[
                    Random.Range(0, boardInfo.NumColumns),
                    Random.Range(0, boardInfo.NumRows)
                ];

                bool endOfEpisode = false;

                do
                {
                    int randomDirection = Random.Range(0, 4);
                    nextState = initialState.WalkableNeighbours(boardInfo)[randomDirection];
                    float r = GetReward(nextState);
                    float Q = qtable[initialState, (Locomotion.MoveDirection)randomDirection];

                    float nextQMax = nextState != null ? qtable.getHighestQAction(nextState) : 0;
                    qtable[initialState, (Locomotion.MoveDirection)randomDirection] = (1 - alpha) * Q + alpha * (r + gamma * nextQMax);

                    if (r == -1 || r == 100)
                    {
                        endOfEpisode = true;
                    }

                    initialState = nextState;
                } while (!endOfEpisode);
                
                Debug.LogFormat("Episode {0} finished", episode);
            }
        }
    }
}
