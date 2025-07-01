using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] Camera _camera => Camera.main;
    
    [Header("카메라 설정")]    
    [SerializeField] Vector3 lookAtOffset = new Vector3(0, 0, 0);
    [SerializeField] Vector3 cameraOffset = new Vector3(0, -6, -10);
    [SerializeField] Vector3 allOffset = new Vector3(0, 0, 0);
    [SerializeField] float smoothSpeed = 5f;
    [SerializeField] float fov = 60f;
    
    Vector3 currentVelocity = Vector3.zero;

    Vector3 lookAtPosition => BoardManager.Instance.BoardCenter + lookAtOffset + allOffset;
    Vector3 cameraPosition => BoardManager.Instance.BoardCenter + cameraOffset + allOffset;

    void Update()
    {
        if (BoardManager.Instance == null) return;
        
        // 목표 위치 계산
        Vector3 targetPosition = cameraPosition;        
        
        // 부드러운 이동
        Vector3 smoothedPosition = Vector3.SmoothDamp(_camera.transform.position, targetPosition, ref currentVelocity, 1f / smoothSpeed);
        _camera.transform.position = smoothedPosition;
        
        // 보드 중앙을 바라보기
        _camera.transform.LookAt(lookAtPosition);
        
        // FOV 설정
        _camera.fieldOfView = fov;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(lookAtPosition, 0.2f);

        GUIStyle style = new GUIStyle();
        style.alignment = TextAnchor.MiddleCenter;
        style.fontSize = 18;
        style.normal.textColor = Color.magenta;
        
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(lookAtPosition + Vector3.up * 0.3f, "lookAtPosition", style);
        #endif
    }
}
