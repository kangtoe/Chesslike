using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct PieceMovement
{
    public int row;
    public int col;
    public int diag;
}

public class Piece : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] PieceMovement pieceMovement;
    public PieceMovement PieceMovement => pieceMovement;
    public Vector2Int currentPosition;

    // 현재 위치를 업데이트하는 메서드
    public void SetPosition(Vector2Int newPosition)
    {
        currentPosition = newPosition;
    }

    // 피스의 이동 가능 여부를 확인하는 메서드
    public bool CanMove()
    {
        return pieceMovement.row > 0 || pieceMovement.col > 0 || pieceMovement.diag > 0;
    }
}
