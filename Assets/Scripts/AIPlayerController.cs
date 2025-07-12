using System.Collections;
using UnityEngine;
using Debug = UnityEngine.Debug;

/// <summary>
/// AI 플레이어 동작을 담당하는 컨트롤러
/// </summary>
public class AIPlayerController : MonoSingleton<AIPlayerController>
{
    [Header("AI 설정")]
    [SerializeField] private float aiMoveDelay = 1f; // AI 이동 전 대기 시간    
    [SerializeField] private PieceColor aiColor = PieceColor.Black; // AI가 플레이할 색상
    
    private void Start()
    {
        // TurnManager의 턴 변경 이벤트 구독
        TurnManager.Instance.OnTurnChanged += OnTurnChanged;
    }
    
    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (TurnManager.Instance != null)
            TurnManager.Instance.OnTurnChanged -= OnTurnChanged;
    }
    
    /// <summary>
    /// 턴 변경 이벤트 핸들러
    /// </summary>
    private void OnTurnChanged(PieceColor newTurn)
    {
        // AI 색상의 턴으로 변경되었고 AI가 아직 동작하지 않았으며, 기물이 이동 중이 아닐 때
        if (newTurn == aiColor)
        {
            StartCoroutine(ExecuteAIMove());
        }
    }
    
    /// <summary>
    /// AI의 이동을 실행합니다.
    /// </summary>
    private IEnumerator ExecuteAIMove()
    {
        yield return new WaitUntil(() => !PieceManager.Instance.IsMoving);

        // AI 이동 전 잠시 대기 (사용자가 AI 동작을 볼 수 있도록)
        yield return new WaitForSeconds(aiMoveDelay);        
        
        // 현재 게임 상태를 FEN으로 변환
        string currentFEN = ChessNotationUtil.GenerateFEN();
        
        if (string.IsNullOrEmpty(currentFEN))
        {
            Debug.LogError("FEN 생성 실패");
            yield break;
        }
        
        // AI로부터 최적 수 계산
        string bestMove = AIManager.Instance.GetBestMove(currentFEN, aiColor);
        
        if (string.IsNullOrEmpty(bestMove))
        {
            Debug.LogError("AI가 최적 수를 찾지 못했습니다.");
            yield break;
        }
        
        // 체스 표기법을 좌표로 변환
        if (!ChessNotationUtil.TryParseChessNotation(bestMove, out Vector2Int fromCoord, out Vector2Int toCoord))
        {
            Debug.LogError($"잘못된 이동 표기법: {bestMove}");
            yield break;
        }
        
        // 해당 위치의 기물 찾기
        if (!PieceManager.Instance.TryGetPieceAt(fromCoord, out DeployedPiece piece))
        {
            Debug.LogError($"시작 위치 {fromCoord}에 기물이 없습니다.");
            yield break;
        }
        
        // 기물 선택
        PieceManager.Instance.SelectPiece(piece);
        
        // 이동 실행
        bool moveSuccess = PieceManager.Instance.MovePiece(toCoord);
        
        if (!moveSuccess)
        {
            Debug.LogError($"AI 이동 실패: {fromCoord} → {toCoord}");
            yield break;
        }
        
        // 이동 완료 대기
        yield return new WaitUntil(() => !PieceManager.Instance.IsMoving);
        
        Debug.Log($"AI 이동 완료: {fromCoord} → {toCoord}");
    }
} 