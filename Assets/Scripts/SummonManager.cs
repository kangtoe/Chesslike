using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.EventSystems; // PointerEventData 사용을 위해 추가

public class SummonManager : MonoSingleton<SummonManager>
{
    [Header("Summon Piece UI")]
    [SerializeField] PieceInfo[] blackSummonPieceInfos;
    [SerializeField] PieceInfo[] whiteSummonPieceInfos;    
    
    [Header("Summon Piece UI Parent")]
    [SerializeField] Transform blackSummonPieceParent;
    [SerializeField] Transform whiteSummonPieceParent;

    [Header("Summon Piece UI Prefab")]
    [SerializeField] SummonPieceUI summonPiecePrefab;

    [Header("선택된 기물 정보")]
    [NaughtyAttributes.ReadOnly]
    [SerializeField] PieceInfo selectedPieceInfo;
    [NaughtyAttributes.ReadOnly]
    [SerializeField] PieceColor selectedPieceColor;
    [NaughtyAttributes.ReadOnly]
    [SerializeField] SummonPieceUI selectedPieceUI;

    [Header("드래그 상태")]
    [NaughtyAttributes.ReadOnly]
    [SerializeField] bool isDragging = false;
    public bool IsDragging => isDragging;

    [Header("드래그 설정")]
    [SerializeField] float returnDuration = 0.5f; // UI가 원래 위치로 돌아가는 시간
    
    [Header("선택 상태 설정")]
    [SerializeField] Color selectedColor = Color.yellow; // 선택됐을 때 색상
    [SerializeField] float selectedScale = 1.1f; // 선택됐을 때 크기
    
    // 드래그 설정 프로퍼티
    public float ReturnDuration => returnDuration;
    
    // 선택 상태 설정 프로퍼티
    public Color SelectedColor => selectedColor;
    public float SelectedScale => selectedScale;

    // 기물 선택 상태 프로퍼티
    public bool HasSelectedPiece => selectedPieceInfo != null;
    public PieceInfo SelectedPieceInfo => selectedPieceInfo;
    public PieceColor SelectedPieceColor => selectedPieceColor;
    public SummonPieceUI SelectedPieceUI => selectedPieceUI;

    void Start()
    {
        foreach (var pieceInfo in whiteSummonPieceInfos)
        {
            SummonPieceUI summonPiece = Instantiate(summonPiecePrefab, whiteSummonPieceParent);
            summonPiece.SetPieceInfo(pieceInfo, PieceColor.White);
        }
        foreach (var pieceInfo in blackSummonPieceInfos)
        {
            SummonPieceUI summonPiece = Instantiate(summonPiecePrefab, blackSummonPieceParent);
            summonPiece.SetPieceInfo(pieceInfo, PieceColor.Black);
        }
    }

    void Update()
    {
        // 소환 모드이고 드래그 중이 아닐 때 마우스 위치에 따른 셀 하이라이트
        if (HasSelectedPiece && !isDragging)
        {
            // 마우스 위치에서 셀 하이라이트 업데이트
            Vector2 mousePosition = Input.mousePosition;
            BoardManager.Instance.UpdateCellHighlight(mousePosition, selectedPieceInfo, selectedPieceColor);
        }
    }

    /// <summary>
    /// 드래그 상태 설정
    /// </summary>
    /// <param name="dragging">드래그 여부</param>
    public void SetDragging(bool dragging)
    {
        isDragging = dragging;
    }

    /// <summary>
    /// 기물 선택 (클릭 방식용)
    /// </summary>
    /// <param name="pieceInfo">선택할 기물 정보</param>
    /// <param name="pieceColor">기물 색상</param>
    /// <param name="pieceUI">선택된 UI 컴포넌트</param>
    public void SelectPieceForSummon(PieceInfo pieceInfo, PieceColor pieceColor, SummonPieceUI pieceUI)
    {
        // 이미 같은 기물이 선택되어 있으면 선택 해제
        if (selectedPieceInfo == pieceInfo && selectedPieceColor == pieceColor && selectedPieceUI == pieceUI)
        {
            DeselectPieceForSummon();
            return;
        }

        // 이전 선택 해제
        if (selectedPieceUI != null)
        {
            selectedPieceUI.SetSelected(false);
        }

        // 새로운 기물 선택
        selectedPieceInfo = pieceInfo;
        selectedPieceColor = pieceColor;
        selectedPieceUI = pieceUI;
        
        if (selectedPieceUI != null)
        {
            selectedPieceUI.SetSelected(true);
        }

        Debug.Log($"소환용 기물 선택: {pieceInfo.pieceName} ({pieceColor})");
    }

