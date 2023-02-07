using System.Collections;
using System.Collections.Generic;
using GoapTFG.Base;
using UnityEngine;

public class GoapData : MonoBehaviour
{
    //Singleton
    public static GoapData Instance;
    
    //Datos
    public PropertyManager.Property[] InitialState;
    public PropertyGroup<string, object> actualState;

    void Awake()
    {
        if (Instance && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
        actualState = new PropertyGroup<string, object>();
        foreach (var property in InitialState)
        {
            PropertyManager.ApplyProperty(property, ref actualState);
        }
    }
}
