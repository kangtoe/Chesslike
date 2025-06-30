using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct PieceMovement
{
    public int row;
    public int col;
    public int diag;
    public bool isKnight;
    public bool isPawn;
}

[CreateAssetMenu(fileName = "New Piece Info", menuName = "Piece Info")]
public class PieceInfo : ScriptableObject
{
    [NaughtyAttributes.ReadOnly]
    public string pieceName;
    public char pieceAlphabet;
    public Sprite sprite;

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