    /// <summary>
    /// 기물 선택 해제
    /// </summary>
    public void DeselectPieceForSummon()
    {
        if (selectedPieceUI != null)
        {
            selectedPieceUI.SetSelected(false);
        }

        selectedPieceInfo = null;
        selectedPieceColor = PieceColor.White;
        selectedPieceUI = null;

        // 소환 모드 해제 시 셀 하이라이트도 제거
        BoardManager.Instance.ClearCellHighlight();

        Debug.Log("소환용 기물 선택 해제");
    }



    /// <summary>
    /// 우클릭 처리 (소환 모드 해제)
    /// </summary>
    public void OnRightClick()
    {
        if (HasSelectedPiece)
        {
            DeselectPieceForSummon();
            Debug.Log("우클릭으로 소환 모드 해제");
        }
    }

    /// <summary>
    /// UI 클릭 처리 (기물 선택/선택해제)
    /// </summary>
    /// <param name="pieceInfo">클릭된 기물 정보</param>
    /// <param name="pieceColor">기물 색상</param>
    /// <param name="pieceUI">클릭된 UI</param>
    public void OnUIClick(PieceInfo pieceInfo, PieceColor pieceColor, SummonPieceUI pieceUI)
    {
        if (isDragging) return; // 드래그 중이면 클릭으로 처리하지 않음
        
        // UI 클릭 - 기물 선택
        SelectPieceForSummon(pieceInfo, pieceColor, pieceUI);
    }

    /// <summary>
    /// 보드 클릭 처리 (소환 시도)
    /// </summary>
    /// <param name="targetPosition">보드 위치</param>
    public void OnBoardClick(Vector2Int targetPosition)
    {
        if (isDragging) return; // 드래그 중이면 클릭으로 처리하지 않음
        
        // 보드 클릭 - 소환 시도
        TrySummonSelectedPiece(targetPosition);
    }

