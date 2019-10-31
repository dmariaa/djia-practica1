using System.Collections;
using System.Collections.Generic;
using Assets.Scripts;
using Assets.Scripts.DataStructures;
using UnityEngine;
[RequireComponent(typeof(Locomotion))]
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyBehaviour : CharacterBehaviour {

    
    public CellInfo CurrentPosition()
    {
        return BoardManager.boardInfo.CellInfos[(int) transform.position.x, (int) transform.position.y];
    }

    void Awake()
    {

        PathController = GetComponentInChildren<AbstractPathMind>();
        PathController.SetCharacter(this);
        LocomotionController = GetComponent<Locomotion>();
        LocomotionController.SetCharacter(this);



    }

    void Update()
    {
        if (BoardManager == null) return;
        if (LocomotionController.MoveNeed)
        {

            
            LocomotionController.SetNewDirection(PathController.GetNextMove(BoardManager.boardInfo, LocomotionController.CurrentEndPosition(), null));
        }
    }




}
