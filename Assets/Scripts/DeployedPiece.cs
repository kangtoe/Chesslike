using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class DeployedPiece : MonoBehaviour
{
    [SerializeField] SpriteRenderer _spriteRenderer;

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

#region 마우스 이벤트
    // 마우스 클릭 감지
    void OnMouseDown()
    {
        Debug.Log($"Piece clicked: {_pieceInfo.name} at position {_cellCoordinate}");
        PieceManager.Instance.SelectPiece(this);
    }

    // 마우스 오버 감지 (선택사항)
    void OnMouseEnter()
    {
        // 마우스가 올라왔을 때의 효과 (예: 스케일 변경)
        transform.localScale = Vector3.one * 1.1f;
    }

    // 마우스 아웃 감지 (선택사항)
    void OnMouseExit()
    {
        // 마우스가 벗어났을 때 원래 크기로 복원
        transform.localScale = Vector3.one;
    }
#endregion 마우스 이벤트
}
