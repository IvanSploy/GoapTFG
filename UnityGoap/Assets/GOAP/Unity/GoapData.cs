using System.Collections;
using System.Collections.Generic;
using GoapTFG.Base;
using UnityEngine;
using static PropertyManager.PropertyList;

public class GoapData : MonoBehaviour
{
    //Singleton
    public static GoapData GoapDataInstance;
    
    //Propiedades
    public PropertyManager.Property[] InitialState;
    public GameObject[] BlackboardObjects;
    
    //Datos
    public PropertyGroup<string, object> actualState;


    void Awake()
    {
        if (GoapDataInstance && GoapDataInstance != this)
        {
            Destroy(this);
            return;
        }

        GoapDataInstance = this;
        actualState = new PropertyGroup<string, object>();
        
        foreach (var property in InitialState)
        {
            PropertyManager.ApplyProperty(property, ref actualState);
        }

        foreach (var go in BlackboardObjects)
        {
            BlackboardData.Add(go);
        }
    }
}
