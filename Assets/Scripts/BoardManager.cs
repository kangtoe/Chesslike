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
    
    // Start is called before the first frame update
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
            }
        }
        
        Debug.Log($"보드가 생성되었습니다. 크기: {boardWidth} x {boardHeight}");
        Debug.Log($"보드 중앙 위치: {BoardCenter}");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
