using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SummonManager : MonoSingleton<SummonManager>
{
    [Header("Summon Piece UI")]
    [SerializeField] PieceInfo[] summonPieceInfos;
    [SerializeField] SummonPieceUI summonPiecePrefab;
    [SerializeField] Transform summonPieceParent;

    void Start()
    {
        foreach (var pieceInfo in summonPieceInfos)
        {
            SummonPieceUI summonPiece = Instantiate(summonPiecePrefab, summonPieceParent);
            summonPiece.SetPieceInfo(pieceInfo, PieceColor.White);
        }
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
