using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PieceManager : MonoSingleton<PieceManager>
{
    [SerializeField] GameObject _pieceRoot;

    [Header("피스 프리팹")]
    [SerializeField] DeployedPiece _piecePrefab;

    [Header("선택된 피스")]
    [NaughtyAttributes.ReadOnly]
    [SerializeField] DeployedPiece _selectedPiece;
    public DeployedPiece SelectedPiece => _selectedPiece;

    [Header("이동 연출")]
    [SerializeField] float moveSpeed = 6f; // 이동 속도
    [SerializeField] float liftHeight = 1f; // 들어올리는 높이
    [SerializeField] float pauseTime = 0.15f; // 내려치기 전 대기 시간
    [SerializeField] float minMoveTime = 0.3f; // 최소 이동 시간
    [SerializeField] float dropTime = 0.1f; // 내려치기 시간 (고정)
    
    [NaughtyAttributes.ReadOnly]
    [SerializeField] bool _isMoving = false;
    public bool IsMoving => _isMoving;    

    // 키: 셀 좌표, 값: 배치된 피스
    Dictionary<Vector2Int, DeployedPiece> deployedPieces = new Dictionary<Vector2Int, DeployedPiece>();
    public Dictionary<Vector2Int, DeployedPiece> DeployedPieces => deployedPieces;

    bool IsValidCellCoordinate(Vector2Int cellCoordinate) => BoardManager.Instance.IsValidCellCoordinate(cellCoordinate);

    public bool TryGetPieceAt(Vector2Int cellCoordinate, out DeployedPiece piece)
    {
        return deployedPieces.TryGetValue(cellCoordinate, out piece);
    }

    /// <summary>
    /// 해당 위치에 기물을 배치할 수 있는지 확인하는 유틸리티 메서드
    /// </summary>
    /// <param name="cellCoordinate">확인할 셀 좌표</param>
    /// <returns>배치 가능 여부</returns>
    public bool IsValidPlacementPosition(Vector2Int cellCoordinate)
    {
        // 보드 범위 내인지 확인
        if (!IsValidCellCoordinate(cellCoordinate))
        {
            Debug.Log($"보드 범위를 벗어남: {cellCoordinate}");
            return false;
        }
        
        // 이미 기물이 배치되어 있는지 확인
        if (TryGetPieceAt(cellCoordinate, out DeployedPiece existingPiece))
        {
            Debug.Log($"이미 기물이 배치되어 있음: {cellCoordinate}");
            return false;
        }
        
        return true;
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
                MovePiece(piece.CellCoordinate);
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

        if(!IsValidPlacementPosition(cellCoordinate)) return false;

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

        if(_isMoving) return false; // 이동 중이면 불가
        
        StartCoroutine(MovePieceAnimated(_selectedPiece.CellCoordinate, toCoordinate));
        return true;
    }

    /// <summary>
    /// LeanTween을 사용한 드라마틱한 이동 연출: 들어올리기 → 잠깐 멈춤 → 쾅! 내려치기
    /// </summary>
    IEnumerator MovePieceAnimated(Vector2Int fromCoordinate, Vector2Int toCoordinate)
    {
        if(!IsValidCellCoordinate(toCoordinate)) yield break;
        if(!deployedPieces.ContainsKey(fromCoordinate))
        {
            Debug.LogError($"기물이 없습니다: {fromCoordinate}");
            yield break;
        }

        _isMoving = true;        

        DeployedPiece piece = deployedPieces[fromCoordinate];
        BoardCell targetCell = BoardManager.Instance.GetCell(toCoordinate);
        
        Vector3 startPos = piece.transform.position;
        Vector3 endPos = targetCell.PieceDeployPoint;
        Vector3 midPos = endPos + Vector3.back * liftHeight; // 목표 위 공중
        
        // 거리에 따른 이동 시간 계산 + 최소 시간 보장
        float calculatedLiftTime = Vector3.Distance(startPos, midPos) / moveSpeed;
        float liftTime = Mathf.Max(calculatedLiftTime, minMoveTime);
        
        // 1단계: 목표 위로 부드럽게 이동
        bool liftDone = false;
        LeanTween.move(piece.gameObject, midPos, liftTime)
            .setEase(LeanTweenType.easeOutQuad)
            .setOnComplete(() => liftDone = true);
            
        yield return new WaitUntil(() => liftDone);
        
        // 2단계: 잠깐 멈춤
        yield return new WaitForSeconds(pauseTime);
        
        // 3단계: 빠르게  내려치기
        bool dropDone = false;
        LeanTween.move(piece.gameObject, endPos, dropTime)
            .setEase(LeanTweenType.easeInQuad)
            .setOnComplete(() => dropDone = true);
            
        yield return new WaitUntil(() => dropDone);

        // 목표에 기물이 있으면 제거 (공격)
        if(deployedPieces.ContainsKey(toCoordinate)) 
            RemovePiece(toCoordinate);

        // 데이터 업데이트
        piece.SetCellCoordinate(toCoordinate);
        piece.MoveCount++;
        deployedPieces.Remove(fromCoordinate);
        deployedPieces[toCoordinate] = piece;
        
        DeselectPiece();
        _isMoving = false;
        
        Debug.Log($"기물 이동: {fromCoordinate} → {toCoordinate}");
        
        // 이동 완료 후 턴 전환
        GameManager.Instance.NextTurn();
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

    /// <summary>
    /// 특정 색상의 킹을 찾습니다
    /// </summary>
    /// <param name="color">킹의 색상</param>
    /// <returns>킹 기물 (찾지 못하면 null)</returns>
    public DeployedPiece FindKing(PieceColor color)
    {
        foreach (var piece in deployedPieces.Values)
        {
            if (piece.PieceColor == color && piece.PieceInfo.pieceName.ToLower().Contains("king"))
            {
                return piece;
            }
        }

        return null;
    }

}
