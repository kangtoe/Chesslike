using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 체스 게임의 체크, 체크메이트, 스테일메이트 상태를 검증하는 클래스
/// </summary>
public enum ChessGameState
{
    Normal,
    Check,
    Checkmate,
    Stalemate
}

public class GameStateValidator : MonoSingleton<GameStateValidator>
{
    private StockfishConnector stockfishConnector;
    StockfishConnector StockfishConnector
    {
        get{
            if (stockfishConnector == null)
            {
                stockfishConnector = FindObjectOfType<StockfishConnector>();
            }
            return stockfishConnector;
        }
    }

    PieceManager PieceManager => PieceManager.Instance;    

    public ChessGameState CheckGameState(string fen, PieceColor currentPlayer)
    {
        List<DeployedPiece> attackingPieces;
        return CheckGameState(fen, currentPlayer, out attackingPieces);
    }

    /// <summary>
    /// 특정 움직임 후의 게임 상태를 확인합니다
    /// </summary>
    /// <param name="fen">현재 보드 상태의 FEN 문자열</param>
    /// <param name="currentPlayer">현재 턴인 플레이어</param>
    /// <param name="moveNotation">적용할 움직임 (UCI 표기법, 예: "e2e4")</param>
    /// <returns>움직임 적용 후의 게임 상태</returns>
    public ChessGameState CheckGameState(string fen, PieceColor currentPlayer, out List<DeployedPiece> attackingPieces, string moveNotation)
    {
        // ChessNotationUtil을 사용하여 움직임 후 FEN 생성
        string newFen = ChessNotationUtil.GenerateFENAfterMove(fen, moveNotation, currentPlayer);

        // 생성된 FEN으로 게임 상태 확인
        return CheckGameState(newFen, currentPlayer, out attackingPieces);
    }

    /// <summary>
    /// 현재 게임 상태를 확인합니다 (체크, 체크메이트, 스테일메이트)
    /// </summary>
    /// <param name="fen">현재 보드 상태의 FEN 문자열</param>
    /// <param name="currentPlayer">현재 턴인 플레이어</param>
    /// <returns>게임 상태</returns>
    public ChessGameState CheckGameState(string fen, PieceColor currentPlayer, out List<DeployedPiece> attackingPieces)
    {
        attackingPieces = null;

        if (StockfishConnector == null)
        {
            Debug.LogError("StockfishConnector가 초기화되지 않았습니다!");
            return ChessGameState.Normal;
        }

        if (string.IsNullOrEmpty(fen))
        {
            Debug.LogError("FEN is empty");
            return ChessGameState.Normal;
        }

        // 스톡피시로부터 직접 bestmove 획득
        string colorStr = (currentPlayer == PieceColor.White) ? "w" : "b";
        string fullFen = fen + " " + colorStr + " - - 0 1";
        string bestMove = StockfishConnector.GetBestMoveFromFEN(fullFen, 1, currentPlayer);

        // bestmove 결과로 합법적 움직임 존재 여부 판단
        bool hasLegalMoves = !string.IsNullOrEmpty(bestMove) && !bestMove.Equals("(none)");
        
        if (!hasLegalMoves)
        {
            // 합법적인 움직임이 없는 경우
            if (IsInCheck(currentPlayer, out attackingPieces) > 0)
            {
                return ChessGameState.Checkmate;
            }
            else
            {
                return ChessGameState.Stalemate;
            }
        }
        else
        {
            // 합법적인 움직임이 있는 경우 체크 상태만 확인
            if (IsInCheck(currentPlayer, out attackingPieces) > 0)
            {
                return ChessGameState.Check;
            }
        }

        return ChessGameState.Normal;
    }

