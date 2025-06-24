using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BoardSpawner))]
public class BoardSpawnerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        BoardSpawner spawner = (BoardSpawner)target;

        // 행, 열 입력
        spawner.rows = EditorGUILayout.IntField("Rows", spawner.rows);
        spawner.columns = EditorGUILayout.IntField("Columns", spawner.columns);

        // 배열 크기 자동 조정
        int size = spawner.rows * spawner.columns;
        if (spawner.piecePrefabs == null || spawner.piecePrefabs.Length != size)
        {
            var newArray = new GameObject[size];
            if (spawner.piecePrefabs != null)
            {
                for (int i = 0; i < Mathf.Min(spawner.piecePrefabs.Length, size); i++)
                    newArray[i] = spawner.piecePrefabs[i];
            }
            spawner.piecePrefabs = newArray;
        }

        // 2차원 그리드로 표시
        EditorGUILayout.LabelField("Piece Prefabs (Grid)", EditorStyles.boldLabel);
        for (int y = 0; y < spawner.rows; y++)
        {
            EditorGUILayout.BeginHorizontal();
            for (int x = 0; x < spawner.columns; x++)
            {
                int idx = y * spawner.columns + x;
                spawner.piecePrefabs[idx] = (GameObject)EditorGUILayout.ObjectField(
                    spawner.piecePrefabs[idx], typeof(GameObject), false, GUILayout.Width(60));
            }
            EditorGUILayout.EndHorizontal();
        }

        // gridParent 필드
        spawner.gridParent = (Transform)EditorGUILayout.ObjectField("Grid Parent", spawner.gridParent, typeof(Transform), true);

        // Spawn 버튼
        if (GUILayout.Button("체스판 배치"))
        {
            spawner.SpawnGrid();
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(spawner);
        }
    }
}