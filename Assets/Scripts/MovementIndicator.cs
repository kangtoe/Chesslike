using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementIndicator : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    
    void Awake()
    {
        // SpriteRenderer가 없으면 자동으로 찾기
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // 인디케이터의 색상을 설정하는 메서드
    public void SetColor(Color color)
    {
        if (spriteRenderer != null)
            spriteRenderer.color = color;
    }

    // 인디케이터를 활성화/비활성화하는 메서드
    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }
}
