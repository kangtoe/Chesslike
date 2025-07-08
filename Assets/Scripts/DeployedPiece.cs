using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class DeployedPiece : MonoBehaviour
{
    [SerializeField] SpriteRenderer _spriteRenderer;
    [SerializeField] PieceUI _pieceUI;

    [NaughtyAttributes.ReadOnly]
    [SerializeField] PieceInfo _pieceInfo;
    public PieceInfo PieceInfo => _pieceInfo;
        
    [NaughtyAttributes.ReadOnly]
    [SerializeField] Vector2Int _cellCoordinate = new Vector2Int(-1, -1);
    public Vector2Int CellCoordinate => _cellCoordinate;
    
    [NaughtyAttributes.ReadOnly]
    [SerializeField] PieceColor _pieceColor = PieceColor.White;
    public PieceColor PieceColor => _pieceColor;

    [NaughtyAttributes.ReadOnly]
    [SerializeField] int _moveCount = 0;
    public int MoveCount { get { return _moveCount; } set { _moveCount = value; } }

    // Start is called before the first frame update
    void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();        
        UpdateUI();
    }

    public void InitPiece(PieceInfo pieceInfo, PieceColor pieceColor)
    {
        _pieceInfo = pieceInfo;
        _pieceColor = pieceColor;
        _spriteRenderer.sprite = _pieceColor == PieceColor.White ? _pieceInfo.whiteSprite : _pieceInfo.blackSprite;
    }

    public void SetCellCoordinate(Vector2Int cellCoordinate)
    {
        _cellCoordinate = cellCoordinate;
    }
    
    void UpdateUI()
    {
        Debug.Log("UpdateUI");

        _pieceUI?.SetHealth(_pieceInfo.health, _pieceInfo.health);
        _pieceUI?.SetDamage(_pieceInfo.attack);
    }
}
