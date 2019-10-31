using Assets.Scripts.DataStructures;
using UnityEngine;

namespace Assets.Scripts
{
    public abstract class AbstractPathMind: MonoBehaviour
    {
        protected CharacterBehaviour character;

        public abstract void Repath();
        
        public abstract Locomotion.MoveDirection GetNextMove(BoardInfo boardInfo, CellInfo currentPos, CellInfo[] goals);

        public void SetCharacter(CharacterBehaviour characterBehaviour)
        {
            this.character = characterBehaviour;
        }
    }
}
