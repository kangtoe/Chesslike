using System.Collections.Generic;
using UnityEngine;
using Debug = UnityEngine.Debug;
using System.IO;

public class FairyStockfishIniGenerator : MonoBehaviour
{    
    [SerializeField] List<PieceInfo> customPieces = new List<PieceInfo>();

    void OnValidate()
    {
        // 중복 제거
        CheckPieceValidity();
        UpdateIniFile();
    }

    [ExecuteInEditMode]
    void Start()
    {
        // 중복 제거
        CheckPieceValidity();
        UpdateIniFile();
    }

    /// <summary>
    /// customPieces 리스트에서 중복된 기물을 제거합니다.
    /// </summary>
    void CheckPieceValidity()
    {
        if (customPieces == null) return;

        HashSet<PieceInfo> uniquePieces = new HashSet<PieceInfo>();
        HashSet<char> uniqueAlphabets = new HashSet<char>();

        foreach (var piece in customPieces)
        {
            if (piece == null) continue;

            // 움직임이 하나도 정의되지 않은 경우 체크 (에러로 출력, 제거하지 않음)
            var m = piece.movement;
            bool hasNoMove = !m.isRookMove && !m.isBishopMove && !m.isKnightMove && !m.isPawnMove && !m.isKingMove;
            if (hasNoMove)
            {
                Debug.LogError($"움직임이 정의되지 않은 기물이 존재합니다: {piece.pieceName} ({piece.pieceAlphabet})");
            }

            // PieceInfo 중복 체크
            if (!uniquePieces.Add(piece))
            {
                Debug.LogError($"중복된 기물(PieceInfo)이 존재합니다: {piece.pieceName} ({piece.pieceAlphabet})");
            }
            // pieceAlphabet 중복 체크
            if (!uniqueAlphabets.Add(piece.pieceAlphabet))
            {
                Debug.LogError($"중복된 기물 문자(pieceAlphabet)가 존재합니다: {piece.pieceName} ({piece.pieceAlphabet})");
            }
        }
    }

    /// <summary>
    /// PieceInfo를 기반으로 Fairy-Stockfish 기물 정의 문자열을 생성합니다.
    /// </summary>
    string GeneratePieceDefinition(PieceInfo piece)
    {
        CheckPieceValidity();

        List<string> types = new List<string>();
        if (piece.movement.isRookMove) types.Add("R");
        if (piece.movement.isBishopMove) types.Add("B");
        if (piece.movement.isKnightMove) types.Add("N");
        if (piece.movement.isKingMove) types.Add("K");
        if (piece.movement.isPawnMove) types.Add("P");

        string moveDef = string.Join("", types); // 콤마 없이 붙임 (BN, RN 등)
        return $"{piece.pieceAlphabet}:{piece.value}:{moveDef}";
    }

    /// <summary>
    /// 커스텀 기물 정보를 Fairy-Stockfish ini 파일에 반영합니다.
    /// </summary>
    /// <param name="iniFilePath">ini 파일 경로</param>
    //[NaughtyAttributes.Button]
    public void UpdateIniFile()
    {
        string iniFilePath = Path.Combine(Application.streamingAssetsPath, "variants.ini");
        
        List<string> lines = new List<string>();
        
        // [chesslike:chess] 섹션 추가
        lines.Add("[chesslike]");
        
        // 커스텀 피스들 추가
        for (int i = 0; i < customPieces.Count; i++)
        {
            if (customPieces[i] != null)
            {
                string definition = GeneratePieceDefinition(customPieces[i]);
                string customPieceLine = $"customPiece{i + 1} = {definition}";
                lines.Add(customPieceLine);
            }
        }

        // 파일에 쓰기
        File.WriteAllLines(iniFilePath, lines);
        
        Debug.Log($"ini 파일이 새로 생성되었습니다. {customPieces.Count}개의 커스텀 피스가 추가되었습니다.");
    }
} 