using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TurnManager : MonoSingleton<TurnManager>
{
    [NaughtyAttributes.ReadOnly]
    [SerializeField] PieceColor _currentTurn = PieceColor.White;
    public PieceColor CurrentTurn => _currentTurn;

    [SerializeField] int _turnCount = 0;
    public int TurnCount => _turnCount;

    public bool IsPlayerTurn => _currentTurn == PieceColor.White;
    
    // 턴 변경 이벤트
    public event Action<PieceColor> OnTurnChanged;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
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
    }
}