    /// <summary>
    /// 킹이 체크 상태인지 확인하고 위협하는 기물들을 반환합니다
    /// </summary>
    /// <param name="currentPlayer">현재 턴인 플레이어</param>
    /// <param name="attackingPieces">위협하는 기물들 (out 매개변수)</param>
    /// <returns></returns>
    public int IsInCheck(PieceColor currentPlayer, out List<DeployedPiece> attackingPieces)
    {
        attackingPieces = new List<DeployedPiece>();

        // 현재 플레이어의 킹 찾기
        DeployedPiece king = PieceManager.FindKing(currentPlayer);
        if (king == null)
        {
            Debug.LogError($"{currentPlayer} 킹을 찾을 수 없습니다!");
            return 0;
        }

        // 상대방 기물들이 킹을 공격하는지 확인
        PieceColor opponentColor = currentPlayer == PieceColor.White ? PieceColor.Black : PieceColor.White;
        
        foreach (var piece in PieceManager.DeployedPieces.Values)
        {
            if (piece.PieceColor == opponentColor)
            {
                // BoardManager의 기존 로직을 활용하여 이 기물이 공격할 수 있는 위치들을 가져옴
                List<Vector2Int> movableCells, attackCells;
                BoardManager.Instance.GetMovableCells(piece, out movableCells, out attackCells);
                
                // 킹의 위치가 공격 가능한 위치에 포함되는지 확인
                if (attackCells != null && attackCells.Contains(king.CellCoordinate))
                {
                    attackingPieces.Add(piece);
                }
            }
        }

        return attackingPieces.Count;
    }

    public int IsInCheck(PieceColor currentPlayer)
    {
        List<DeployedPiece> attackingPieces;
        return IsInCheck(currentPlayer, out attackingPieces);
    }

    /// <summary>
    /// 킹의 특정 위치로의 이동이 안전한지 직접 확인합니다 (순환 호출 방지)
    /// </summary>
    /// <param name="king">킹 기물</param>
    /// <param name="targetPosition">이동할 위치</param>
    /// <returns>안전하면 true, 체크 상태가 되면 false</returns>
    public bool IsKingMoveToPositionSafe(DeployedPiece king, Vector2Int targetPosition)
    {
        // 상대방 색상
        PieceColor opponentColor = king.PieceColor == PieceColor.White ? PieceColor.Black : PieceColor.White;
        
        // 모든 상대 기물들이 킹의 새 위치를 공격할 수 있는지 확인
        foreach (var piece in PieceManager.DeployedPieces.Values)
        {
            // 킹 자신과 제거될 기물은 제외, 상대방 기물만 확인
            if (piece.PieceColor == opponentColor && 
                piece != king && 
                piece.CellCoordinate != targetPosition &&
                !piece.PieceInfo.pieceName.ToLower().Contains("king")) // 상대 킹은 제외 (인접성은 별도 확인)
            {
                // 기존 GetMovableCells를 활용하여 이 기물이 공격할 수 있는 위치들을 가져옴
                List<Vector2Int> movableCells, attackCells;
                BoardManager.Instance.GetMovableCells(piece, out movableCells, out attackCells);
                
                // 킹의 새 위치가 이동 가능하거나 공격 가능한 위치에 포함되는지 확인
                bool canMoveToTarget = movableCells != null && movableCells.Contains(targetPosition);
                bool canAttackTarget = attackCells != null && attackCells.Contains(targetPosition);
                
                if (canMoveToTarget || canAttackTarget)
                {
                    return false; // 공격받을 수 있으므로 안전하지 않음
                }
            }
        }
        
        // 상대 킹과의 인접성 확인 (킹끼리는 인접할 수 없음)
        DeployedPiece opponentKing = PieceManager.FindKing(opponentColor);
        if (opponentKing != null)
        {
            int distanceX = Mathf.Abs(targetPosition.x - opponentKing.CellCoordinate.x);
            int distanceY = Mathf.Abs(targetPosition.y - opponentKing.CellCoordinate.y);
            if (distanceX <= 1 && distanceY <= 1)
            {
                return false; // 킹끼리 인접하면 안전하지 않음
            }
        }
        
        return true; // 안전함
    }

} 