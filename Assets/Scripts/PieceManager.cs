using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PieceManager : MonoSingleton<PieceManager>
{
    [SerializeField] GameObject _pieceRoot;

    [Header("피스 프리팹")]
    [SerializeField] DeployedPiece _piecePrefab;

    // 키: 셀 좌표, 값: 배치된 피스
    Dictionary<Vector2Int, DeployedPiece> deployedPieces = new Dictionary<Vector2Int, DeployedPiece>();

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

    #region 피스 선택
    public void SelectPiece(DeployedPiece piece)
    {
        if(piece == null) return;
        List<Vector2Int> movableCells = BoardManager.Instance.GetMovableCells(piece);
        Debug.Log($"이동 가능한 셀: {string.Join(", ", movableCells)}");
        BoardManager.Instance.ActiveMoveIndicator(movableCells);
    }
    #endregion 피스 선택

    #region 피스 제어
    public bool DeployPiece(PieceInfo pieceInfo, Vector2Int cellCoordinate, PieceColor pieceColor)
    {
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

    public bool MovePiece(Vector2Int fromCoordinate, Vector2Int toCoordinate)
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

    #region 디버그
    [Header("디버그")]
    [SerializeField] PieceInfo _pieceInfo;
    [SerializeField] PieceColor _pieceColor = PieceColor.White;
    [SerializeField] Vector2Int _coordinate;
    
    [NaughtyAttributes.Button]
    void DeployPiece_Debug()
    {        
        DeployPiece(_pieceInfo, _coordinate, _pieceColor);
    }

    [NaughtyAttributes.Button]
    public void PrintBoardState()
    {
        Debug.Log("=== 보드 상태 ===");
        if(deployedPieces.Count == 0)
        {
            Debug.Log("배치된 피스가 없습니다.");
            return;
        }

        foreach(var kvp in deployedPieces)
        {
            Vector2Int coordinate = kvp.Key;
            DeployedPiece piece = kvp.Value;
            Debug.Log($"좌표 {coordinate}: {piece.name}");
        }
        Debug.Log("================");

        Debug.Log($"FEN: {GetFENFromCurrentBoard()}");
    }

    [NaughtyAttributes.Button]
    void ClearAllPieces()
    {
        // 딕셔너리의 키들을 리스트로 변환하여 역순으로 처리
        List<Vector2Int> coordinates = new List<Vector2Int>(deployedPieces.Keys);
        for(int i = coordinates.Count - 1; i >= 0; i--)
        {
            RemovePiece(coordinates[i]);
        }
        Debug.Log("모든 피스가 제거되었습니다.");

        BoardManager.Instance.ActiveMoveIndicator(null);
    }
    #endregion 디버그

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
