using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class InputManager : MonoSingleton<InputManager>
{
    DeployedPiece _hoveredPiece;

    void Update()
    {
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

    void OnMouseClick(RaycastHit hit)
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
                // BoardCell 클릭 판별
                var cell = hit.collider.GetComponent<BoardCell>();
                if (cell != null)
                {
                    Debug.Log($"보드 셀 클릭: 좌표=({cell.CellCoordinate.x}, {cell.CellCoordinate.y})");                        
                    bool isMoved = PieceManager.Instance.MovePiece(cell.CellCoordinate);
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



