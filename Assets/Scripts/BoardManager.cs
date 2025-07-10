using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoSingleton<BoardManager>
{
    [Header("보드 설정")]
    [SerializeField] GameObject boardRoot;
    [SerializeField] private int boardWidth = 8;  // 보드 가로 크기
    [SerializeField] private int boardHeight = 8; // 보드 세로 크기
    public int BoardWidth => boardWidth;
    public int BoardHeight => boardHeight;
    public Vector3 BoardCenter
    {
        get
        {
            float centerX = (boardWidth - 1) * 0.5f;
            float centerY = (boardHeight - 1) * 0.5f;
            return new Vector3(centerX, centerY, 0);
        }
    }
    
    [Header("셀 프리팹")]
    [SerializeField] private GameObject boardCellPrefab; // BoardCell 프리팹
    
    [Header("셀 색상")]
    [SerializeField] private Color lightCellColor = Color.white; // 밝은 셀 색상
    [SerializeField] private Color darkCellColor = Color.gray;   // 어두운 셀 색상
    
    [Header("보드 셀")]
    private BoardCell[,] boardCells; // 보드의 셀들을 저장하는 2차원 배열
    private Vector2Int selectedCellPosition = new Vector2Int(-1, -1); // 선택된 셀의 위치 (-1, -1은 선택되지 않음을 의미)
    public BoardCell SelectedCell 
    { 
        get 
        {
            if (boardCells == null || boardCells.Length == 0) return null;
            if (selectedCellPosition.x < 0 || selectedCellPosition.y < 0) return null;
            if (selectedCellPosition.x >= boardCells.GetLength(0) || selectedCellPosition.y >= boardCells.GetLength(1)) return null;

            return boardCells[selectedCellPosition.x, selectedCellPosition.y];
        }
    }
            
    void Start()
    {
        CreateBoard();
    }
    
    /// <summary>
    /// N * M 개의 셀로 구성된 보드를 생성합니다.
    /// </summary>
    private void CreateBoard()
    {
        boardCells = new BoardCell[boardWidth, boardHeight];
        
        for (int x = 0; x < boardWidth; x++)
        {
            for (int y = 0; y < boardHeight; y++)
            {
                // 셀 위치 계산
                Vector3 cellPosition = new Vector3(x, y, 0);
                
                // BoardCell 프리팹을 스폰
                GameObject cellObject = Instantiate(boardCellPrefab, cellPosition, Quaternion.identity);
                cellObject.transform.SetParent(boardRoot.transform);
                cellObject.name = $"Cell_{x}_{y}";
                
                // BoardCell 컴포넌트 가져오기
                BoardCell boardCell = cellObject.GetComponent<BoardCell>();
                
                // 체스판 패턴으로 색상 설정 (x + y가 짝수면 어두운 색, 홀수면 밝은 색)
                Color cellColor = ((x + y) % 2 == 0) ? darkCellColor : lightCellColor;
                boardCell.SetColor(cellColor);
                boardCell.ToggleMoveIndicator(false);                

                // 보드 배열에 저장
                boardCells[x, y] = boardCell;
                boardCell.CellCoordinate = new Vector2Int(x, y);
            }
        }
        
        Debug.Log($"보드가 생성되었습니다. 크기: {boardWidth} x {boardHeight}");
        Debug.Log($"보드 중앙 위치: {BoardCenter}");
    }

    public bool IsValidCellCoordinate(Vector2Int cellCoordinate)
    {
        return cellCoordinate.x >= 0 && cellCoordinate.x < boardWidth && cellCoordinate.y >= 0 && cellCoordinate.y < boardHeight;
    }

    public BoardCell GetCell(Vector2Int cellCoordinate)
    {
        if(!IsValidCellCoordinate(cellCoordinate))
        {
            Debug.LogError($"Invalid cell coordinate: {cellCoordinate}");
            return null;
        }

        return boardCells[cellCoordinate.x, cellCoordinate.y];
    }

    public void ActiveMoveIndicator(List<Vector2Int> cells)
    {
        foreach(var cell in boardCells)
        {
            cell.ToggleMoveIndicator(false);
        }

        if(cells == null) return;
        foreach(var cell in cells)
        {
            boardCells[cell.x, cell.y].ToggleMoveIndicator(true);
        }
    }

    /// <summary>
    /// 특정 피스가 이동 가능한 모든 셀 좌표를 반환합니다.
    /// </summary>
    public List<Vector2Int> GetMovableCells(DeployedPiece piece)
    {
        HashSet<Vector2Int> movableCells = new HashSet<Vector2Int>();
        if (piece == null) return new List<Vector2Int>(movableCells);

        Vector2Int start = piece.CellCoordinate;
        PieceMovement move = piece.PieceInfo.movement;

        // 상, 하, 좌, 우 4방향
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        foreach (var dir in directions)
        {
            for (int dist = 1; dist <= move.rookMove; dist++)
            {
                Vector2Int next = start + dir * dist;
                if (!IsValidCellCoordinate(next)) break;
                if (PieceManager.Instance.TryGetPieceAt(next, out _))
                    break; // 막히면 그 뒤로는 못 감
                movableCells.Add(next);
            }
        }

        // 2. 대각선 이동 (diag)
        Vector2Int[] diagDirs = { new Vector2Int(1, 1), new Vector2Int(-1, 1), new Vector2Int(1, -1), new Vector2Int(-1, -1) };
        foreach (var dir in diagDirs)
        {
            for (int dist = 1; dist <= move.bishopMove; dist++)
            {
                Vector2Int next = start + dir * dist;
                if (!IsValidCellCoordinate(next)) break;
                if (PieceManager.Instance.TryGetPieceAt(next, out _))
                    break; // 막히면 그 뒤로는 못 감
                movableCells.Add(next);
            }
        }

        // 3. 나이트 이동
        if (move.isKnight)
        {
            Vector2Int[] knightMoves = {
                new Vector2Int(1,2), new Vector2Int(2,1), new Vector2Int(-1,2), new Vector2Int(-2,1),
                new Vector2Int(1,-2), new Vector2Int(2,-1), new Vector2Int(-1,-2), new Vector2Int(-2,-1)
            };
            foreach (var km in knightMoves)
            {
                Vector2Int next = start + km;
                if (!IsValidCellCoordinate(next)) continue;
                if (PieceManager.Instance.TryGetPieceAt(next, out _))
                    continue;
                movableCells.Add(next);
            }
        }

        // 4. 폰 이동
        if (move.isPawn)
        {
            int forward = piece.PieceColor == PieceColor.White ? 1 : -1;

            // 1. 앞으로 한 칸
            Vector2Int forwardPos = start + new Vector2Int(0, forward);
            if (IsValidCellCoordinate(forwardPos) && !PieceManager.Instance.TryGetPieceAt(forwardPos, out _))
            {
                movableCells.Add(forwardPos);

                // 2. 처음 이동 시 두 칸
                bool isFirstMove = piece.MoveCount == 0;
                Vector2Int doubleForwardPos = start + new Vector2Int(0, 2 * forward);
                if (isFirstMove && IsValidCellCoordinate(doubleForwardPos) && !PieceManager.Instance.TryGetPieceAt(doubleForwardPos, out _))
                {
                    movableCells.Add(doubleForwardPos);
                }
            }

            // 3. 대각선 공격
            Vector2Int[] attackDirs = { new Vector2Int(-1, forward), new Vector2Int(1, forward) };
            foreach (var dir in attackDirs)
            {
                Vector2Int attackPos = start + dir;
                if (IsValidCellCoordinate(attackPos) && PieceManager.Instance.TryGetPieceAt(attackPos, out DeployedPiece target))
                {
                    // 상대 기물인지 체크
                    if(piece.PieceColor == target.PieceColor) continue;
                    movableCells.Add(attackPos);
                }
            }
        }

        return new List<Vector2Int>(movableCells);
    }

    void OnDrawGizmos()
    {
        if (boardCells == null) return;
        
        Gizmos.color = Color.red;
        
        // 각 셀의 좌표를 표기
        for (int x = 0; x < boardWidth; x++)
        {
            for (int y = 0; y < boardHeight; y++)
            {
                if (boardCells[x, y] != null)
                {
                    Vector3 cellPosition = boardCells[x, y].transform.position;
                    string cellCoordinate = boardCells[x, y].CellCoordinate.ToString();
                    
                    // 체스 표기법으로 변환 (좌측 하단이 a1)
                    char file = (char)('a' + x);  // a, b, c, d, e, f, g, h
                    int rank = y + 1;             // 1, 2, 3, 4, 5, 6, 7, 8
                    string chessNotation = $"{file}{rank}";
                    
                    // 좌표값 텍스트 표기 (Unity 좌표 + 체스 표기법)
                    GUIStyle style = new GUIStyle();
                    style.alignment = TextAnchor.MiddleCenter;
                    style.fontSize = 18;
                    style.normal.textColor = Color.magenta;
                    
                    #if UNITY_EDITOR
                    // Unity 좌표계 표시
                    UnityEditor.Handles.Label(cellPosition + Vector3.up * 0.3f, cellCoordinate, style);
                    
                    // 체스 표기법 표시
                    style.normal.textColor = Color.cyan;
                    UnityEditor.Handles.Label(cellPosition + Vector3.down * 0.3f, chessNotation, style);
                    #endif
                }
            }
        }
    }
}
