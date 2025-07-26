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

    public bool IsPlaying => _gameState == GameState.Playing;
    public bool IsPlayerTurn => _currentTurn == PieceColor.White;
    
    // 이벤트
    public event Action<PieceColor> OnTurnChanged;
    public event Action<PieceColor?> OnGameEnded;

    // 체크메이트 검증기
    private GameStateValidator gameStateValidator;

    // Start is called before the first frame update
    void Start()
    {
        gameStateValidator = GameStateValidator.Instance;

        StartGame();
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

        // 체스 게임 상태 변경 이벤트 발생
        //if(IsPlaying) OnChessGameStateChanged?.Invoke(chessState);
    }

    /// <summary>
    /// 턴 변경 후 체크/체크메이트 상태를 확인합니다
    /// </summary>
    private void CheckGameStateAfterTurn()
    {
        if (gameStateValidator == null || _gameState != GameState.Playing)
            return;

        // 현재 보드 상태의 FEN을 생성
        string currentFEN = ChessNotationUtil.GenerateFEN();
        
        if (!string.IsNullOrEmpty(currentFEN))
        {
            List<DeployedPiece> attackingPieces;
            ChessGameState chessState = gameStateValidator.CheckGameState(currentFEN, _currentTurn, out attackingPieces);
            
            string message = "";
            Color color = Color.white;

            // 체크메이트나 스테일메이트 상태일 경우 게임 종료
            switch (chessState)
            {
                case ChessGameState.Checkmate:
                    PieceColor winner = _currentTurn == PieceColor.White ? PieceColor.Black : PieceColor.White;
                    EndGame(winner);
                    
                    message = $"{chessState}! {winner} Wins!";
                    color = Color.red;
                    break;
                case ChessGameState.Stalemate:
                    EndGame(null);
                    
                    message = $"{chessState}! Draw!";
                    color = Color.yellow;
                    break;
                case ChessGameState.Check:
                    message = $"{_currentTurn} in {chessState}!";
                    color = new Color(1f, 0.5f, 0f); // 오렌지색

                    message += $"\nAttacked by: {attackingPieces.Count} pieces";
                    if (attackingPieces != null && attackingPieces.Count > 0)
                    {                                                
                        // 공격하는 기물 정보를 메시지에 추가
                        string attackInfo = "";
                        foreach (var piece in attackingPieces)
                        {
                            Debug.Log($"- {piece.PieceInfo.pieceName} at {piece.CellCoordinate}");
                            if (!string.IsNullOrEmpty(attackInfo)) attackInfo += ", ";
                            attackInfo += $"{piece.PieceInfo.pieceName}";
                        }
                        
                        message += $"\nAttacked by: {attackInfo}";
                    }
                    break;
                default:
                    break;
            }

            Text3dMaker.Instance.MakeText(message, BoardManager.Instance.BoardCenter, color);        
        }
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
