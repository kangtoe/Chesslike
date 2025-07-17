using UnityEngine;

/// <summary>
/// 입력 처리 관련 유틸리티 메서드들을 제공하는 정적 클래스
/// </summary>
public static class InputUtil
{
    /// <summary>
    /// RaycastHit에서 보드 좌표를 가져오는 유틸리티 메서드
    /// BoardCell이나 DeployedPiece 컴포넌트에서 CellCoordinate를 추출합니다.
    /// </summary>
    /// <param name="hit">레이캐스트 히트 정보</param>
    /// <returns>보드 좌표 (없으면 null)</returns>
    public static Vector2Int? GetBoardCoordinateFromHit(RaycastHit hit)
    {
        if (hit.collider == null) return null;
        
        // BoardCell 체크
        var cell = hit.collider.GetComponent<BoardCell>();
        if (cell != null)
        {
            return cell.CellCoordinate;
        }
        
        // DeployedPiece 체크 (기물이 있는 셀도 유효한 위치)
        var piece = hit.collider.GetComponent<DeployedPiece>();
        if (piece != null)
        {
            return piece.CellCoordinate;
        }
        
        return null;
    }

    /// <summary>
    /// 스크린 위치에서 보드 셀 좌표를 가져오는 유틸리티 메서드
    /// </summary>
    /// <param name="screenPosition">스크린 위치</param>
    /// <param name="camera">사용할 카메라 (null이면 Camera.main 사용)</param>
    /// <returns>보드 셀 좌표 (없으면 null)</returns>
    public static Vector2Int? GetBoardCellFromScreenPosition(Vector2 screenPosition, Camera camera = null)
    {
        if (camera == null) camera = Camera.main;
        if (camera == null) return null;

        Ray ray = camera.ScreenPointToRay(screenPosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            return GetBoardCoordinateFromHit(hit);
        }

        return null;
    }
} 