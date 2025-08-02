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
        GameManager.Instance.OnTurnChanged += OnTurnChanged;
    }
    
    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (GameManager.Instance != null)
            GameManager.Instance.OnTurnChanged -= OnTurnChanged;
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
        
        // 소환 명령어인지 확인
        if (ChessNotationUtil.IsSummonNotation(bestMove))
        {
            // 소환 명령어 처리
            yield return StartCoroutine(ExecuteAISummon(bestMove));
        }
        else
        {
            // 일반 이동 명령어 처리 (PieceManager에서 턴 넘김)
            yield return StartCoroutine(ExecuteAIMovePiece(bestMove));
        }
    }

    /// <summary>
    /// AI 소환을 실행합니다. (향상된 버전 - 시각적 피드백 포함)
    /// </summary>
    /// <param name="summonNotation">소환 표기법 (예: "P@e4")</param>
    private IEnumerator ExecuteAISummon(string summonNotation)
    {
        // 소환 표기법 파싱
        if (!ChessNotationUtil.TryParseSummonNotation(summonNotation, out char pieceSymbol, out Vector2Int targetCoord))
        {
            Debug.LogError($"잘못된 소환 표기법: {summonNotation}");
            yield break;
        }

        // AI 색상에 맞는 기물 정보 찾기
        PieceInfo pieceInfo = SummonManager.Instance.FindPieceInfoBySymbol(pieceSymbol, aiColor);
        if (pieceInfo == null)
        {
            Debug.LogError($"AI 색상에 맞는 기물 정보를 찾을 수 없습니다: {pieceSymbol}");
            yield break;
        }

        // 시각적 피드백과 함께 소환 실행
        yield return StartCoroutine(ExecuteAISummonWithFeedback(pieceInfo, targetCoord, summonNotation));
    }

    /// <summary>
    /// AI 소환을 시각적 피드백과 함께 실행합니다.
    /// </summary>
    /// <param name="pieceInfo">소환할 기물 정보</param>
    /// <param name="targetCoord">목표 좌표</param>
    /// <param name="summonNotation">원본 소환 표기법 (로깅용)</param>
    private IEnumerator ExecuteAISummonWithFeedback(PieceInfo pieceInfo, Vector2Int targetCoord, string summonNotation)
    {
        Debug.Log($"AI 소환 시작 (피드백 포함): {summonNotation}");
        
        // 시각적 피드백과 함께 소환 실행
        yield return StartCoroutine(SummonManager.Instance.TrySummonPieceWithAIFeedback(pieceInfo, targetCoord, aiColor));
        
        Debug.Log($"AI 소환 완료 (피드백 포함): {summonNotation}");
    }

    /// <summary>
    /// AI 기물 이동을 실행합니다.
    /// </summary>
    /// <param name="moveNotation">이동 표기법 (예: "e2e4")</param>
    private IEnumerator ExecuteAIMovePiece(string moveNotation)
    {
        // 체스 표기법을 좌표로 변환
        if (!ChessNotationUtil.TryParseChessNotation(moveNotation, out Vector2Int fromCoord, out Vector2Int toCoord))
        {
            Debug.LogError($"잘못된 이동 표기법: {moveNotation}");
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