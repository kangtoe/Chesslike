using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoardSpawner : MonoBehaviour
{
    public int rows = 8;
    public int columns = 8;
    public Transform gridParent; // GridLayoutGroup이 붙은 오브젝트

    // [행, 열] 순서로 각 칸에 배치할 프리팹을 지정
    public GameObject[] piecePrefabs; // 크기: rows * columns

    // Start is called before the first frame update
    void Start()
    {
        SpawnGrid();
    }

    public void SpawnGrid()
    {
        // 기존 자식 오브젝트 삭제
        foreach (Transform child in gridParent)
            Destroy(child.gameObject);

        // 그리드 레이아웃 설정
        GridLayoutGroup grid = gridParent.GetComponent<GridLayoutGroup>();
        if (grid != null)
        {
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = columns;
        }

        // 각 칸마다 지정된 프리팹 배치
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                int idx = y * columns + x;
                GameObject prefab = piecePrefabs[idx];
                if (prefab != null)
                    Instantiate(prefab, gridParent);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
