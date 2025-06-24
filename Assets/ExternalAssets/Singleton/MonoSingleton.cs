using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static object lockObject = new object();
    static T instance = null;

    public static bool has_instance => instance != null;

    public static T Instance
    {        
        get
        {            
            lock (lockObject) // for Thread-Safe
            {                
                if (!has_instance)
                {
                    instance = FindObjectOfType<T>();
                    
                    if (!has_instance)
                    {
                        GameObject go = new GameObject();
                        go.name = typeof(T).ToString();
                        instance = go.AddComponent<T>();                        
                    }                    
                }
                return instance;
            }
        }
    }
}