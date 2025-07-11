using NaughtyAttributes;
using UnityEngine;
using Debug = UnityEngine.Debug;

[RequireComponent(typeof(StockfishConnector))]
[RequireComponent(typeof(CustomPieceRegister))]
public class AIManager : MonoSingleton<AIManager>
{
    [Header("AI Settings")]
    [SerializeField] private int defaultSearchDepth = 10;
    
    [Header("Components")]
    [ReadOnly] [SerializeField] StockfishConnector stockfishConnector;
    [ReadOnly] [SerializeField] CustomPieceRegister customPieceManager;
    
    void OnValidate()
    {    
        if (stockfishConnector == null)
            stockfishConnector = GetComponent<StockfishConnector>();
            
        if (customPieceManager == null)
            customPieceManager = GetComponent<CustomPieceRegister>();
    }

    void Start()
    {
        InitializeAI();
    }
    
    /// <summary>
    /// AI 시스템을 초기화합니다.
    /// </summary>
    public void InitializeAI()
    {
        Debug.Log("=== AI 시스템 초기화 시작 ===");
        
        if (stockfishConnector == null)
        {
            Debug.LogError("StockfishConnector를 찾을 수 없습니다.");
            return;
        }
        
        if (customPieceManager == null)
        {
            Debug.LogError("CustomPieceManager를 찾을 수 없습니다.");
            return;
        }
        
        stockfishConnector.StartStockfish();
        customPieceManager.RegisterCustomPieces(stockfishConnector);

        Debug.Log("=== AI 시스템 초기화 완료 ===");
    }
    
    /// <summary>
    /// FEN 문자열을 기반으로 최적의 이동을 계산합니다.
    /// </summary>
    public string GetBestMove(string fen, PieceColor color = PieceColor.Black)
    {
        if (stockfishConnector == null)
        {
            Debug.LogError("AI가 준비되지 않았습니다.");
            return null;
        }

        return stockfishConnector.GetBestMoveFromFEN(fen, defaultSearchDepth, color);
    }
} 