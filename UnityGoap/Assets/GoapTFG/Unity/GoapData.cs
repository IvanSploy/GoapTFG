using System;
using System.Collections.Generic;
using UnityEngine;
using GoapTFG.Base;
using GoapTFG.Planner;
using GoapTFG.Unity.ScriptableObjects;
using GoapTFG.Unity.CodeGenerator.Enums;
using static GoapTFG.Unity.PropertyManager;
using static GoapTFG.Unity.PropertyManager.PropertyList;
using static GoapTFG.Unity.PropertyManager.PropertyType;
using static GoapTFG.Unity.CodeGenerator.Enums.GoalName;
using static GoapTFG.Unity.CodeGenerator.Enums.ActionName;

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

            AddPerformedActionsToAction(GoTo, (agent, ws) =>
            {
                ((AgentUnity)agent).GoToTarget((string)ws.Get(Target));
            });

            AddConditionsToAction(GoIdle, (agent, ws) =>
                ((AgentUnity)agent).GetCurrentGoal().Name.Equals(Idleling.ToString()));
            
            AddPerformedActionsToAction(GoIdle, (agent, ws) =>
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

            return;
            AddCustomCostToAction(GoTo, (agent, ws) =>
            {
                var agentPos = ((AgentUnity)agent).transform.position;
                var targetPos = WorkingMemoryManager.Get((string)ws.Get(Target)).Position;
                return (int)Vector3.Distance(agentPos, targetPos);
            });
        }

        //Actions Additional Data Usages
        public static void AddCustomCostToAction(ActionName key,
            Func<IAgent<PropertyList, object>, PropertyGroup<PropertyList, object>, int> customCost)
        {
            string sKey = key.ToString();
            ActionAdditionalData aad = CreateAdditionalDataIfNeeded(sKey);
            aad.customCost = customCost;
            SaveAdditionalData(sKey, aad);
        }
        
        public static void AddConditionsToAction(ActionName key, Base.Action<PropertyList, object>.Condition condition)
        {
            string sKey = key.ToString();
            ActionAdditionalData aad = CreateAdditionalDataIfNeeded(sKey);
            aad.conditions += condition;
            SaveAdditionalData(sKey, aad);
        }

        public static void AddEffectsToAction(ActionName key, Base.Action<PropertyList, object>.Effect effect)
        {
            string sKey = key.ToString();
            ActionAdditionalData aad = CreateAdditionalDataIfNeeded(sKey);
            aad.effects += effect;
            SaveAdditionalData(sKey, aad);
        }

        public static void AddPerformedActionsToAction(ActionName key, Base.Action<PropertyList, object>.Effect action)
        {
            string sKey = key.ToString();
            ActionAdditionalData aad = CreateAdditionalDataIfNeeded(sKey);
            aad.actions += action;
            SaveAdditionalData(sKey, aad);
        }
        
        public static ActionAdditionalData GetActionAdditionalData(string key)
        {
            if (!GoapDataInstance.ActionAdditionalDatas.ContainsKey(key)) return null;
            return GoapDataInstance.ActionAdditionalDatas[key];
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
        
        /// <summary>
        /// User defined heuristic for GOAP.
        /// </summary>
        /// <returns></returns>
        public static Func<Goal<PropertyList, object>, PropertyGroup<PropertyList, object>, int> GetCustomHeuristic()
        {
            return null;
            return (goal, worldState) =>
            {
                var heuristic = 0;
                foreach (var name in goal.GetState().GetKeys())
                {
                    if(!worldState.HasConflict(name, goal.GetState())) continue;
                    switch (PropertyManager.GetType(name))
                    {
                        case Integer:
                            if (worldState.HasKey(name))
                                heuristic += Math.Abs((int)goal.GetState().Get(name) - (int)worldState.Get(name));
                            else heuristic += (int)goal.GetState().Get(name);
                            break;
                        case Float:
                            if (worldState.HasKey(name))
                                heuristic += (int)Mathf.Abs((float)goal.GetState().Get(name) - (float)worldState.Get(name));
                            else heuristic += (int)goal.GetState().Get(name);
                            break;
                        default:
                            if (!worldState.HasKey(name) || !goal.GetState().Get(name).Equals(worldState.Get(name))) 
                                heuristic += 1;
                            break;
                    }
                }
                return heuristic;
            };
        }
    }
}
