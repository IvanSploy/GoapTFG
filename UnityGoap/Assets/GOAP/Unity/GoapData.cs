using System.Collections;
using System.Collections.Generic;
using GoapTFG.Base;
using GoapTFG.Unity;
using UnityEngine;
using static GoapTFG.Unity.PropertyManager.PropertyList;

public class ActionAdditionalData
{
    public Action<string, object>.Condition conditions;
    public Action<string, object>.Effect effects;
    public Action<string, object>.Effect actions;
}

public class GoapData : MonoBehaviour
{
    //Singleton
    public static GoapData GoapDataInstance;
    
    //Propiedades
    public GameObject[] BlackboardObjects;
    
    //Evaluaciones de comparación.
    //TO DO
    
    //Acciones
    public Dictionary<string, ActionAdditionalData> ActionAdditionalDatas;
    
    //Datos
    public GoapScriptableObject initialState;
    public PropertyGroup<string, object> actualState;


    void Awake()
    {
        //Singletone
        if (GoapDataInstance && GoapDataInstance != this)
        {
            Destroy(this);
            return;
        }
        GoapDataInstance = this;
        
        actualState = initialState.GetState();

        foreach (var go in BlackboardObjects)
        {
            WorkingMemoryManager.Add(go);
        }
        
        //Actions Additional Data
        ActionAdditionalDatas = new Dictionary<string, ActionAdditionalData>();

        //AddEffectsToAction("Pick Gold", (pg) => pg.Set(GoldCount.ToString(), (float)pg.Get(GoldCount.ToString()) + 100f));

    }

    
    //Actions Additional Data Usages
    public static ActionAdditionalData GetActionAdditionalData(string key)
    {
        if (!GoapDataInstance.ActionAdditionalDatas.ContainsKey(key)) return null;
        return GoapDataInstance.ActionAdditionalDatas[key];
    }

    public static void AddConditionsToAction(string key, Action<string, object>.Condition condition)
    {
        ActionAdditionalData aad = CreateAdditionalDataIfNeeded(key);
        aad.conditions += condition;
        SaveAdditionalData(key, aad);
    }

    public static void AddEffectsToAction(string key, Action<string, object>.Effect effect)
    {
        ActionAdditionalData aad = CreateAdditionalDataIfNeeded(key);
        aad.effects += effect;
        SaveAdditionalData(key, aad);
    }

    public static void AddPerformedActionsToAction(string key, Action<string, object>.Effect action)
    {
        ActionAdditionalData aad = CreateAdditionalDataIfNeeded(key);
        aad.actions += action;
        SaveAdditionalData(key, aad);
    }

    private static ActionAdditionalData CreateAdditionalDataIfNeeded(string key)
    {
        ActionAdditionalData aad; 
        bool hasdata = GoapDataInstance.ActionAdditionalDatas.TryGetValue(key, out aad);
        if (!hasdata) aad = new ActionAdditionalData();
        return aad;
    }
    
    private static void SaveAdditionalData(string key, ActionAdditionalData data)
    {
        GoapDataInstance.ActionAdditionalDatas[key] = data;
    }
}
