using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PieceManager : MonoSingleton<PieceManager>
{
    [SerializeField] GameObject _pieceRoot;

    [Header("피스 프리팹")]
    [SerializeField] DeployedPiece _piecePrefab;

    [NaughtyAttributes.ReadOnly]
    [SerializeField] DeployedPiece _selectedPiece;
    public DeployedPiece SelectedPiece => _selectedPiece;

    // 키: 셀 좌표, 값: 배치된 피스
    Dictionary<Vector2Int, DeployedPiece> deployedPieces = new Dictionary<Vector2Int, DeployedPiece>();
    public Dictionary<Vector2Int, DeployedPiece> DeployedPieces => deployedPieces;

    bool IsValidCellCoordinate(Vector2Int cellCoordinate)
    {
        if(!BoardManager.Instance.IsValidCellCoordinate(cellCoordinate))
        {
            Debug.LogError($"Invalid cell coordinate: {cellCoordinate}");
            return false;
        }
        return true;
    }

    public bool TryGetPieceAt(Vector2Int cellCoordinate, out DeployedPiece piece)
    {
        return deployedPieces.TryGetValue(cellCoordinate, out piece);
    }

    #region 피스 제어
    public void SelectPiece(DeployedPiece piece)
    {
        if(piece == null) return;

        // 이미 선택된 피스가 있으면 공격 가능한지 확인
        if(_selectedPiece != null)
        {
            List<Vector2Int> _attackCells = null;
            BoardManager.Instance.GetMovableCells(_selectedPiece, out _, out _attackCells);
            if(_attackCells.Contains(piece.CellCoordinate))
            {
                MovePiece(_selectedPiece.CellCoordinate, piece.CellCoordinate);
                return;
            }
        }
        
        List<Vector2Int> movableCells = null;
        List<Vector2Int> attackCells = null;
        BoardManager.Instance.GetMovableCells(piece, out movableCells, out attackCells);
        
        //Debug.Log($"이동 가능한 셀: {string.Join(", ", movableCells)}");
        BoardManager.Instance.ActiveCellIndicator(movableCells, attackCells);

        _selectedPiece = piece;
    }    

    public void DeselectPiece()
    {
        BoardManager.Instance.ActiveCellIndicator(null);
        _selectedPiece = null;
    }

    public bool DeployPiece(PieceInfo pieceInfo, Vector2Int cellCoordinate, PieceColor pieceColor)
    {
        if(pieceInfo == null)
        {
            Debug.LogWarning("PieceInfo is null");
            return false;
        }

        if(!IsValidCellCoordinate(cellCoordinate)) return false;
        
        if(deployedPieces.ContainsKey(cellCoordinate))
        {
            Debug.LogWarning($"이미 좌표 {cellCoordinate}에 피스가 배치되어 있습니다.");
            return false;
        }

        BoardCell cell = BoardManager.Instance.GetCell(cellCoordinate);
        DeployedPiece piece = Instantiate(_piecePrefab, _pieceRoot.transform);
        piece.transform.position = cell.PieceDeployPoint;
        piece.InitPiece(pieceInfo, pieceColor);
        piece.SetCellCoordinate(cellCoordinate);

        // 딕셔너리에 추가
        deployedPieces[cellCoordinate] = piece;

        Debug.Log($"피스가 좌표 {cellCoordinate}에 배치되었습니다: {pieceInfo.name}");
        return true;
    }

    public bool MovePiece(Vector2Int toCoordinate)
    {
        if(_selectedPiece == null)
        {
            Debug.LogError("No piece selected");
            return false;
        }

        List<Vector2Int> movableCells = null;
        List<Vector2Int> attackCells = null;
        BoardManager.Instance.GetMovableCells(_selectedPiece, out movableCells, out attackCells);

        if(!movableCells.Contains(toCoordinate) && !attackCells.Contains(toCoordinate))
        {
            Debug.LogWarning($"Invalid cell coordinate: {toCoordinate}");
            return false;
        }

        return MovePiece(_selectedPiece.CellCoordinate, toCoordinate);
    }

    bool MovePiece(Vector2Int fromCoordinate, Vector2Int toCoordinate)
    {
        if(!IsValidCellCoordinate(toCoordinate)) return false;

        if(!deployedPieces.ContainsKey(fromCoordinate))
        {
            Debug.LogError($"시작 좌표 {fromCoordinate}에 피스가 없습니다.");
            return false;
        }

        // 목표 좌표에 이미 피스가 있다면 제거 (공격)
        if(deployedPieces.ContainsKey(toCoordinate))RemovePiece(toCoordinate);

        DeployedPiece piece = deployedPieces[fromCoordinate];
        BoardCell targetCell = BoardManager.Instance.GetCell(toCoordinate);
        
        // 피스 위치 업데이트
        piece.transform.position = targetCell.PieceDeployPoint;
        piece.SetCellCoordinate(toCoordinate);
        piece.MoveCount++;

        // 딕셔너리 업데이트
        deployedPieces.Remove(fromCoordinate);
        deployedPieces[toCoordinate] = piece;

        DeselectPiece();

        Debug.Log($"피스가 {fromCoordinate}에서 {toCoordinate}로 이동했습니다.");
        return true;
    }

    public bool RemovePiece(Vector2Int cellCoordinate)
    {
        if(!deployedPieces.ContainsKey(cellCoordinate))
        {
            Debug.LogWarning($"좌표 {cellCoordinate}에 피스가 없습니다.");
            return false;
        }

        DeployedPiece piece = deployedPieces[cellCoordinate];
        deployedPieces.Remove(cellCoordinate);
        Destroy(piece.gameObject);

        Debug.Log($"피스가 좌표 {cellCoordinate}에서 제거되었습니다.");
        return true;
    }
    #endregion 피스 제어

    #region FEN 변환
    /// <summary>
    /// 현재 배치된 피스들로부터 FEN 문자열을 반환합니다.
    /// </summary>
    public string GetFENFromCurrentBoard()
    {
        int width = BoardManager.Instance.BoardWidth;
        int height = BoardManager.Instance.BoardHeight;
        System.Text.StringBuilder fen = new System.Text.StringBuilder();

        for (int y = height - 1; y >= 0; y--) // FEN은 8(위)~1(아래) 순서
        {
            int emptyCount = 0;
            for (int x = 0; x < width; x++)
            {
                Vector2Int coord = new Vector2Int(x, y);
                if (deployedPieces.TryGetValue(coord, out DeployedPiece piece))
                {
                    if (emptyCount > 0)
                    {
                        fen.Append(emptyCount);
                        emptyCount = 0;
                    }
                    char symbol = piece.PieceInfo.pieceAlphabet;
                    // 백: 대문자, 흑: 소문자
                    fen.Append(piece.PieceColor == PieceColor.White ? char.ToUpper(symbol) : char.ToLower(symbol));
                }
                else
                {
                    emptyCount++;
                }
            }
            if (emptyCount > 0)
                fen.Append(emptyCount);
            if (y > 0)
                fen.Append('/');
        }
        return fen.ToString();
    }
    #endregion FEN 변환
}
