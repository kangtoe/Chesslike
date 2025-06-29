using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoSingleton<BoardManager>
{
    [Header("보드 설정")]
    [SerializeField] private int boardWidth = 8;  // 보드 가로 크기
    [SerializeField] private int boardHeight = 8; // 보드 세로 크기
    
    [Header("셀 프리팹")]
    [SerializeField] private GameObject boardCellPrefab; // BoardCell 프리팹
    
    [Header("셀 색상")]
    [SerializeField] private Color lightCellColor = Color.white; // 밝은 셀 색상
    [SerializeField] private Color darkCellColor = Color.gray;   // 어두운 셀 색상
    
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
    
    /// <summary>
    /// 보드의 중앙 위치를 반환합니다.
    /// </summary>
    public Vector3 BoardCenter
    {
        get
        {
            float centerX = (boardWidth - 1) * 0.5f;
            float centerY = (boardHeight - 1) * 0.5f;
            return new Vector3(centerX, centerY, 0);
        }
    }
            
    void Start()
    {
        CreateBoard();
    }

    private void Update()
    {
        CheckInput();
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
                cellObject.transform.SetParent(transform);
                cellObject.name = $"Cell_{x}_{y}";
                
                // BoardCell 컴포넌트 가져오기
                BoardCell boardCell = cellObject.GetComponent<BoardCell>();
                
                // 체스판 패턴으로 색상 설정 (x + y가 짝수면 밝은 색, 홀수면 어두운 색)
                Color cellColor = ((x + y) % 2 == 0) ? lightCellColor : darkCellColor;
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

    void CheckInput()
    {
        if (Input.GetMouseButtonDown(0))
        {            
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                BoardCell cell = hit.collider.GetComponent<BoardCell>();
                if (cell)
                {
                    SelectCell(cell.CellCoordinate.x, cell.CellCoordinate.y);
                    return;
                }
            }

            ClearSelection();
        }
    }
    
    /// <summary>
    /// 지정된 위치의 셀을 선택합니다.
    /// </summary>
    /// <param name="x">X 좌표</param>
    /// <param name="y">Y 좌표</param>
    /// <returns>선택 성공 여부</returns>
    public bool SelectCell(int x, int y)
    {
        // 이전 선택 해제
        ClearSelection();
        
        // 새 셀 선택
        selectedCellPosition = new Vector2Int(x, y);    
        
        Debug.Log($"셀이 선택되었습니다: ({x}, {y})");
        return true;
    }
    
    /// <summary>
    /// 선택을 해제합니다.
    /// </summary>
    public void ClearSelection()
    {
        if (SelectedCell)
        {
            Debug.Log($"셀 선택 해제: {SelectedCell.CellCoordinate}");
            selectedCellPosition = new Vector2Int(-1, -1);
        }
    }

    void OnDrawGizmosSelected()
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
                                        
                    // 좌표값 텍스트 표기
                    GUIStyle style = new GUIStyle();
                    style.alignment = TextAnchor.MiddleCenter;
                    style.fontSize = 18;
                    style.normal.textColor = Color.magenta;
                    
                    #if UNITY_EDITOR
                    UnityEditor.Handles.Label(cellPosition, cellCoordinate, style);
                    #endif
                }
            }
        }
    }
}
