using System.Collections.Generic;
using GoapTFG.Base;
using GoapTFG.UGoap;
using UnityEngine;

namespace GoapTFG.Planner
{
    public abstract class Planner<TKey, TValue>
    {
        //Stats
        protected static int nodesCreated = 0;
        protected static int nodesSkipped = 0;
        
        //Data
        protected INodeGenerator<TKey, TValue> _nodeGenerator;
        protected Node<TKey, TValue> _current;
        protected GoapGoal<TKey, TValue> _goal;

        protected Planner(GoapGoal<TKey, TValue> goal,
            INodeGenerator<TKey, TValue> nodeGenerator)
        {
            _goal = goal;
            _nodeGenerator = nodeGenerator;
        }

        /// <summary>
        /// Generates the plan using the generator and the actions provided.
        /// </summary>
        /// <param name="initialState"></param>
        /// <param name="actions"></param>
        /// <returns></returns>
        public abstract Stack<GoapActionData<TKey, TValue>> GeneratePlan(PropertyGroup<TKey, TValue> initialState,
            List<IGoapAction<TKey, TValue>> actions);
        
        /// <summary>
        /// Gets the final plan that the researcher has found.
        /// </summary>
        /// <param name="nodeGoal">Objective node</param>
        /// <returns>Stack of actions.</returns>
        public static Stack<GoapActionData<TKey, TValue>> GetPlan(Node<TKey, TValue> nodeGoal)
        {
            Stack<GoapActionData<TKey, TValue>> plan = new Stack<GoapActionData<TKey, TValue>>();
            while (nodeGoal.Parent != null)
            {
                
                //Debug.Log("Estado: " + nodeGoal.State + "| Goal: " + nodeGoal.Goal);
                var actionData = new GoapActionData<TKey, TValue>(nodeGoal.Action, nodeGoal.Parent.Goal, nodeGoal.ProceduralEffects);
                plan.Push(actionData);
                nodeGoal = nodeGoal.Parent;
            }
            return plan;
        }
        
        /// <summary>
        /// Gets the final inverted plan that the researcher has found.
        /// </summary>
        /// <param name="nodeGoal"></param>
        /// <returns></returns>
        public static Stack<GoapActionData<TKey, TValue>> GetInvertedPlan(Node<TKey, TValue> nodeGoal)
        {
            Stack<GoapActionData<TKey, TValue>> plan = GetPlan(nodeGoal);
            Stack<GoapActionData<TKey, TValue>> invertedPlan = new Stack<GoapActionData<TKey, TValue>>();
            foreach (var actionData in plan)
            {
                invertedPlan.Push(actionData);
            }
            return invertedPlan;
        }

        public static void DebugPlan(Node<TKey, TValue> node)
        {
            var debugLog = "Acciones para conseguir el objetivo: ";
            var actionNames = "";
            int count = 0;
            int cost = node.TotalCost;
            
            while (node.Parent != null)
            {
                actionNames += node.Action.Name + "\n";
                count++;
                node = node.Parent;
            }

            debugLog += count + "\n";
            debugLog += "con coste: " + cost + "\n";
            debugLog += actionNames + "\n";
            
            Debug.Log(debugLog);
        }
        
        protected void DebugInfo(Node<TKey, TValue> node)
        {
            string info = "";
            info += "NODOS CREADOS: " + nodesCreated + "\n";
            info += "NODOS SALTADOS: " + nodesSkipped + "\n";
            info += "ACCIONES RECORRIDAS: " + UGoapAction.actionsApplied + "\n";
            Debug.Log(info);
            UGoapAction.actionsApplied = 0;
            DebugPlan(node);
        }
    }
}