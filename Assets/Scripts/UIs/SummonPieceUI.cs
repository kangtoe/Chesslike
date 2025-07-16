using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(EventTrigger))]
public class SummonPieceUI : MonoBehaviour
{
    [SerializeField] PieceInfo pieceInfo;
    [SerializeField] Image image;
    [SerializeField] PieceColor pieceColor = PieceColor.White;
    
    [Header("드래그 설정")]
    [SerializeField] float returnDuration = 0.5f; // 원래 위치로 돌아가는 시간
    
    private Vector2 originalAnchoredPosition;
    private Vector2 dragStartOffset; // 드래그 시작 시 마우스와 UI 사이의 오프셋
    private bool isDragging = false;
    private EventTrigger eventTrigger;
    private RectTransform rectTransform;
    private Canvas parentCanvas;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        originalAnchoredPosition = rectTransform.anchoredPosition;
        parentCanvas = GetComponentInParent<Canvas>();
        SetupEventTrigger();
    }

    public void SetPieceInfo(PieceInfo pieceInfo, PieceColor pieceColor)
    {
        this.pieceInfo = pieceInfo;
        this.pieceColor = pieceColor;
        UpdatePieceSprite();
    }

    /// <summary>
    /// 기물 정보에 따라 스프라이트 업데이트
    /// </summary>
    private void UpdatePieceSprite()
    {
        if (pieceInfo == null || image == null) return;
        
        // 색상에 따라 적절한 스프라이트 선택
        Sprite targetSprite = pieceColor == PieceColor.White ? pieceInfo.whiteSprite : pieceInfo.blackSprite;
        
        if (targetSprite != null)
        {
            image.sprite = targetSprite;
            Debug.Log($"스프라이트 업데이트: {pieceInfo.pieceName} ({pieceColor})");
        }
        else
        {
            Debug.LogWarning($"스프라이트가 없습니다: {pieceInfo.pieceName} ({pieceColor})");
        }
    }

    /// <summary>
    /// EventTrigger 설정
    /// </summary>
    private void SetupEventTrigger()
    {
        // EventTrigger 컴포넌트 가져오기 또는 추가
        eventTrigger = GetComponent<EventTrigger>();
        if (eventTrigger == null)
        {
            eventTrigger = gameObject.AddComponent<EventTrigger>();
        }

        // BeginDrag 이벤트 추가
        EventTrigger.Entry beginDragEntry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.BeginDrag
        };
        beginDragEntry.callback.AddListener((data) => { OnBeginDragEvent((PointerEventData)data); });
        eventTrigger.triggers.Add(beginDragEntry);

        // Drag 이벤트 추가
        EventTrigger.Entry dragEntry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.Drag
        };
        dragEntry.callback.AddListener((data) => { OnDragEvent((PointerEventData)data); });
        eventTrigger.triggers.Add(dragEntry);

        // EndDrag 이벤트 추가
        EventTrigger.Entry endDragEntry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.EndDrag
        };
        endDragEntry.callback.AddListener((data) => { OnEndDragEvent((PointerEventData)data); });
        eventTrigger.triggers.Add(endDragEntry);
    }

    /// <summary>
    /// 드래그 시작 이벤트
    /// </summary>
    public void OnBeginDragEvent(PointerEventData eventData)
    {
        if (pieceInfo == null) return;
        
        isDragging = true;
        originalAnchoredPosition = rectTransform.anchoredPosition;
        
        // 드래그 시작 시점의 마우스 UI 좌표 계산
        Vector2 mouseUIPos;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentCanvas.transform as RectTransform, 
            eventData.position, 
            parentCanvas.worldCamera, 
            out mouseUIPos))
        {
            // 마우스 위치와 UI 위치 사이의 오프셋 계산
            dragStartOffset = rectTransform.anchoredPosition - mouseUIPos;
        }
        
        Debug.Log($"드래그 시작: {pieceInfo.pieceName}");
    }

    /// <summary>
    /// 드래그 중 이벤트
    /// </summary>
    public void OnDragEvent(PointerEventData eventData)
    {
        if (!isDragging) return;
        
        // 마우스 위치를 UI 좌표로 변환
        Vector2 mouseUIPos;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentCanvas.transform as RectTransform, 
            eventData.position, 
            parentCanvas.worldCamera, 
            out mouseUIPos))
        {
            // 드래그 시작 시의 오프셋을 유지하며 UI 위치 업데이트
            rectTransform.anchoredPosition = mouseUIPos + dragStartOffset;
        }
        
        // 셀 하이라이트 업데이트
        BoardManager.Instance.UpdateCellHighlight(eventData.position, pieceInfo, pieceColor);
    }

    /// <summary>
    /// 드래그 종료 이벤트
    /// </summary>
    public void OnEndDragEvent(PointerEventData eventData)
    {
        if (!isDragging) return;
        
        isDragging = false;
        
        // 셀 하이라이트 제거
        BoardManager.Instance.ClearCellHighlight();
        
        // 마우스 위치에서 보드 셀 확인
        Vector2Int? targetCell = BoardManager.Instance.GetBoardCellFromScreenPosition(eventData.position);
        
        if (targetCell.HasValue)
        {
            // SummonManager를 통해 기물 소환 시도
            if (SummonManager.Instance.TrySummonPiece(pieceInfo, targetCell.Value, pieceColor))
            {
                Debug.Log($"기물 소환 성공: {pieceInfo.pieceName} ({pieceColor}) at {targetCell.Value}");
                
                // UI 삭제
                Destroy(gameObject);
                return;
            }
        }
        
        // 유효하지 않은 위치이므로 원래 위치로 부드럽게 돌아감
        ReturnToOriginalPosition();
    }

    /// <summary>
    /// 원래 위치로 부드럽게 돌아가기
    /// </summary>
    private void ReturnToOriginalPosition()
    {
        Debug.Log("유효하지 않은 위치 - 원래 위치로 돌아감");
        
        LeanTween.value(gameObject, rectTransform.anchoredPosition, originalAnchoredPosition, returnDuration)
            .setEase(LeanTweenType.easeOutQuad)
            .setOnUpdate((Vector2 pos) => {
                rectTransform.anchoredPosition = pos;
            })
            .setOnComplete(() => {
                Debug.Log("원래 위치로 복귀 완료");
            });
    }
}
