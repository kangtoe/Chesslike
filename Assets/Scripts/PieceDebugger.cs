using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PieceDebugger : MonoBehaviour
{
    [Header("Print FEN")]
    [NaughtyAttributes.ReadOnly]
    [SerializeField] string fen = "SHOULD DISPLAY FEN";

    [Header("Print Best Move")]
    [NaughtyAttributes.ReadOnly]
    [SerializeField] string white_bestMove = "SHOULD DISPLAY WHITE BEST MOVE";
    [NaughtyAttributes.ReadOnly]
    [SerializeField] string black_bestMove = "SHOULD DISPLAY BLACK BEST MOVE";

    [Header("Deploy Piece")]
    [SerializeField] PieceColor _pieceColor = PieceColor.White;
    [SerializeField] PieceInfo deploy_pieceInfo;    
    [SerializeField] Vector2Int deploy_coordinate;

    void Update()
    {
        #if UNITY_EDITOR
        PrintFEN();
        PrintBestMove();
        #endif
    }

    [NaughtyAttributes.Button]
    void DeployPiece()
    {        
        PieceManager.Instance.DeployPiece(deploy_pieceInfo, deploy_coordinate, _pieceColor);
    }

    [NaughtyAttributes.Button]
    void ClearAllPieces()
    {
        // 딕셔너리의 키들을 리스트로 변환하여 역순으로 처리
        List<Vector2Int> coordinates = new List<Vector2Int>(PieceManager.Instance.DeployedPieces.Keys);
        for(int i = coordinates.Count - 1; i >= 0; i--)
        {
            PieceManager.Instance.RemovePiece(coordinates[i]);
        }
        Debug.Log("모든 피스가 제거되었습니다.");

        BoardManager.Instance.ActiveMoveIndicator(null);
    }
    
    void PrintFEN()
    {        
        if(!PieceManager.Instance || PieceManager.Instance.DeployedPieces.Count == 0)
        {
            //Debug.Log("배치된 피스가 없습니다.");
            return;
        }

        Debug.Log("=== 보드 상태 ===");

        foreach(var kvp in PieceManager.Instance.DeployedPieces)
        {
            Vector2Int coordinate = kvp.Key;
            DeployedPiece piece = kvp.Value;
            Debug.Log($"좌표 {coordinate}: {piece.name}");
        }
        Debug.Log("================");

        fen = PieceManager.Instance.GetFENFromCurrentBoard();
        //Debug.Log($"FEN: {fen}");
    }
    
    void PrintBestMove()
    {        
        if(!PieceManager.Instance || PieceManager.Instance.DeployedPieces.Count == 0)
        {
            //Debug.Log("배치된 피스가 없습니다.");
            return;
        }

        Debug.Log("=== 최적 이동 ===");

        string piecePlacement = PieceManager.Instance.GetFENFromCurrentBoard();
        white_bestMove = AIManager.Instance.GetBestMove(piecePlacement, PieceColor.White);
        black_bestMove = AIManager.Instance.GetBestMove(piecePlacement, PieceColor.Black);
        //Debug.Log("White Best move: " + white_bestMove);
        //Debug.Log("Black Best move: " + black_bestMove);
    }
}
