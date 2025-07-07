using System.Diagnostics;
using System.IO;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class StockfishConnector : MonoBehaviour
{
    private Process stockfishProcess;
    private StreamWriter stockfishInput;
    private StreamReader stockfishOutput;

    void Start()
    {
        StartStockfish();

        SendCommand("uci");
        SendCommand("isready");
        ReadUntil("readyok");

        // 예시: 수를 지정하고 응답받기
        SendCommand("position startpos moves e2e4 e7e5");
        SendCommand("go depth 10");
        ReadUntil("bestmove");
    }

    void StartStockfish()
    {
        string exePath = Path.Combine(Application.streamingAssetsPath, "fairy-stockfish_x86-64.exe");

        stockfishProcess = new Process();
        stockfishProcess.StartInfo.FileName = exePath;
        stockfishProcess.StartInfo.UseShellExecute = false;
        stockfishProcess.StartInfo.RedirectStandardInput = true;
        stockfishProcess.StartInfo.RedirectStandardOutput = true;
        stockfishProcess.StartInfo.CreateNoWindow = true;
        stockfishProcess.Start();

        stockfishInput = stockfishProcess.StandardInput;
        stockfishOutput = stockfishProcess.StandardOutput;

        Debug.Log("Fairy-Stockfish 실행됨.");
    }

    void SendCommand(string command)
    {
        stockfishInput.WriteLine(command);
        stockfishInput.Flush();
        Debug.Log("[Sent] " + command);
    }

    void ReadUntil(string keyword)
    {
        string line;
        while ((line = stockfishOutput.ReadLine()) != null)
        {
            Debug.Log("[Recv] " + line);
            if (line.Contains(keyword)) break;
        }
    }

    void OnApplicationQuit()
    {
        stockfishInput?.WriteLine("quit");
        stockfishProcess?.Close();
    }
}
