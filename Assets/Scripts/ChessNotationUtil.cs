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

    /// <summary>
    /// 특정 움직임 적용 후 FEN을 생성합니다 (간단한 기물 이동만 처리)
    /// </summary>
    /// <param name="originalFen">원본 FEN</param>
    /// <param name="moveNotation">적용할 움직임 (UCI 표기법, 예: "e2e4")</param>
    /// <param name="currentPlayer">현재 플레이어</param>
    /// <returns>움직임 적용 후 FEN</returns>
    public static string GenerateFENAfterMove(string originalFen, string moveNotation, PieceColor currentPlayer)
    {
        if (string.IsNullOrEmpty(originalFen) || string.IsNullOrEmpty(moveNotation))
            return originalFen;

        if (moveNotation.Length < 4)
            return originalFen;

        try
        {
            // UCI 표기법 파싱 (예: "e2e4")
            Vector2Int fromPos, toPos;
            if (!TryParseChessNotation(moveNotation, out fromPos, out toPos))
                return originalFen;

            // FEN 분해 (보드 상태만 사용)
            string[] fenParts = originalFen.Split(' ');
            if (fenParts.Length == 0) return originalFen;

            string boardState = fenParts[0];

            // 보드 상태를 2차원 배열로 변환
            char[,] board = FENToBoard(boardState);

            // 기물 이동 (단순히 from에서 to로 이동)
            char movingPiece = board[fromPos.y, fromPos.x];
            board[fromPos.y, fromPos.x] = ' ';
            board[toPos.y, toPos.x] = movingPiece;

            // 새로운 보드 상태를 FEN으로 변환
            string newBoardState = BoardToFEN(board);

            // 턴 변경
            string newActiveColor = currentPlayer == PieceColor.White ? "b" : "w";

            // 나머지 FEN 요소들은 간단하게 처리
            string castling = fenParts.Length > 2 ? fenParts[2] : "KQkq";
            string enPassant = "-";
            string halfmove = fenParts.Length > 4 ? fenParts[4] : "0";
            string fullmove = fenParts.Length > 5 ? fenParts[5] : "1";

            // 새 FEN 생성
            return $"{newBoardState} {newActiveColor} {castling} {enPassant} {halfmove} {fullmove}";
        }
        catch (System.Exception e)
        {
            Debug.LogError($"FEN 생성 중 오류 발생: {e.Message}");
            return originalFen;
        }
    }

    /// <summary>
    /// FEN 보드 문자열을 2차원 배열로 변환
    /// </summary>
    /// <param name="fenBoard">FEN 보드 부분</param>
    /// <returns>8x8 보드 배열</returns>
    public static char[,] FENToBoard(string fenBoard)
    {
        char[,] board = new char[8, 8];
        
        // 배열을 공백으로 초기화
        for (int i = 0; i < 8; i++)
            for (int j = 0; j < 8; j++)
                board[i, j] = ' ';

        string[] ranks = fenBoard.Split('/');
        
        for (int rank = 0; rank < 8; rank++)
        {
            string rankString = ranks[rank];
            int file = 0;
            
            foreach (char c in rankString)
            {
                if (char.IsDigit(c))
                {
                    // 숫자는 빈 칸의 수
                    file += c - '0';
                }
                else
                {
                    // 문자는 기물
                    board[rank, file] = c;
                    file++;
                }
            }
        }
        
        return board;
    }

    /// <summary>
    /// 2차원 배열을 FEN 보드 문자열로 변환
    /// </summary>
    /// <param name="board">8x8 보드 배열</param>
    /// <returns>FEN 보드 문자열</returns>
    public static string BoardToFEN(char[,] board)
    {
        string[] ranks = new string[8];
        
        for (int rank = 0; rank < 8; rank++)
        {
            string rankString = "";
            int emptyCount = 0;
            
            for (int file = 0; file < 8; file++)
            {
                char piece = board[rank, file];
                
                if (piece == ' ')
                {
                    emptyCount++;
                }
                else
                {
                    if (emptyCount > 0)
                    {
                        rankString += emptyCount.ToString();
                        emptyCount = 0;
                    }
                    rankString += piece;
                }
            }
            
            if (emptyCount > 0)
                rankString += emptyCount.ToString();
                
            ranks[rank] = rankString;
        }
        
        return string.Join("/", ranks);
    }
} 