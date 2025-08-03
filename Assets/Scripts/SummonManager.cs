using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.EventSystems; // PointerEventData 사용을 위해 추가

public class SummonManager : MonoSingleton<SummonManager>
{
    [Header("Summon Piece UI")]
    [SerializeField] List<PieceInfo> blackSummonPieceInfos;
    [SerializeField] List<PieceInfo> whiteSummonPieceInfos;
    
    // AI 포켓 초기화를 위한 public 프로퍼티
    public List<PieceInfo> BlackSummonPieceInfos => blackSummonPieceInfos;    
    public List<PieceInfo> WhiteSummonPieceInfos => whiteSummonPieceInfos;    
    
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
    
    [Header("AI 시각적 피드백 설정")]
    [SerializeField] bool showAISelectionProcess = true; // AI 선택 과정을 시각적으로 표시할지 여부
    [SerializeField] float aiSelectionDuration = 0.3f; // AI 기물 선택 표시 시간
    [SerializeField] float aiHighlightDuration = 0.2f; // AI 목표 위치 하이라이트 시간
    
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
        RefreshSummonUI();
    }
    
    /// <summary>
    /// 소환 UI 전체 갱신
    /// </summary>
    private void RefreshSummonUI()
    {
        // 화이트 기물 UI 삭제
        foreach (Transform child in whiteSummonPieceParent)
        {
            Destroy(child.gameObject);
        }
        
        // 블랙 기물 UI 삭제
        foreach (Transform child in blackSummonPieceParent)
        {
            Destroy(child.gameObject);
        }
        
        // 화이트 기물 UI 생성
        foreach (var pieceInfo in whiteSummonPieceInfos)
        {
            SummonPieceUI summonPiece = Instantiate(summonPiecePrefab, whiteSummonPieceParent);
            summonPiece.SetPieceInfo(pieceInfo, PieceColor.White);
        }
        
        // 블랙 기물 UI 생성
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
            
            // 소환 성공했다면 리스트에서 제거되어 UI가 자동 갱신됨, 실패했다면 선택 상태 유지됨
            if (!HasSelectedPiece)
            {
                // 성공 시: UI는 이미 갱신되어 사라졌으므로 별도 처리 불필요
                Debug.Log("드래그 소환 성공 - UI 자동 갱신됨");
            }
            else
            {
                // 실패 시: 원래 위치로 복귀
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

        // 해당 기물이 소환 가능한 리스트에 있는지 확인
        var targetList = pieceColor == PieceColor.White ? whiteSummonPieceInfos : blackSummonPieceInfos;
        if (targetList == null || !targetList.Contains(pieceInfo))
        {
            Debug.LogWarning($"소환할 수 없는 기물입니다: {pieceInfo.pieceName} ({pieceColor})");
            return false;
        }

        // 현재 턴 플레이어의 기물인지 확인
        // if (GameManager.Instance.CurrentTurn != pieceColor)
        // {
        //     Debug.LogWarning($"현재 턴 플레이어의 기물이 아닙니다. {pieceColor}");
        //     return false;
        // }

        // 유효성 검사
        if (!PieceManager.Instance.IsValidPlacementPosition(targetPosition))
        {
            Debug.Log($"유효하지 않은 소환 위치: {targetPosition}");
            return false;
        }

        // 추가 소환 제한 로직이 있다면 여기에 구현
        // 예: 자원 소모, 쿨다운, 최대 소환 수 제한 등

        // 기물 배치 (애니메이션 포함)
        bool success = PieceManager.Instance.DeployPieceAnimated(pieceInfo, targetPosition, pieceColor);
        if (success)
        {
            // 소환 성공 시 리스트에서 기물 제거
            RemovePieceFromSummonList(pieceInfo, pieceColor);
            
            // 소환 완료 후 턴 전환 (애니메이션 완료 후 호출되도록 코루틴 사용)
            StartCoroutine(WaitForSummonAnimationAndAdvanceTurn());
        }
        
        return success;
    }
    
    /// <summary>
    /// 소환 리스트에서 기물 제거 및 UI 갱신
    /// </summary>
    /// <param name="pieceInfo">제거할 기물</param>
    /// <param name="pieceColor">기물 색상</param>
    private void RemovePieceFromSummonList(PieceInfo pieceInfo, PieceColor pieceColor)
    {
        var targetList = pieceColor == PieceColor.White ? whiteSummonPieceInfos : blackSummonPieceInfos;
        
        if (targetList != null && targetList.Contains(pieceInfo))
        {
            targetList.Remove(pieceInfo);
            Debug.Log($"소환 리스트에서 기물 제거: {pieceInfo.pieceName} ({pieceColor}), 남은 개수: {targetList.Count}");
            
            // UI 갱신
            RefreshSummonUI();
        }
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
        
        bool success = TrySummonPiece(selectedPieceInfo, targetPosition, selectedPieceColor);
        
        if (success)
        {
            // 성공 시 선택 해제 (하이라이트도 함께 제거됨)
            DeselectPieceForSummon();
            
        }

        return success;
    }

    /// <summary>
    /// 특정 색상의 기물을 기호로 찾습니다.
    /// </summary>
    /// <param name="pieceSymbol">기물 기호 (대소문자 구분 없음)</param>
    /// <param name="pieceColor">찾을 기물 색상</param>
    /// <returns>찾은 PieceInfo, 없으면 null</returns>
    public PieceInfo FindPieceInfoBySymbol(char pieceSymbol, PieceColor pieceColor)
    {
        char normalizedSymbol = char.ToLower(pieceSymbol);
        
        // 색상에 맞는 기물 리스트 선택
        var targetPieces = pieceColor == PieceColor.White ? 
            whiteSummonPieceInfos : 
            blackSummonPieceInfos;

        foreach (var pieceInfo in targetPieces)
        {
            if (pieceInfo != null && char.ToLower(pieceInfo.pieceAlphabet) == normalizedSymbol)
            {
                return pieceInfo;
            }
        }

        Debug.LogError($"색상({pieceColor})에 맞는 기물 기호 '{pieceSymbol}'에 해당하는 PieceInfo를 찾을 수 없습니다.");
        return null;
    }

    #region AI 전용 메서드들

    /// <summary>
    /// 특정 기물 정보와 색상에 해당하는 SummonPieceUI를 찾습니다.
    /// </summary>
    /// <param name="pieceInfo">찾을 기물 정보</param>
    /// <param name="color">기물 색상</param>
    /// <returns>찾은 SummonPieceUI, 없으면 null</returns>
    public SummonPieceUI FindSummonPieceUI(PieceInfo pieceInfo, PieceColor color)
    {
        Transform parentTransform = color == PieceColor.Black ? 
            blackSummonPieceParent : whiteSummonPieceParent;
        
        if (parentTransform == null)
        {
            Debug.LogError($"해당 색상의 소환 UI 부모가 설정되지 않았습니다: {color}");
            return null;
        }

        SummonPieceUI[] summonUIs = parentTransform.GetComponentsInChildren<SummonPieceUI>();
        
        foreach (var ui in summonUIs)
        {
            if (ui.PieceInfo == pieceInfo && ui.PieceColor == color)
            {
                return ui;
            }
        }
        
        return null;
    }

    /// <summary>
    /// AI용 향상된 소환 메서드 (시각적 피드백 포함)
    /// </summary>
    /// <param name="pieceInfo">소환할 기물 정보</param>
    /// <param name="targetPosition">목표 위치</param>
    /// <param name="pieceColor">기물 색상</param>
    public IEnumerator TrySummonPieceWithAIFeedback(PieceInfo pieceInfo, Vector2Int targetPosition, PieceColor pieceColor)
    {
        if (pieceInfo == null) 
        {
            Debug.LogWarning("소환할 기물 정보가 없습니다.");
            yield break;
        }

        // AI 선택 과정 시각적 피드백
        if (showAISelectionProcess)
        {
            SummonPieceUI targetUI = FindSummonPieceUI(pieceInfo, pieceColor);
            if (targetUI != null)
            {
                // AI가 기물을 "고려"하는 시각적 피드백
                targetUI.SetSelected(true);
                yield return new WaitForSeconds(aiSelectionDuration);
                targetUI.SetSelected(false);
            }
            
            // 목표 위치 하이라이트
            BoardManager.Instance.UpdateCellHighlight(targetPosition, pieceInfo, pieceColor);
            yield return new WaitForSeconds(aiHighlightDuration);
            BoardManager.Instance.ClearCellHighlight();
        }

        // 실제 소환 실행
        bool success = TrySummonPiece(pieceInfo, targetPosition, pieceColor);
        
        // 결과 로깅
        if (success)
        {
            Debug.Log($"AI 소환 성공: {pieceInfo.pieceName} → {targetPosition} ({pieceColor})");
        }
        else
        {
            Debug.LogError($"AI 소환 실패: {pieceInfo.pieceName} → {targetPosition} ({pieceColor})");
        }
    }

    /// <summary>
    /// AI용 간단한 소환 메서드 (시각적 피드백 없음)
    /// </summary>
    /// <param name="pieceInfo">소환할 기물 정보</param>
    /// <param name="targetPosition">목표 위치</param>
    /// <param name="pieceColor">기물 색상</param>
    /// <returns>소환 성공 여부</returns>
    public bool TrySummonPieceForAI(PieceInfo pieceInfo, Vector2Int targetPosition, PieceColor pieceColor)
    {
        // 기본 소환 로직에서 리스트 제거와 UI 갱신이 모두 처리됨
        return TrySummonPiece(pieceInfo, targetPosition, pieceColor);
    }

    #endregion

    /// <summary>
    /// 소환 애니메이션 완료를 기다린 후 턴을 전환합니다
    /// </summary>
    IEnumerator WaitForSummonAnimationAndAdvanceTurn()
    {
        // PieceManager의 애니메이션이 완료될 때까지 대기
        yield return new WaitUntil(() => !PieceManager.Instance.IsMoving);
        
        // 애니메이션 완료 후 턴 전환
        GameManager.Instance.TryAdvanceTurn();
    }
}
