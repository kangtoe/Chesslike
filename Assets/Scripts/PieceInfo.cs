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
    public int rookMove;    
    public int bishopMove;
    public bool isKnight;
    public bool isPawn;
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
