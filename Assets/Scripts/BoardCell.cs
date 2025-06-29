using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class BoardCell : MonoBehaviour
{
    [SerializeField] GameObject moveIndicator;

    private Renderer cellRenderer;
    public Renderer CellRenderer
    {
        get
        {
            if (cellRenderer == null)
                cellRenderer = GetComponent<Renderer>();
            return cellRenderer;
        }
    }

    public void SetColor(Color color)
    {
        if(CellRenderer) CellRenderer.material.color = color;
    }
    
    public void ToggleMoveIndicator(bool isActive)
    {
        if(moveIndicator) moveIndicator.SetActive(isActive);
    }
}
