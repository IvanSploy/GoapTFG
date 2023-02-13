using System;
using System.Collections;
using System.Collections.Generic;
using GoapTFG.Base;

namespace GoapTFG.Planner
{
    public interface INodeGenerator<TA, TB>
    {
        /// <summary>
        /// Peforms the main planning cycle from the start to the end.
        /// </summary>
        /// <param name="currentState">Initial state of the world</param>
        /// <param name="actions">Actions aviable for the agent.</param>
        /// <returns>Stack of actions for the agent.</returns>
        Stack<Base.Action<TA, TB>> DoCreatePlan(PropertyGroup<TA, TB> currentState,
            List<Base.Action<TA, TB>> actions);

        /// <summary>
        /// Extracts the next node to be expanded.
        /// </summary>
        /// <returns>Node of the researcher.</returns>
        Node<TA, TB> Pop();

        /// <summary>
        /// Expands the and replaces the current node of the generator.
        /// </summary>
        /// <param name="actions">Actions</param>
        void ExpandCurrentNode(List<Base.Action<TA, TB>> actions);
        
        /// <summary>
        /// Gets the final plan that the researcher has found.
        /// </summary>
        /// <param name="nodeGoal">Objective node</param>
        /// <returns>Stack of actions.</returns>
        static Stack<Base.Action<TA, TB>> GetPlan(Node<TA, TB> nodeGoal)
        {
            Stack<Base.Action<TA, TB>> plan = new Stack<Base.Action<TA, TB>>();
            while (nodeGoal.Parent != null)
            {
                plan.Push(nodeGoal.Action);
                nodeGoal = nodeGoal.Parent;
            }
            return plan;
        }

        /// <summary>
        /// Retrieves the custom heuristic of the generator if exists.
        /// </summary>
        /// <returns>Custom heuristic.</returns>
        Func<Goal<TA, TB>, PropertyGroup<TA, TB>, int> GetCustomHeuristic();
    }
}