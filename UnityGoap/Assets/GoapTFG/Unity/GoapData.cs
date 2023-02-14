using System;
using System.Collections.Generic;
using GoapTFG.Base;
using GoapTFG.Planner;
using GoapTFG.Unity.ScriptableObjects;
using UnityEngine;
using static GoapTFG.Unity.PropertyManager;
using static GoapTFG.Unity.PropertyManager.PropertyList;

namespace GoapTFG.Unity
{
    public class GoapData : MonoBehaviour
    {
        public class ActionAdditionalData
        {
            public Func<IAgent<PropertyList, object>, PropertyGroup<PropertyList, object>, int> customCost;
            public Base.Action<PropertyList, object>.Condition conditions;
            public Base.Action<PropertyList, object>.Effect effects;
            public Base.Action<PropertyList, object>.Effect actions;
        }

        //Singleton
        public static GoapData GoapDataInstance;

        //Propiedades
        public GameObject[] BlackboardObjects;

        //Evaluaciones de comparación.
        //TO DO

        //Acciones
        public Dictionary<string, ActionAdditionalData> ActionAdditionalDatas;

        //Datos
        public StateScriptableObject initialState;
        public PropertyGroup<PropertyList, object> actualState;


        [ContextMenu("Reset State")]
        void Awake()
        {
            //Singletone
            if (GoapDataInstance && GoapDataInstance != this)
            {
                Destroy(this);
                return;
            }

            GoapDataInstance = this;

            actualState = initialState.Create();

            foreach (var go in BlackboardObjects)
            {
                WorkingMemoryManager.Add(go);
            }

            //Actions Additional Data
            ActionAdditionalDatas = new Dictionary<string, ActionAdditionalData>();

            AddCustomCostToAction("GoTo", (agent, ws) =>
            {
                var agentPos = ((AgentUnity)agent).transform.position;
                var targetPos = WorkingMemoryManager.Get((string)ws.Get(Target)).Position;
                return (int)Vector3.Distance(agentPos, targetPos);
            });
            
            AddPerformedActionsToAction("GoTo", (agent, ws) =>
            {
                ((AgentUnity)agent).GoToTarget((string)ws.Get(Target));
            });

            AddConditionsToAction("GoIdle", (agent, ws) =>
                ((AgentUnity)agent).GetCurrentGoal().Name.Equals("Idleling"));
            
            AddPerformedActionsToAction("GoIdle", (agent, ws) =>
            {
                ((AgentUnity)agent).GoIdleling(10);
                ws.Set(IsIdle, false);
            });
            

            /*AddEffectsToAction("Buy Stone", (ws) =>
            {
                var initialGold = (float)ws.Get(GoldCount.ToString());
                var initialStone = (int)ws.Get(StoneCount.ToString());
                var num = (int)initialGold / 70;
                float mod = initialGold % 70;
                ws.Set(GoldCount.ToString(), mod);
                ws.Set(StoneCount.ToString(), initialStone + 200 * num);
            });
                    
            AddEffectsToAction("Chop Trees", (ws) =>
            {
                var initialStone = (int)ws.Get(StoneCount.ToString());
                var initialWood = (int)ws.Get(WoodCount.ToString());
                var num = initialStone / 500;
                var mod = initialStone % 500;
                ws.Set(StoneCount.ToString(), mod);
                ws.Set(WoodCount.ToString(), initialWood + 150 * num);
            });*/
        }

        //Actions Additional Data Usages
        public static ActionAdditionalData GetActionAdditionalData(string key)
        {
            if (!GoapDataInstance.ActionAdditionalDatas.ContainsKey(key)) return null;
            return GoapDataInstance.ActionAdditionalDatas[key];
        }

        public static void AddCustomCostToAction(string key,
            Func<IAgent<PropertyList, object>, PropertyGroup<PropertyList, object>, int> customCost)
        {
            ActionAdditionalData aad = CreateAdditionalDataIfNeeded(key);
            aad.customCost = customCost;
            SaveAdditionalData(key, aad);
        }
        
        public static void AddConditionsToAction(string key, Base.Action<PropertyList, object>.Condition condition)
        {
            ActionAdditionalData aad = CreateAdditionalDataIfNeeded(key);
            aad.conditions += condition;
            SaveAdditionalData(key, aad);
        }

        public static void AddEffectsToAction(string key, Base.Action<PropertyList, object>.Effect effect)
        {
            ActionAdditionalData aad = CreateAdditionalDataIfNeeded(key);
            aad.effects += effect;
            SaveAdditionalData(key, aad);
        }

        public static void AddPerformedActionsToAction(string key, Base.Action<PropertyList, object>.Effect action)
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
}
