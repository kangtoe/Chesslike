using System.Collections.Generic;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class CustomPieceRegister : MonoBehaviour
{
    [Header("Custom Pieces")]
    [SerializeField] List<PieceInfo> customPieces = new List<PieceInfo>();

    void OnValidate()
    {
        // 중복 제거
        RemoveDuplicates();
    }

    /// <summary>
    /// customPieces 리스트에서 중복된 기물을 제거합니다.
    /// </summary>
    void RemoveDuplicates()
    {
        if (customPieces == null) return;

        // 중복 제거를 위한 HashSet 사용
        HashSet<PieceInfo> uniquePieces = new HashSet<PieceInfo>();
        List<PieceInfo> newList = new List<PieceInfo>();

        foreach (var piece in customPieces)
        {
            if (piece != null && uniquePieces.Add(piece))
            {
                newList.Add(piece);
            }
        }

        // 중복이 제거된 경우에만 리스트 업데이트
        if (newList.Count != customPieces.Count)
        {
            customPieces = newList;
            Debug.Log($"중복된 기물이 제거되었습니다. 현재 기물 수: {customPieces.Count}");
        }
    }

    /// <summary>
    /// 커스텀 기물들을 Fairy-Stockfish 엔진에 등록합니다.
    /// </summary>
    public void RegisterCustomPieces(StockfishConnector connector)
    {
        if (customPieces == null || customPieces.Count == 0)
        {
            Debug.LogWarning("등록할 커스텀 기물이 없습니다.");
            return;
        }

        int registeredCount = 0;
        
        foreach (var piece in customPieces)
        {
            if (piece == null) continue;
            
            // Fairy-Stockfish에 기물 등록
            RegisterPieceToEngine(piece, connector);
            registeredCount++;
        }
        
        Debug.Log($"총 {registeredCount}개의 커스텀 기물이 등록되었습니다.");
    }

    /// <summary>
    /// 개별 기물을 Fairy-Stockfish 엔진에 등록합니다.
    /// </summary>
    void RegisterPieceToEngine(PieceInfo piece, StockfishConnector connector)
    {
        if (piece == null || connector == null) return;

        // 기물 정의 문자열 생성
        string pieceDefinition = GeneratePieceDefinition(piece);
        
        // Fairy-Stockfish에 기물 등록
        connector.RegisterPieceToEngine(piece.pieceAlphabet.ToString(), pieceDefinition);
        
        Debug.Log($"기물 등록: {piece.pieceName} ({piece.pieceAlphabet}) = {pieceDefinition}");
    }

    /// <summary>
    /// PieceInfo를 기반으로 Fairy-Stockfish 기물 정의 문자열을 생성합니다.
    /// </summary>
    string GeneratePieceDefinition(PieceInfo piece)
    {
        List<string> moves = new List<string>();
        
        // 룩 이동 (가로/세로)
        if (piece.movement.rookMove > 0)
        {
            for (int i = 1; i <= piece.movement.rookMove; i++)
            {
                moves.Add($"0,{i}");   // 위
                moves.Add($"0,-{i}");  // 아래
                moves.Add($"{i},0");   // 오른쪽
                moves.Add($"-{i},0");  // 왼쪽
            }
        }
        
        // 비숍 이동 (대각선)
        if (piece.movement.bishopMove > 0)
        {
            for (int i = 1; i <= piece.movement.bishopMove; i++)
            {
                moves.Add($"{i},{i}");     // 우상
                moves.Add($"{i},-{i}");    // 우하
                moves.Add($"-{i},{i}");    // 좌상
                moves.Add($"-{i},-{i}");   // 좌하
            }
        }
        
        // 나이트 이동
        if (piece.movement.isKnight)
        {
            moves.AddRange(new string[] {
                "1,2", "2,1", "2,-1", "1,-2",
                "-1,-2", "-2,-1", "-2,1", "-1,2"
            });
        }
        
        // 폰 이동
        if (piece.movement.isPawn)
        {
            moves.Add("0,1");  // 전진
            moves.Add("1,1");  // 대각선 공격
            moves.Add("-1,1"); // 대각선 공격
        }
        
        // 이동이 정의되지 않은 경우 기본 이동 추가
        if (moves.Count == 0)
        {
            moves.Add("0,1");  // 기본 전진
        }
        
        // 기물 정의 문자열 생성 (이동:가중치 형태)
        return string.Join("|", moves) + ":" + piece.value;
    }
} 