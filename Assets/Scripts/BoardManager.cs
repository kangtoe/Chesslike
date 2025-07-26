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

    [Header("셀 하이라이트 색상")]
    [SerializeField] private Color validPlacementColor = Color.green;    // 유효한 배치 위치 색상
    [SerializeField] private Color invalidPlacementColor = Color.red;    // 유효하지 않은 배치 위치 색상
    
    // 셀 하이라이트 상태
    private BoardCell currentHighlightedCell;
    private Color originalHighlightCellColor;
    private bool hasHighlightColorChanged = false;
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

    public void ActiveCellIndicator(List<Vector2Int> movableCells, List<Vector2Int> attackCells = null)
    {
        foreach(var cell in boardCells)
        {
            cell.ToggleMoveIndicator(false);
        }

        if(movableCells != null)
        {
            foreach(var cell in movableCells)
            {
                boardCells[cell.x, cell.y].ToggleMoveIndicator(true);
            }
        }

        if(attackCells != null)
        {
            foreach(var cell in attackCells)
            {
                boardCells[cell.x, cell.y].ToggleMoveIndicator(true, Color.red);
            }
        }
    }

    /// <summary>
    /// 특정 피스가 이동 가능한 모든 셀 좌표를 반환합니다.
    /// movableCells: 빈 칸으로 이동 가능한 위치
    /// attackCells: 적 기물을 공격할 수 있는 위치
    /// </summary>
    public void GetMovableCells(DeployedPiece piece, out List<Vector2Int> movableCells, out List<Vector2Int> attackCells)
    {
        HashSet<Vector2Int> _movableCells = new HashSet<Vector2Int>();
        HashSet<Vector2Int> _attackCells = new HashSet<Vector2Int>();
        if (piece == null)
        {
            movableCells = null;
            attackCells = null;
            return;
        }

        Vector2Int start = piece.CellCoordinate;
        PieceMovement move = piece.PieceInfo.movement;

        // 1. 룩 이동 (상, 하, 좌, 우 4방향) - 무제한 이동
        if (move.isRookMove)
        {
            Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
            foreach (var dir in directions)
            {
                // 보드 크기에 맞게 최대 이동 거리 계산
                int maxDistance = Mathf.Max(boardWidth, boardHeight) - 1;
                for (int dist = 1; dist <= maxDistance; dist++)
                {
                    Vector2Int next = start + dir * dist;
                    if (!IsValidCellCoordinate(next)) break;
                    
                    if (PieceManager.Instance.TryGetPieceAt(next, out DeployedPiece targetPiece))
                    {
                        // 기물이 있는 경우
                        if (targetPiece.PieceColor != piece.PieceColor)
                        {
                            // 적 기물이면 공격 가능
                            _attackCells.Add(next);
                        }
                        break; // 기물이 있으면 그 뒤로는 못 감
                    }
                    else
                    {
                        // 빈 칸이면 이동 가능
                        _movableCells.Add(next);
                    }
                }
            }
        }

        // 2. 비숍 이동 (대각선) - 무제한 이동
        if (move.isBishopMove)
        {
            Vector2Int[] diagDirs = { new Vector2Int(1, 1), new Vector2Int(-1, 1), new Vector2Int(1, -1), new Vector2Int(-1, -1) };
            foreach (var dir in diagDirs)
            {
                // 보드 크기에 맞게 최대 이동 거리 계산
                int maxDistance = Mathf.Min(boardWidth, boardHeight) - 1;
                for (int dist = 1; dist <= maxDistance; dist++)
                {
                    Vector2Int next = start + dir * dist;
                    if (!IsValidCellCoordinate(next)) break;
                    
                    if (PieceManager.Instance.TryGetPieceAt(next, out DeployedPiece targetPiece))
                    {
                        // 기물이 있는 경우
                        if (targetPiece.PieceColor != piece.PieceColor)
                        {
                            // 적 기물이면 공격 가능
                            _attackCells.Add(next);
                        }
                        break; // 기물이 있으면 그 뒤로는 못 감
                    }
                    else
                    {
                        // 빈 칸이면 이동 가능
                        _movableCells.Add(next);
                    }
                }
            }
        }

        // 3. 나이트 이동
        if (move.isKnightMove)
        {
            Vector2Int[] knightMoves = {
                new Vector2Int(1,2), new Vector2Int(2,1), new Vector2Int(-1,2), new Vector2Int(-2,1),
                new Vector2Int(1,-2), new Vector2Int(2,-1), new Vector2Int(-1,-2), new Vector2Int(-2,-1)
            };
            foreach (var km in knightMoves)
            {
                Vector2Int next = start + km;
                if (!IsValidCellCoordinate(next)) continue;
                
                if (PieceManager.Instance.TryGetPieceAt(next, out DeployedPiece targetPiece))
                {
                    // 기물이 있는 경우
                    if (targetPiece.PieceColor != piece.PieceColor)
                    {
                        // 적 기물이면 공격 가능
                        _attackCells.Add(next);
                    }
                    // 아군 기물이면 이동 불가 (continue)
                }
                else
                {
                    // 빈 칸이면 이동 가능
                    _movableCells.Add(next);
                }
            }
        }

        // 4. 폰 이동
        if (move.isPawnMove)
        {
            int forward = piece.PieceColor == PieceColor.White ? 1 : -1;

            // 1. 앞으로 한 칸 이동 (공격 X, 이동만)
            Vector2Int forwardPos = start + new Vector2Int(0, forward);
            if (IsValidCellCoordinate(forwardPos) && !PieceManager.Instance.TryGetPieceAt(forwardPos, out _))
            {
                _movableCells.Add(forwardPos);

                // 2. 처음 이동 시 두 칸 이동 (공격 X, 이동만)
                bool isFirstMove = piece.MoveCount == 0;
                Vector2Int doubleForwardPos = start + new Vector2Int(0, 2 * forward);
                if (isFirstMove && IsValidCellCoordinate(doubleForwardPos) && !PieceManager.Instance.TryGetPieceAt(doubleForwardPos, out _))
                {
                    _movableCells.Add(doubleForwardPos);
                }
            }

            // 3. 대각선 공격 (이동 X, 공격만)
            Vector2Int[] attackDirs = { new Vector2Int(-1, forward), new Vector2Int(1, forward) };
            foreach (var dir in attackDirs)
            {
                Vector2Int attackPos = start + dir;
                if (IsValidCellCoordinate(attackPos) && PieceManager.Instance.TryGetPieceAt(attackPos, out DeployedPiece target))
                {
                    // 상대 기물인지 체크
                    if(piece.PieceColor != target.PieceColor)
                    {
                        _attackCells.Add(attackPos);
                    }
                }
            }
        }

        // 5. 킹 이동
        if (move.isKingMove)
        {
            // 킹은 모든 방향으로 1칸씩만 이동
            Vector2Int[] kingMoves = {
                Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right,
                new Vector2Int(1,1), new Vector2Int(1,-1), new Vector2Int(-1,1), new Vector2Int(-1,-1)
            };
            
            // 킹의 후보 이동 위치들을 먼저 계산
            List<Vector2Int> candidateMovableCells = new List<Vector2Int>();
            List<Vector2Int> candidateAttackCells = new List<Vector2Int>();
            
            foreach (var dir in kingMoves)
            {
                Vector2Int next = start + dir;
                if (!IsValidCellCoordinate(next)) continue;
                
                if (PieceManager.Instance.TryGetPieceAt(next, out DeployedPiece targetPiece))
                {
                    // 기물이 있는 경우
                    if (targetPiece.PieceColor != piece.PieceColor)
                    {
                        // 적 기물이면 공격 가능 (후보)
                        candidateAttackCells.Add(next);
                    }
                    // 아군 기물이면 이동 불가 (continue)
                }
                else
                {
                    // 빈 칸이면 이동 가능 (후보)
                    candidateMovableCells.Add(next);
                }
            }
            
            // 각 후보 위치에 대해 안전한지 확인 (GameStateValidator 활용)
            foreach (var candidatePos in candidateMovableCells)
            {
                if (GameStateValidator.Instance.IsKingMoveToPositionSafe(piece, candidatePos))
                {
                    _movableCells.Add(candidatePos);
                }
            }
            
            foreach (var candidatePos in candidateAttackCells)
            {
                if (GameStateValidator.Instance.IsKingMoveToPositionSafe(piece, candidatePos))
                {
                    _attackCells.Add(candidatePos);
                }
            }
        }

        movableCells = new List<Vector2Int>(_movableCells);
        attackCells = new List<Vector2Int>(_attackCells);
    }

    #region 셀 하이라이트 기능

    /// <summary>
    /// 스크린 위치에 따른 셀 하이라이트 업데이트
    /// </summary>
    /// <param name="screenPosition">스크린 위치</param>
    /// <param name="pieceInfo">배치할 기물 정보 (유효성 검사용)</param>
    /// <param name="pieceColor">기물 색상 (유효성 검사용)</param>
    public void UpdateCellHighlight(Vector2 screenPosition, PieceInfo pieceInfo = null, PieceColor pieceColor = PieceColor.White)
    {
        Vector2Int? targetCell = InputUtil.GetBoardCellFromScreenPosition(screenPosition);
        
        if (targetCell.HasValue)
        {
            BoardCell cell = GetCell(targetCell.Value);
            
            // 이전에 하이라이트된 셀과 다른 셀인 경우
            if (currentHighlightedCell != cell)
            {
                // 이전 셀의 색상 복원
                ClearCellHighlight();
                
                // 새 셀 하이라이트
                if (cell != null)
                {
                    HighlightCell(cell, targetCell.Value);
                }
            }
        }
        else
        {
            // 마우스가 보드 밖에 있으면 하이라이트 제거
            ClearCellHighlight();
        }
    }

    /// <summary>
    /// 특정 셀을 하이라이트
    /// </summary>
    /// <param name="cell">하이라이트할 셀</param>
    /// <param name="cellPosition">셀 위치 (유효성 검사용)</param>
    private void HighlightCell(BoardCell cell, Vector2Int cellPosition)
    {
        currentHighlightedCell = cell;
        originalHighlightCellColor = cell.CellRenderer.material.color;
        hasHighlightColorChanged = true;
        
        // 유효한 배치 위치인지 확인
        bool isValidPosition = PieceManager.Instance.IsValidPlacementPosition(cellPosition);
        Color highlightColor = isValidPosition ? validPlacementColor : invalidPlacementColor;
        
        cell.SetColor(highlightColor);
        
        Debug.Log($"셀 하이라이트: {cellPosition} - {(isValidPosition ? "유효" : "무효")}");
    }

    /// <summary>
    /// 현재 하이라이트 제거 및 원래 색상 복원
    /// </summary>
    public void ClearCellHighlight()
    {
        if (currentHighlightedCell != null && hasHighlightColorChanged)
        {
            currentHighlightedCell.SetColor(originalHighlightCellColor);
            currentHighlightedCell = null;
            hasHighlightColorChanged = false;
        }
    }

    #endregion

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
