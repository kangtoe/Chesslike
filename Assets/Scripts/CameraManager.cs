using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] Camera _camera;

    [SerializeField] float _angle = 0f;
    [SerializeField] Vector3 _offset = new Vector3(0, 0, -10);

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        _camera.transform.position = BoardManager.Instance.BoardCenter + _offset;
        _camera.transform.rotation = Quaternion.Euler(_angle, 0, 0);
    }
}
