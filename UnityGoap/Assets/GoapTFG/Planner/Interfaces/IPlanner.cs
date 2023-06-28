using System;
using System.Collections.Generic;
using GoapTFG.Base;

namespace GoapTFG.Planner
{
    public interface IPlanner<TKey, TValue>
    {
        /// <summary>
        /// Generates the plan using the generator and the actions provided.
        /// </summary>
        /// <param name="initialState"></param>
        /// <param name="actions"></param>
        /// <returns></returns>
        Stack<IGoapAction<TKey, TValue>> GeneratePlan(PropertyGroup<TKey, TValue> initialState,
            List<IGoapAction<TKey, TValue>> actions);
        
        /// <summary>
        /// Gets the final plan that the researcher has found.
        /// </summary>
        /// <param name="nodeGoal">Objective node</param>
        /// <returns>Stack of actions.</returns>
        public static Stack<IGoapAction<TKey, TValue>> GetPlan(Node<TKey, TValue> nodeGoal)
        {
            Stack<IGoapAction<TKey, TValue>> plan = new Stack<IGoapAction<TKey, TValue>>();
            while (nodeGoal.Parent != null)
            {
                plan.Push(nodeGoal.GoapAction);
                nodeGoal = nodeGoal.Parent;
            }
            return plan;
        }
        
        /// <summary>
        /// Gets the final inverted plan that the researcher has found.
        /// </summary>
        /// <param name="nodeGoal"></param>
        /// <returns></returns>
        public static Stack<IGoapAction<TKey, TValue>> GetInvertedPlan(Node<TKey, TValue> nodeGoal)
        {
            Stack<IGoapAction<TKey, TValue>> plan = GetPlan(nodeGoal);
            Stack<IGoapAction<TKey, TValue>> invertedPlan = new Stack<IGoapAction<TKey, TValue>>();
            foreach (var action in plan)
            {
                invertedPlan.Push(action);
            }
            return invertedPlan;
        }
    }
}