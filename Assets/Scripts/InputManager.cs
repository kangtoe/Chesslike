using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class InputManager : MonoSingleton<InputManager>
{
    DeployedPiece _hoveredPiece;

    void Update()
    {
        // 플레이어 턴이 아니면 입력 무시
        if(!TurnManager.Instance.IsPlayerTurn) return;

        // 이동 애니메이션 중이면 입력 무시
        if(PieceManager.Instance.IsMoving) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // 마우스 오버 피스 확인
        if (Physics.Raycast(ray, out hit))
        {
            var piece = hit.collider.GetComponent<DeployedPiece>();
            MouseHoverPiece(piece);
        }
        else
        {
            // 마우스가 아무것도 가리키지 않을 때 호버 해제
            MouseHoverPiece(null);
        }    

        // 마우스 왼쪽 클릭
        if (Input.GetMouseButtonDown(0)) 
        {
            OnMouseClick(hit);
        }

        // 마우스 오른쪽 클릭 (소환 모드 해제용)
        if (Input.GetMouseButtonDown(1))
        {
            OnRightMouseClick();
        }
    }

    void MouseHoverPiece(DeployedPiece newHoveredPiece)
    {
        if(_hoveredPiece != newHoveredPiece) 
        {
            // 이전에 호버된 피스의 크기를 원래대로 되돌림
            if(_hoveredPiece != null)
            {
                _hoveredPiece.transform.localScale = Vector3.one;
            }

            // 새로운 피스에 호버 효과 적용
            if(newHoveredPiece != null)
            {
                newHoveredPiece.transform.localScale = Vector3.one * 1.1f;
            }

            // 호버된 피스 업데이트
            _hoveredPiece = newHoveredPiece;
        }
    }

    void OnRightMouseClick()
    {
        // 소환 모드인 경우 선택 해제
        if (SummonManager.Instance.HasSelectedPiece)
        {
            SummonManager.Instance.OnRightClick();
            return;
        }

        // 일반 모드에서는 기물 선택 해제
        PieceManager.Instance.DeselectPiece();
    }

    void OnMouseClick(RaycastHit hit)
    {
        // 소환 모드 확인 - 선택된 소환 기물이 있는 경우
        if (SummonManager.Instance.HasSelectedPiece)
        {
            HandleSummonModeClick(hit);
            return;
        }

        // 일반 기물 이동 모드
        HandleNormalModeClick(hit);
    }

    /// <summary>
    /// 소환 모드에서의 클릭 처리
    /// </summary>
    void HandleSummonModeClick(RaycastHit hit)
    {
        Vector2Int? clickedPosition = InputUtil.GetBoardCoordinateFromHit(hit);
        
        if (clickedPosition.HasValue)
        {
            // 유효한 보드 위치 클릭 - 소환 시도
            SummonManager.Instance.OnBoardClick(clickedPosition.Value);
        }
        else
        {
            // 보드 밖 클릭 - 소환 모드 해제
            SummonManager.Instance.OnRightClick();
        }
    }

    /// <summary>
    /// 일반 모드에서의 클릭 처리 (기존 로직)
    /// </summary>
    void HandleNormalModeClick(RaycastHit hit)
    {
        if(hit.collider != null)
        {
            var selectedPiece = PieceManager.Instance.SelectedPiece;

            // Piece 클릭 판별
            var piece = hit.collider.GetComponent<DeployedPiece>();
            if (piece != null)
            {
                Debug.Log($"기물을 클릭했습니다: {piece.PieceInfo.pieceName}, 좌표: ({piece.CellCoordinate})");

                PieceManager.Instance.SelectPiece(piece);
                return;
            }

            if(selectedPiece != null)
            {
                // 클릭한 위치의 보드 좌표 가져오기
                Vector2Int? clickedPosition = InputUtil.GetBoardCoordinateFromHit(hit);
                if (clickedPosition.HasValue)
                {
                    Debug.Log($"보드 셀 클릭: 좌표=({clickedPosition.Value.x}, {clickedPosition.Value.y})");                        
                    bool isMoved = PieceManager.Instance.MovePiece(clickedPosition.Value);
                    if(!isMoved)
                    {
                        PieceManager.Instance.DeselectPiece();
                    }
                    return;
                }            
            }
        }       

        // 피스 선택 해제
        PieceManager.Instance.DeselectPiece();    
    }
}



