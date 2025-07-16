using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    // 기물 선택 상태 프로퍼티
    public bool HasSelectedPiece => selectedPieceInfo != null;
    public PieceInfo SelectedPieceInfo => selectedPieceInfo;
    public PieceColor SelectedPieceColor => selectedPieceColor;

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

        Debug.Log("소환용 기물 선택 해제");
    }

    /// <summary>
    /// 선택된 기물을 지정된 위치에 소환 시도 (클릭 방식용)
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
            // 성공시 UI 제거 및 선택 해제
            if (selectedPieceUI != null)
            {
                Destroy(selectedPieceUI.gameObject);
            }
            DeselectPieceForSummon();
        }

        return success;
    }

    /// <summary>
    /// 지정된 위치에 기물 소환 시도
    /// </summary>
    /// <param name="pieceInfo">소환할 기물 정보</param>
    /// <param name="targetPosition">목표 위치</param>
    /// <param name="pieceColor">기물 색상</param>
    /// <returns>소환 성공 여부</returns>
    public bool TrySummonPiece(PieceInfo pieceInfo, Vector2Int targetPosition, PieceColor pieceColor)
    {
        if (pieceInfo == null) return false;

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
        
        if (success)
        {
            //OnPieceSummoned(pieceInfo, targetPosition, pieceColor);
        }

        return success;
    }
}