    /// <summary>
    /// 클릭 이벤트 처리 (좌클릭/우클릭)
    /// </summary>
    /// <param name="eventData">이벤트 데이터</param>
    public void OnPointerClick(PointerEventData eventData)
    {
        // eventData에서 SummonPieceUI 컴포넌트 추출
        var pieceUI = eventData.pointerPress?.GetComponent<SummonPieceUI>();
        if (pieceUI == null || pieceUI.PieceInfo == null) return;
        
        // 드래그 중이었다면 클릭으로 처리하지 않음
        if (isDragging) return;
        
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            // 좌클릭: 기물 선택/선택해제
            Debug.Log($"좌클릭: {pieceUI.PieceInfo.pieceName} ({pieceUI.PieceColor})");
            OnUIClick(pieceUI.PieceInfo, pieceUI.PieceColor, pieceUI);
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            // 우클릭: 소환 모드 해제
            Debug.Log($"UI 우클릭: 소환 모드 해제");
            OnRightClick();
        }
    }

    /// <summary>
    /// 드래그 시작 처리 (클릭으로 소환 모드 진입과 동일)
    /// </summary>
    /// <param name="eventData">이벤트 데이터</param>
    public void OnBeginDrag(PointerEventData eventData)
    {
        // eventData에서 SummonPieceUI 컴포넌트 추출
        var pieceUI = eventData.pointerDrag?.GetComponent<SummonPieceUI>();
        if (pieceUI == null || pieceUI.PieceInfo == null) return;
        
        var pieceInfo = pieceUI.PieceInfo;
        var pieceColor = pieceUI.PieceColor;
        
        // 클릭으로 소환 모드 진입과 완전히 동일한 처리
        OnUIClick(pieceInfo, pieceColor, pieceUI);
        
        // 드래그 상태 설정 (Update에서 하이라이트를 멈추기 위함)
        SetDragging(true);
        
        // UI 드래그 정보 초기화 (UI 위치 변경용)
        pieceUI.InitializeDrag(eventData);
        
        Debug.Log($"드래그 시작 (소환 모드 진입과 동일): {pieceInfo.pieceName}");
    }

    /// <summary>
    /// 드래그 중 처리 (소환 모드 중 마우스 움직임과 동일)
    /// </summary>
    /// <param name="eventData">이벤트 데이터</param>
    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging || selectedPieceUI == null) return;
        
        // UI 위치 업데이트 (드래그만의 특별한 기능)
        selectedPieceUI.UpdateDragPosition(eventData);
        
        // 소환 모드 중 마우스 움직임과 완전히 동일한 처리
        if (selectedPieceInfo != null)
        {
            BoardManager.Instance.UpdateCellHighlight(eventData.position, selectedPieceInfo, selectedPieceColor);
        }
    }

    /// <summary>
    /// 드래그 종료 처리 (소환 모드에서 보드 클릭과 동일)
    /// </summary>
    /// <param name="eventData">이벤트 데이터</param>
    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging || selectedPieceUI == null) return;
        
        var uiToProcess = selectedPieceUI;
        
        // 드래그 상태 해제 (Update 하이라이트 재개)
        SetDragging(false);
        
        // 마우스 위치에서 보드 셀 확인
        Vector2Int? targetCell = InputUtil.GetBoardCellFromScreenPosition(eventData.position);
        
        if (targetCell.HasValue)
        {
            // 소환 모드에서 보드 클릭과 완전히 동일한 처리
            OnBoardClick(targetCell.Value);
            
            // 소환 성공했다면 UI 삭제, 실패했다면 선택 상태 유지됨
            if (!HasSelectedPiece)
            {
                // 성공 시 UI 삭제
                uiToProcess.DestroySelf();
            }
            else
            {
                // 실패 시 원래 위치로 복귀
                uiToProcess.ReturnToOriginalPosition();
            }
        }
        else
        {
            // 보드 밖 - 선택 취소 (우클릭과 동일한 동작)
            DeselectPieceForSummon();
            uiToProcess.ReturnToOriginalPosition();
        }
    }

    /// <summary>
    /// 기본 소환 로직 - 모든 소환 방식의 핵심 로직
    /// </summary>
    /// <param name="pieceInfo">소환할 기물 정보</param>
    /// <param name="targetPosition">목표 위치</param>
    /// <param name="pieceColor">기물 색상</param>
    /// <returns>소환 성공 여부</returns>
    public bool TrySummonPiece(PieceInfo pieceInfo, Vector2Int targetPosition, PieceColor pieceColor)
    {
        if (pieceInfo == null) 
        {
            Debug.LogWarning("소환할 기물 정보가 없습니다.");
            return false;
        }

        // 현재 턴 플레이어의 기물인지 확인
        if (!TurnManager.Instance.IsPlayerTurn) return false;

        // 유효성 검사
        if (!PieceManager.Instance.IsValidPlacementPosition(targetPosition))
        {
            Debug.Log($"유효하지 않은 소환 위치: {targetPosition}");
            return false;
        }

        // 추가 소환 제한 로직이 있다면 여기에 구현
        // 예: 자원 소모, 쿨다운, 최대 소환 수 제한 등

        // 기물 배치
        bool success = PieceManager.Instance.DeployPiece(pieceInfo, targetPosition, pieceColor);
        
        return success;
    }

    /// <summary>
    /// 선택된 기물을 지정된 위치에 소환 시도 (클릭 방식)
    /// 성공 시 선택 정보를 초기화하고 UI 삭제
    /// </summary>
    /// <param name="targetPosition">목표 위치</param>
    /// <returns>소환 성공 여부</returns>
    public bool TrySummonSelectedPiece(Vector2Int targetPosition)
    {
        if (!HasSelectedPiece)
        {
            Debug.Log("선택된 기물이 없습니다.");
            return false;
        }

        // 삭제할 UI 미리 저장
        var uiToDestroy = selectedPieceUI;
        
        bool success = TrySummonPiece(selectedPieceInfo, targetPosition, selectedPieceColor);
        
        if (success)
        {
            // 성공 시 선택 해제 (하이라이트도 함께 제거됨)
            DeselectPieceForSummon();
            
            // UI 직접 삭제
            uiToDestroy.DestroySelf();
        }

        return success;
    }


}
