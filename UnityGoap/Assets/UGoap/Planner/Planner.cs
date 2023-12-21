using System;
using System.Collections.Generic;
using GoapTFG.Base;
using GoapTFG.UGoap;
using UnityEngine;

namespace GoapTFG.Planner
{
    public abstract class Planner<TKey, TValue>
    {
        protected Node<TKey, TValue> _current;
        protected GoapGoal<TKey, TValue> _goal;
        protected INodeGenerator<TKey, TValue> _nodeGenerator;

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
                var actionData = new GoapActionData<TKey, TValue>(nodeGoal.Action, nodeGoal.ProceduralEffects);
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
    }
}