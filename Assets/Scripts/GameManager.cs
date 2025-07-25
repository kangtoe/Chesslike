using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum GameState
{
    NotStarted,
    Playing,
    Paused,
    GameOver
}

public class GameManager : MonoSingleton<GameManager>
{
    [NaughtyAttributes.ReadOnly]
    [SerializeField] PieceColor _currentTurn = PieceColor.White;
    public PieceColor CurrentTurn => _currentTurn;

    [SerializeField] int _turnCount = 0;
    public int TurnCount => _turnCount;

    // 게임 상태 관리
    [SerializeField] GameState _gameState = GameState.NotStarted;
    public GameState CurrentGameState => _gameState;

    public bool IsPlayerTurn => _currentTurn == PieceColor.White;
    
    // 이벤트
    public event Action<PieceColor> OnTurnChanged;
    public event Action<PieceColor?> OnGameEnded;
    public event Action<ChessGameState> OnChessGameStateChanged; // 체크/체크메이트 상태 변경 이벤트

    // 체크메이트 검증기
    private GameStateValidator gameStateValidator;

    // Start is called before the first frame update
    void Start()
    {
        gameStateValidator = GameStateValidator.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [NaughtyAttributes.Button]
    public void NextTurn()
    {
        _turnCount++;
        _currentTurn = _currentTurn == PieceColor.White ? PieceColor.Black : PieceColor.White;

        string str = $"Turn {_turnCount} : {_currentTurn}";
        Text3dMaker.Instance.MakeText(str, BoardManager.Instance.BoardCenter, Color.white);
        
        // 턴 변경 이벤트 발생
        OnTurnChanged?.Invoke(_currentTurn);

        // 체크/체크메이트 상태 확인
        CheckGameStateAfterTurn();
    }

    /// <summary>
    /// 턴 변경 후 체크/체크메이트 상태를 확인합니다
    /// </summary>
    private void CheckGameStateAfterTurn()
    {
        if (gameStateValidator == null || _gameState != GameState.Playing)
            return;

        // 현재 보드 상태의 FEN을 생성 (이 부분은 BoardManager나 PieceManager에서 구현 필요)
        string currentFEN = GetCurrentBoardFEN();
        
        if (!string.IsNullOrEmpty(currentFEN))
        {
            ChessGameState chessState = gameStateValidator.CheckGameState(currentFEN, _currentTurn);
            
            // 체크메이트나 스테일메이트 상태일 경우 게임 종료
            if (chessState == ChessGameState.Checkmate)
            {
                PieceColor winner = _currentTurn == PieceColor.White ? PieceColor.Black : PieceColor.White;
                EndGame(winner);
                
                string message = $"{gameStateValidator.GetGameStateString(chessState)}! {winner} 승리!";
                Text3dMaker.Instance.MakeText(message, BoardManager.Instance.BoardCenter, Color.red);
            }
            else if (chessState == ChessGameState.Stalemate)
            {
                EndGame(null); // 무승부
                
                string message = $"{gameStateValidator.GetGameStateString(chessState)}! 무승부!";
                Text3dMaker.Instance.MakeText(message, BoardManager.Instance.BoardCenter, Color.yellow);
            }
                         else if (chessState == ChessGameState.Check)
             {
                 string message = $"{_currentTurn} {gameStateValidator.GetGameStateString(chessState)}!";
                 Text3dMaker.Instance.MakeText(message, BoardManager.Instance.BoardCenter, new Color(1f, 0.5f, 0f)); // 오렌지색
             }

            // 체스 게임 상태 변경 이벤트 발생
            OnChessGameStateChanged?.Invoke(chessState);
        }
    }

    /// <summary>
    /// 현재 보드 상태를 FEN 문자열로 반환합니다
    /// TODO: BoardManager나 PieceManager에서 구현해야 할 메서드
    /// </summary>
    /// <returns>현재 보드의 FEN 문자열</returns>
    private string GetCurrentBoardFEN()
    {
        // 이 메서드는 BoardManager나 PieceManager에서 구현해야 합니다
        // 현재는 임시로 빈 문자열을 반환합니다
        Debug.LogWarning("GetCurrentBoardFEN 메서드가 구현되지 않았습니다. BoardManager나 PieceManager에서 구현이 필요합니다.");
        return "";
    }

    /// <summary>
    /// 현재 체스 게임 상태를 확인합니다 (외부에서 호출 가능)
    /// </summary>
    /// <returns>현재 체스 게임 상태</returns>
    public ChessGameState GetCurrentChessGameState()
    {
        if (gameStateValidator == null || _gameState != GameState.Playing)
            return ChessGameState.Normal;

        string currentFEN = GetCurrentBoardFEN();
        if (string.IsNullOrEmpty(currentFEN))
            return ChessGameState.Normal;

        return gameStateValidator.CheckGameState(currentFEN, _currentTurn);
    }

    #region 게임 흐름 제어
    public void StartGame() { 
        _gameState = GameState.Playing;
    }
    public void PauseGame() { 
        _gameState = GameState.Paused;
    }
    public void EndGame(PieceColor? winner) { 
        _gameState = GameState.GameOver;
        OnGameEnded?.Invoke(winner);

        string str = winner.ToString() + " Win!";
        Text3dMaker.Instance.MakeText(str, BoardManager.Instance.BoardCenter, Color.white);
    }
    public void RestartGame() { 
        _gameState = GameState.Playing;
    }
    #endregion 게임 흐름 제어
}
