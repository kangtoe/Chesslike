using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PieceColor
{
    Black = -1,
    White = 1
}

[System.Serializable]
public struct PieceMovement
{
    public bool isRookMove;    
    public bool isBishopMove;
    public bool isKnightMove;
    public bool isPawnMove;
    public bool isKingMove;
}

[CreateAssetMenu(fileName = "New Piece Info", menuName = "Piece Info")]
public class PieceInfo : ScriptableObject
{
    [NaughtyAttributes.ReadOnly]
    public string pieceName;
    public char pieceAlphabet;
    public Sprite blackSprite;
    public Sprite whiteSprite;

    [Header("Stats")]
    public int value;
    public int health;
    public int attack;
    public int defense;

    [Header("Movement")]
    public PieceMovement movement;

    void OnValidate()
    {
        pieceName = name;
    }
}
