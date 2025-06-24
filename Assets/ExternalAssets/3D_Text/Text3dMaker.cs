using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Text3dMaker : MonoSingleton<Text3dMaker>
{
    [SerializeField]
    GameObject textPrefab;

    public GameObject MakeText(string str, Vector3 pos, Color color, Transform preant = null)
    {
        //Quaternion quat = Camera.main.transform.rotation;
        //Quaternion quat = textPrefab.transform.rotation;

        GameObject go = Instantiate(textPrefab, preant);
        go.transform.localPosition = pos;
        go.GetComponent<TextMesh>().text = str;
        go.GetComponent<TextMesh>().color = color;

        // 뛰어오르는 물리 효과 추가
        {
            //Vector3 vec = Random.onUnitSphere * 5;
            //if (vec.y < 0) vec.y *= -1;
            //vec += Vector3.up * 1.5f;
            // 중력 활성화 할것
            //go.GetComponent<Rigidbody>().AddForce(vec, ForceMode.Impulse);
        }        

        return go;
    }
}
