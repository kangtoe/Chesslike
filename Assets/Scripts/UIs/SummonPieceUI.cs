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
    
    [Header("선택 상태 표시")]
    [SerializeField] GameObject selectionIndicator; // 선택 표시 오브젝트 (optional)
    
    private bool isSelected = false;
    private EventTrigger eventTrigger;
    private RectTransform rectTransform;
    
    // 드래그 관련 변수들
    private Vector2 originalAnchoredPosition;
    
    // 원래 상태 저장
    private Color originalColor;
    private Vector3 originalScale;

    // Public 프로퍼티 추가
    public PieceInfo PieceInfo => pieceInfo;
    public PieceColor PieceColor => pieceColor;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        originalColor = image != null ? image.color : Color.white;
        originalScale = rectTransform.localScale;
        SetupEventTrigger();
        
        // 선택 표시 오브젝트 초기화
        if (selectionIndicator != null)
        {
            selectionIndicator.SetActive(false);
        }
    }

    /// <summary>
    /// UI 삭제 (외부 호출용)
    /// </summary>
    public void DestroySelf()
    {
        Destroy(gameObject);
    }

    public void SetPieceInfo(PieceInfo pieceInfo, PieceColor pieceColor)
    {
        this.pieceInfo = pieceInfo;
        this.pieceColor = pieceColor;
        UpdatePieceSprite();
    }

    /// <summary>
    /// 선택 상태 설정
    /// </summary>
    /// <param name="selected">선택 여부</param>
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        
        if (selected)
        {
            // 선택 상태 시각적 효과
            if (image != null)
            {
                image.color = SummonManager.Instance.SelectedColor;
            }
            rectTransform.localScale = originalScale * SummonManager.Instance.SelectedScale;
            
            if (selectionIndicator != null)
            {
                selectionIndicator.SetActive(true);
            }
        }
        else
        {
            // 원래 상태로 복원
            if (image != null)
            {
                image.color = originalColor;
            }
            rectTransform.localScale = originalScale;
            
            if (selectionIndicator != null)
            {
                selectionIndicator.SetActive(false);
            }
        }
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
            originalColor = image.color; // 스프라이트 변경 후 원래 색상 업데이트
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

        // PointerClick 이벤트 추가 - 직접 SummonManager 바인딩
        EventTrigger.Entry clickEntry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerClick
        };
        clickEntry.callback.AddListener((data) => { SummonManager.Instance.OnPointerClick((PointerEventData)data); });
        eventTrigger.triggers.Add(clickEntry);

        // BeginDrag 이벤트 추가 - 직접 SummonManager 바인딩
        EventTrigger.Entry beginDragEntry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.BeginDrag
        };
        beginDragEntry.callback.AddListener((data) => { SummonManager.Instance.OnBeginDrag((PointerEventData)data); });
        eventTrigger.triggers.Add(beginDragEntry);

        // Drag 이벤트 추가 - 직접 SummonManager 바인딩
        EventTrigger.Entry dragEntry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.Drag
        };
        dragEntry.callback.AddListener((data) => { SummonManager.Instance.OnDrag((PointerEventData)data); });
        eventTrigger.triggers.Add(dragEntry);

        // EndDrag 이벤트 추가 - 직접 SummonManager 바인딩
        EventTrigger.Entry endDragEntry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.EndDrag
        };
        endDragEntry.callback.AddListener((data) => { SummonManager.Instance.OnEndDrag((PointerEventData)data); });
        eventTrigger.triggers.Add(endDragEntry);
    }

    /// <summary>
    /// 드래그 초기화 (SummonManager에서 호출)
    /// </summary>
    /// <param name="eventData">이벤트 데이터</param>
    public void InitializeDrag(PointerEventData eventData)
    {
        // UI 드래그 정보 초기화
        originalAnchoredPosition = rectTransform.anchoredPosition;
    }

    /// <summary>
    /// 드래그 위치 업데이트 (SummonManager에서 호출)
    /// </summary>
    /// <param name="eventData">이벤트 데이터</param>
    public void UpdateDragPosition(PointerEventData eventData)
    {
        // 단순하게 스크린 좌표를 월드 좌표로 변환 후 UI 위치 설정
        Vector3 worldPosition;
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(rectTransform, eventData.position, null, out worldPosition))
        {
            rectTransform.position = worldPosition;
        }
    }



    /// <summary>
    /// 원래 위치로 부드럽게 돌아가기 (SummonManager에서 호출)
    /// </summary>
    public void ReturnToOriginalPosition()
    {
        Debug.Log("유효하지 않은 위치 - 원래 위치로 돌아감");
        
        LeanTween.value(gameObject, rectTransform.anchoredPosition, originalAnchoredPosition, SummonManager.Instance.ReturnDuration)
            .setEase(LeanTweenType.easeOutQuad)
            .setOnUpdate((Vector2 pos) => {
                rectTransform.anchoredPosition = pos;
            })
            .setOnComplete(() => {
                Debug.Log("원래 위치로 복귀 완료");
            });
    }
}
