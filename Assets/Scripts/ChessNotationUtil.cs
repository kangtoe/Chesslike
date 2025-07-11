using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 체스 표기법과 Unity 좌표계 간의 변환을 담당하는 유틸리티 클래스
/// </summary>
public static class ChessNotationUtil
{
    /// <summary>
    /// 체스 표기법(예: "e2e4")을 두 개의 Vector2Int 좌표로 변환합니다.
    /// </summary>
    /// <param name="moveNotation">체스 이동 표기법 (예: "e2e4")</param>
    /// <param name="fromCoord">시작 좌표</param>
    /// <param name="toCoord">목표 좌표</param>
    /// <returns>변환 성공 여부</returns>
    public static bool TryParseChessNotation(string moveNotation, out Vector2Int fromCoord, out Vector2Int toCoord)
    {
        fromCoord = Vector2Int.zero;
        toCoord = Vector2Int.zero;
        
        if (string.IsNullOrEmpty(moveNotation) || moveNotation.Length < 4)
        {
            Debug.LogError($"잘못된 이동 표기법: {moveNotation}");
            return false;
        }
        
        // "e2e4"에서 시작점과 끝점 분리
        string fromNotation = moveNotation.Substring(0, 2);
        string toNotation = moveNotation.Substring(2, 2);
        
        if (!TryParseSquareNotation(fromNotation, out fromCoord) ||
            !TryParseSquareNotation(toNotation, out toCoord))
        {
            return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// 단일 체스 칸 표기법(예: "e2")을 Vector2Int로 변환합니다.
    /// </summary>
    /// <param name="square">체스 칸 표기법 (예: "e2")</param>
    /// <param name="coord">변환된 좌표</param>
    /// <returns>변환 성공 여부</returns>
    public static bool TryParseSquareNotation(string square, out Vector2Int coord)
    {
        coord = Vector2Int.zero;
        
        if (string.IsNullOrEmpty(square) || square.Length != 2)
        {
            Debug.LogError($"잘못된 칸 표기법: {square}");
            return false;
        }
        
        char file = square[0]; // 파일 (a-h)
        char rank = square[1]; // 랭크 (1-8)
        
        // 파일 변환 (a=0, b=1, ..., h=7)
        if (file < 'a' || file > 'h')
        {
            Debug.LogError($"잘못된 파일: {file}");
            return false;
        }
        int x = file - 'a';
        
        // 랭크 변환 (1=0, 2=1, ..., 8=7)
        if (rank < '1' || rank > '8')
        {
            Debug.LogError($"잘못된 랭크: {rank}");
            return false;
        }
        int y = rank - '1';
        
        coord = new Vector2Int(x, y);
        return true;
    }
    
    /// <summary>
    /// Vector2Int 좌표를 체스 표기법으로 변환합니다.
    /// </summary>
    /// <param name="coord">좌표</param>
    /// <returns>체스 표기법 (예: "e2")</returns>
    public static string CoordinateToChessNotation(Vector2Int coord)
    {
        if (coord.x < 0 || coord.x > 7 || coord.y < 0 || coord.y > 7)
        {
            Debug.LogError($"잘못된 좌표: {coord}");
            return "";
        }
        
        char file = (char)('a' + coord.x);
        char rank = (char)('1' + coord.y);
        
        return $"{file}{rank}";
    }
    
    /// <summary>
    /// 현재 배치된 피스들로부터 FEN 문자열을 생성합니다.
    /// </summary>
    /// <returns>FEN 문자열</returns>
    public static string GenerateFEN()
    {
        Dictionary<Vector2Int, DeployedPiece> deployedPieces = PieceManager.Instance.DeployedPieces;
        int boardWidth = BoardManager.Instance.BoardWidth;
        int boardHeight = BoardManager.Instance.BoardHeight;

        if (deployedPieces == null)
        {
            Debug.LogError("deployedPieces가 null입니다.");
            return "";
        }
        
        System.Text.StringBuilder fen = new System.Text.StringBuilder();

        for (int y = boardHeight - 1; y >= 0; y--) // FEN은 8(위)~1(아래) 순서
        {
            int emptyCount = 0;
            for (int x = 0; x < boardWidth; x++)
            {
                Vector2Int coord = new Vector2Int(x, y);
                if (deployedPieces.TryGetValue(coord, out DeployedPiece piece))
                {
                    if (emptyCount > 0)
                    {
                        fen.Append(emptyCount);
                        emptyCount = 0;
                    }
                    char symbol = piece.PieceInfo.pieceAlphabet;
                    // 백: 대문자, 흑: 소문자
                    fen.Append(piece.PieceColor == PieceColor.White ? char.ToUpper(symbol) : char.ToLower(symbol));
                }
                else
                {
                    emptyCount++;
                }
            }
            if (emptyCount > 0)
                fen.Append(emptyCount);
            if (y > 0)
                fen.Append('/');
        }
        return fen.ToString();
    }
} 