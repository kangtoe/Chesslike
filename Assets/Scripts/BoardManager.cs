using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;

public class BoardManager : MonoSingleton<BoardManager>
{   
    [Header("Board")]
    public Transform gridParent; // GridLayoutGroup이 붙은 오브젝트    
    public Vector2Int cellCount;    
    [SerializeField] float padding = 0f;     
    
    public Bounds BoardArea 
    { 
        get 
        {
            var bounds = gridParent.GetComponent<SpriteRenderer>().bounds;
            bounds.Expand(padding);
            return bounds;
        }
    }    
    public float CellSize => BoardArea.size.x / cellCount.x;    

    public Piece[,] deployedPieces; // 크기: [rows, columns]
    
    // 셀 좌표 정보를 저장할 전역 변수
    Vector3[,] cellPositions; // 각 셀의 월드 좌표
    Vector3 cellBoardCenter; // 보드의 중심점

    [Header("Piece Movement")]    
    public MovementIndicator movementIndicatorPrefab;
    List<MovementIndicator> movementIndicators = new List<MovementIndicator>();

    Vector2Int selectedCell = new Vector2Int(-1, -1); // -1, -1은 선택되지 않은 상태

    #region Unity Lifecycle
    
    void OnValidate()
    {
        ClearBoard();
    }

    public void Start()
    {
        ClearBoard();
    }

    void Update()
    {
        HandleInput();
    }

    #endregion

    #region Input Handling

    void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var mousePosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
            
