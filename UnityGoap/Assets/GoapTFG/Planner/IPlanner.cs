using System;
using System.Collections.Generic;
using GoapTFG.Base;

namespace GoapTFG.Planner
{
    public interface IPlanner<TA, TB>
    {
        /// <summary>
        /// Generates the plan using the generator and the actions provided.
        /// </summary>
        /// <param name="initialState"></param>
        /// <param name="actions"></param>
        /// <returns></returns>
        Stack<Base.GoapAction<TA, TB>> GeneratePlan(PropertyGroup<TA, TB> initialState,
            List<Base.GoapAction<TA, TB>> actions);
        
        /// <summary>
        /// Gets the final plan that the researcher has found.
        /// </summary>
        /// <param name="nodeGoal">Objective node</param>
        /// <returns>Stack of actions.</returns>
        public static Stack<Base.GoapAction<TA, TB>> GetPlan(Node<TA, TB> nodeGoal)
        {
            Stack<Base.GoapAction<TA, TB>> plan = new Stack<Base.GoapAction<TA, TB>>();
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
        public static Stack<Base.GoapAction<TA, TB>> GetInvertedPlan(Node<TA, TB> nodeGoal)
        {
            Stack<Base.GoapAction<TA, TB>> plan = GetPlan(nodeGoal);
            Stack<Base.GoapAction<TA, TB>> invertedPlan = new Stack<Base.GoapAction<TA, TB>>();
            foreach (var action in plan)
            {
                invertedPlan.Push(action);
            }
            return invertedPlan;
        }
    }
}