using System;
using Assets.Scripts.DataStructures;
using UnityEngine;

namespace Assets.Scripts
{
    public class Locomotion : MonoBehaviour
    {
        public enum MoveDirection
        {
            Up,
            Down,
            Left,
            Right, None
        }

        private CellInfo end;
        private Rigidbody2D rb;
        public float speed=1.0f;
        
        private CellInfo last;

        public AbstractPathMind MindController { get; set; }
        private BoardInfo Board { get { return character.BoardManager.boardInfo; } }

        public bool keyControl = false;
        private float cooldown = 1.0f;

        private CharacterBehaviour character;
        
        // Use this for initialization
        void Start()
        {
            rb = GetComponent<Rigidbody2D>();


            MoveNeed = true;
            end = null;

        }

        public void MoveLeft()
        {
            if (end.ColumnId == 0)
                return;
            end = Board.CellInfos[end.ColumnId - 1, end.RowId];
            Debug.Log("Left\n");
        }

        public void MoveUp()
        {
            if (end.RowId == Board.NumRows)
                return;
            end = Board.CellInfos[end.ColumnId, end.RowId+1];
            Debug.Log("Up\n");
        }

        public void MoveDown()
        {
            if (end.RowId == 0)
                return;
            end = Board.CellInfos[end.ColumnId, end.RowId - 1];
            Debug.Log("Down\n");
        }

        public void MoveRight()
        {
            if (end.ColumnId == Board.NumColumns)
                return;
            end = Board.CellInfos[end.ColumnId + 1, end.RowId];
            Debug.Log("Right\n");
        }

        public void KeyControl()
        {
            cooldown -= Time.deltaTime;

            if (cooldown < 0)
            {
                if (Input.GetKeyDown(KeyCode.DownArrow))
                {
                    MoveDown();
                    cooldown = 1.0f;
                }
                else if (Input.GetKey(KeyCode.UpArrow))
                {
                    MoveUp();
                    cooldown = 1.0f;
                }
                else if (Input.GetKey(KeyCode.LeftArrow))
                {
                    MoveLeft();
                    cooldown = 1.0f;
                }
                else if (Input.GetKey(KeyCode.RightArrow))
                {
                    MoveRight();
                    cooldown = 1.0f;
                }
            }
        }

        public bool AtDestination()
        {
            if (last == null || end == null) return true;

            if (Vector2.Distance(last.GetPosition, end.GetPosition) > 0.10)//float.Epsilon)
            {
                var dist = Vector2.Distance(transform.position, end.GetPosition);
                return dist < 0.10;
            }
            else
            {
                return true;
            }
        }

        public void SetNewDirection(MoveDirection newDirection)
        {
            if (end == null)
            {
                end = Board.CellInfos[(int)Math.Round(transform.position.x), (int)Math.Round(transform.position.y)];
            }
            last = end;
            switch (newDirection)
            {
                    case MoveDirection.Up: MoveUp(); break;
                    case MoveDirection.Down: MoveDown(); break;
                    case MoveDirection.Left: MoveLeft();
                        break;
                    case MoveDirection.Right: MoveRight();
                        break;


                case MoveDirection.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException("NewDirection "+newDirection);
            }
           
        }

        // Update is called once per frame
        void Update()
        {
            if (end == null) return;
           
            if (!AtDestination())
            {
                MoveNeed = false;
                Vector2 pos = Vector2.Lerp(transform.position, end.GetPosition, Math.Max(0.05f,Time.deltaTime*speed));
                rb.MovePosition(pos);
            
            }
            else
            {
                transform.position = end.GetPosition;
                MoveNeed = true;
                
            }
        }

        public bool MoveNeed { get; set; }

        public CellInfo CurrentPosition()
        {
            ;
            return Board.CellInfos[(int)Math.Round(transform.position.x), (int) Math.Round(transform.position.y)];
        }
        public CellInfo CurrentEndPosition()
        {
            return end ?? CurrentPosition();
        }

        public void SetCharacter(CharacterBehaviour characterBehaviour)
        {
            this.character = characterBehaviour;
        }
    }
}