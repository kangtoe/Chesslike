using System.Diagnostics;
using System.IO;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class StockfishConnector : MonoBehaviour
{
    private Process stockfishProcess;
    private StreamWriter stockfishInput;
    private StreamReader stockfishOutput;

    #region stockfish utility

    public void StartStockfish()
    {        
        string exePath = Path.Combine(Application.streamingAssetsPath, "fairy-stockfish_x86-64.exe");

        stockfishProcess = new Process();
        stockfishProcess.StartInfo.FileName = exePath;
        stockfishProcess.StartInfo.Arguments = "load variants.ini";
        stockfishProcess.StartInfo.UseShellExecute = false;
        stockfishProcess.StartInfo.RedirectStandardInput = true;
        stockfishProcess.StartInfo.RedirectStandardOutput = true;
        stockfishProcess.StartInfo.CreateNoWindow = true;
        stockfishProcess.StartInfo.WorkingDirectory = Application.streamingAssetsPath;
        stockfishProcess.Start();

        stockfishInput = stockfishProcess.StandardInput;
        stockfishOutput = stockfishProcess.StandardOutput;        

        SendCommand("uci");
        SendCommand("isready");
        ReadUntil("readyok");

        SendCommand($"setoption name UCI_Variant value chesslike");

        Debug.Log("Fairy-Stockfish 준비 완료 (사용자 정의 변형 로드됨).");
    }

    public void SendCommand(string command)
    {
        stockfishInput.WriteLine(command);
        stockfishInput.Flush();
        //Debug.Log("[Sent] " + command);
    }

    void ReadUntil(string keyword)
    {
        string line;
        while ((line = stockfishOutput.ReadLine()) != null)
        {
            //Debug.Log("[Recv] " + line);
            if (line.Contains(keyword)) break;
        }
    }

    void OnApplicationQuit()
    {
        stockfishInput?.WriteLine("quit");
        stockfishProcess?.Close();
    }

    #endregion stockfish utility

    // 완전한 FEN 문자열을 받아 엔진의 다음 bestmove를 구하는 함수
    public string GetBestMoveFromFEN(string fen, int depth = 10, PieceColor color = PieceColor.Black)
    {
        if(string.IsNullOrEmpty(fen))
        {
            Debug.LogError("FEN is empty");
            return null;
        }

        // FEN이 완전한 형태인지 확인하고, 필요하면 턴 정보 수정
        string processedFEN = ChessNotationUtil.ProcessFEN(fen, color);

        Debug.Log($"[Stockfish] Position FEN: {processedFEN}");
        SendCommand("position fen " + processedFEN);
        SendCommand("go depth " + depth);

        string bestMove = null;
        string line;
        while ((line = stockfishOutput.ReadLine()) != null)
        {
            // depth가 10인 라인만 디버깅 로그에 남기기
            if (line.StartsWith("info depth 10"))
            {
                Debug.Log($"[Recv] {line}");
            }
            
            if (line.StartsWith("bestmove"))
            {
                var parts = line.Split(' ');
                if (parts.Length >= 2)
                    bestMove = parts[1];
                break;
            }
        }
        
        Debug.Log($"[Stockfish] Best move: {bestMove}");
        return bestMove;
    }


}
