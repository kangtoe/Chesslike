using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 체스 게임에서 사용하는 유틸리티 메서드들을 모아놓은 클래스
/// </summary>
public static class ChessUtils
{
    /// <summary>
    /// 두 위치 간의 거리를 계산합니다 (맨해튼 거리)
    /// </summary>
    public static int GetManhattanDistance(Vector2Int from, Vector2Int to)
    {
        return Mathf.Abs(to.x - from.x) + Mathf.Abs(to.y - from.y);
    }

    /// <summary>
    /// 두 위치 간의 체스보드 거리를 계산합니다 (최대값)
    /// </summary>
    public static int GetChessboardDistance(Vector2Int from, Vector2Int to)
    {
        return Mathf.Max(Mathf.Abs(to.x - from.x), Mathf.Abs(to.y - from.y));
    }

    /// <summary>
    /// 주어진 방향이 대각선인지 확인합니다
    /// </summary>
    public static bool IsDiagonalDirection(Vector2Int direction)
    {
        return Mathf.Abs(direction.x) == Mathf.Abs(direction.y) && direction.x != 0;
    }

    /// <summary>
    /// 주어진 방향이 직선인지 확인합니다 (가로 또는 세로)
    /// </summary>
    public static bool IsStraightDirection(Vector2Int direction)
    {
        return direction.x == 0 || direction.y == 0;
    }

    /// <summary>
    /// 위치가 보드 범위 내에 있는지 확인합니다
    /// </summary>
    public static bool IsValidBoardPosition(Vector2Int position, Vector2Int boardSize)
    {
        return position.x >= 0 && position.x < boardSize.x && 
               position.y >= 0 && position.y < boardSize.y;
    }

    /// <summary>
    /// 주어진 방향으로 이동 가능한 모든 위치를 반환합니다
    /// </summary>
    public static List<Vector2Int> GetPositionsInDirection(Vector2Int startPos, Vector2Int direction, int maxDistance, Vector2Int boardSize)
    {
        List<Vector2Int> positions = new List<Vector2Int>();
        
        for (int i = 1; i <= maxDistance; i++)
        {
            Vector2Int targetPos = startPos + direction * i;
            
            if (!IsValidBoardPosition(targetPos, boardSize))
                break;
                
            positions.Add(targetPos);
        }
        
        return positions;
    }

    /// <summary>
    /// 월드 좌표를 보드 셀 좌표로 변환합니다
    /// </summary>
    public static Vector2Int WorldToCellPosition(Vector3 worldPosition, Bounds boardBounds, Vector2Int cellCount, float cellSize)
    {
        Vector3 relativePosition = worldPosition - boardBounds.center;
        
        int x = Mathf.FloorToInt(relativePosition.x / cellSize + cellCount.x * 0.5f);
        int y = Mathf.FloorToInt(relativePosition.y / cellSize + cellCount.y * 0.5f);
        
        x = Mathf.Clamp(x, 0, cellCount.x - 1);
        y = Mathf.Clamp(y, 0, cellCount.y - 1);
        
        return new Vector2Int(x, y);
    }

    /// <summary>
    /// 보드 셀 좌표를 월드 좌표로 변환합니다
    /// </summary>
    public static Vector3 CellToWorldPosition(Vector2Int cellPosition, Bounds boardBounds, Vector2Int cellCount, float cellSize)
    {
        Vector3 relativePosition = new Vector3(
            (cellPosition.x - (cellCount.x - 1) * 0.5f) * cellSize,
            (cellPosition.y - (cellCount.y - 1) * 0.5f) * cellSize,
            0
        );
        
        return boardBounds.center + relativePosition;
    }
} 