            if (IsPositionInBoardArea(mousePosition))
            {
                SelectCell(GetCellPosition(mousePosition));
            }
            else
            {
                DeselectCell();
                Debug.Log("Mouse position is outside board area");
            }                        
        }
    }

    bool IsPositionInBoardArea(Vector3 worldPosition)
    {
        return worldPosition.x >= BoardArea.min.x && worldPosition.x <= BoardArea.max.x &&
               worldPosition.y >= BoardArea.min.y && worldPosition.y <= BoardArea.max.y;
    }

    // 마우스 위치를 셀 좌표로 변환하는 메서드
    Vector2Int GetCellPosition(Vector3 worldPosition)
    {
        // 보드 중심을 기준으로 한 상대 위치 계산
        Vector3 relativePosition = worldPosition - BoardArea.center;
        
        // 셀 크기로 나누어 셀 인덱스 계산
        int x = Mathf.FloorToInt(relativePosition.x / CellSize + cellCount.y * 0.5f);
        int y = Mathf.FloorToInt(relativePosition.y / CellSize + cellCount.x * 0.5f);
        
        // 보드 범위 내에 있는지 확인
        x = Mathf.Clamp(x, 0, cellCount.y - 1);
        y = Mathf.Clamp(y, 0, cellCount.x - 1);
        
        return new Vector2Int(x, y);
    }

    #endregion

    #region Board Management

    [Button("Clear Board")]
    void ClearBoard()
    {                
        CalculateCellPositions();
        ClearDeployedPieces();
        deployedPieces = new Piece[cellCount.x, cellCount.y];
    }

    void CalculateCellPositions()
    {
        float cellSize = CellSize;
        cellBoardCenter = new Vector3((cellCount.y - 1) * cellSize * 0.5f, (cellCount.x - 1) * cellSize * 0.5f, 0);
        
        cellPositions = new Vector3[cellCount.x, cellCount.y];
        
        for (int y = 0; y < cellCount.x; y++)
        {
            for (int x = 0; x < cellCount.y; x++)
            {
                Vector3 position = new Vector3(x * cellSize, y * cellSize, 0) - cellBoardCenter;
                cellPositions[y, x] = position;
            }
        }
    }

    void ClearDeployedPieces()
    {
        if (deployedPieces != null)
        {
            for (int i = 0; i < deployedPieces.GetLength(0); i++)
            {
                for (int j = 0; j < deployedPieces.GetLength(1); j++)
                {
                    if (deployedPieces[i, j] != null)
                    {
                        DestroyImmediate(deployedPieces[i, j].gameObject);
                        deployedPieces[i, j] = null;
                    }
                }
            }
        }
    }

    #endregion

    #region Piece Management

    [Header("Debug")]
    [SerializeField] Piece testPiece;
    [SerializeField] Vector2Int testPosition;
    
    [Button("Deploy Piece")]
    public void DeployPiece()
    {
        DeployPiece(testPiece, testPosition);
    }

    public void DeployPiece(Piece piece, Vector2Int position)
    {
        if (!IsValidPosition(position))
        {
            Debug.LogError($"Invalid position: {position}");
            return;
        }

        if (deployedPieces[position.y, position.x] != null)
        {
            Debug.LogError($"Piece already deployed at {position}");
            return;
        }

        Debug.Log($"Deploying piece {piece.name} at {position}");

        var pieceObject = Instantiate(piece, cellPositions[position.y, position.x] - Vector3.forward, Quaternion.identity);
        pieceObject.transform.SetParent(gridParent);
        pieceObject.currentPosition = position;
        deployedPieces[position.y, position.x] = pieceObject;
    }

    #endregion

    #region Cell Selection

    void SelectCell(Vector2Int cell)
    {
        if(selectedCell == cell)
        {
            DeselectCell();
            return;
        }

        selectedCell = cell;
        var piece = deployedPieces[cell.y, cell.x];
        TogglePieceMovement(true, piece);

        Debug.Log($"Cell position: {selectedCell}");
    }

    void DeselectCell()
    {        
        TogglePieceMovement(false);
        selectedCell = new Vector2Int(-1, -1);        
    }

    void TogglePieceMovement(bool isShow, Piece piece = null)
    {        
        if (isShow && piece != null)
        {
            var positions = GetPieceMovementPositions(piece);
            foreach (var position in positions)
            {
                var indicator = Instantiate(movementIndicatorPrefab, cellPositions[position.y, position.x] - Vector3.forward, Quaternion.identity);
                movementIndicators.Add(indicator);
            }
        }
        else
        {
            ClearMovementIndicators();
        }
    }

    void ClearMovementIndicators()
    {
        foreach (var indicator in movementIndicators)
        {
            if (indicator != null)
                Destroy(indicator.gameObject);
        }
        movementIndicators.Clear();
    }

    #endregion

    #region Movement Logic

    public List<Vector2Int> GetPieceMovementPositions(Piece piece)
    {
        var pieceMovement = piece.PieceMovement;
        var currentPosition = piece.currentPosition;

        List<Vector2Int> positions = new List<Vector2Int>();

        // 가로 이동 (좌/우)
        AddMovementInDirection(positions, currentPosition, new Vector2Int(1, 0), pieceMovement.row);
        AddMovementInDirection(positions, currentPosition, new Vector2Int(-1, 0), pieceMovement.row);

        // 세로 이동 (상/하)
        AddMovementInDirection(positions, currentPosition, new Vector2Int(0, 1), pieceMovement.col);
        AddMovementInDirection(positions, currentPosition, new Vector2Int(0, -1), pieceMovement.col);

        // 대각선 이동 (4방향)
        AddMovementInDirection(positions, currentPosition, new Vector2Int(1, 1), pieceMovement.diag);
        AddMovementInDirection(positions, currentPosition, new Vector2Int(1, -1), pieceMovement.diag);
        AddMovementInDirection(positions, currentPosition, new Vector2Int(-1, 1), pieceMovement.diag);
        AddMovementInDirection(positions, currentPosition, new Vector2Int(-1, -1), pieceMovement.diag);

        return positions;
    }   

    // 특정 방향으로 이동 가능한 위치들을 추가하는 공통 메서드
    void AddMovementInDirection(List<Vector2Int> positions, Vector2Int startPos, Vector2Int direction, int maxDistance)
    {
        for (int i = 1; i <= maxDistance; i++)
        {
            Vector2Int targetPos = startPos + direction * i;
            
            // 보드 경계 체크
            if (!IsValidPosition(targetPos)) break;
            
            // 다른 기물이 있는지 체크
            if (deployedPieces[targetPos.y, targetPos.x] != null) break;
            
            positions.Add(targetPos);
        }
    }

    // 보드 경계 내의 유효한 위치인지 확인하는 헬퍼 메서드
    bool IsValidPosition(Vector2Int position)
    {
        return position.x >= 0 && position.x < cellCount.y && 
               position.y >= 0 && position.y < cellCount.x;
    }

    #endregion

    #region Debug & Gizmos

    void OnDrawGizmosSelected()
    {
        if (cellPositions == null) return;
        
        float cellSize = CellSize;
        
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(BoardArea.center, BoardArea.size);

        Gizmos.color = Color.red;      
        for (int y = 0; y < cellCount.x; y++)
        {
            for (int x = 0; x < cellCount.y; x++)
            {
                Vector3 position = cellPositions[y, x];
                Gizmos.DrawWireSphere(position, cellSize * 0.5f);
                
                // 좌표값 텍스트 표기
                Vector2Int coord = new Vector2Int(x, y);

                GUIStyle style = new GUIStyle();
                style.alignment = TextAnchor.MiddleCenter;
                style.fontSize = 18;
                style.normal.textColor = Color.magenta;
                
                #if UNITY_EDITOR
                UnityEditor.Handles.Label(position, coord.ToString(), style);
                #endif
            }
        }
    }

    #endregion
}
