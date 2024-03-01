using System;
using UGoap.Base;

namespace UGoap.Planner
{
    public interface INodeGenerator
    {
        /// <summary>
        /// Peforms the main planning cycle from the start to the end.
        /// </summary>
        /// <param name="currentGoapState">Initial goapState of the world.</param>
        /// <param name="goal">Goal expected to reach.</param>
        /// <returns>Stack of actions for the agent.</returns>
        Node CreateInitialNode(GoapState currentGoapState, GoapGoal goal);

        /// <summary>
        /// Retrieves the next node that has to be analyzed by the generator.
        /// </summary>
        /// <param name="current">The last node analized</param>
        /// <returns></returns>
        Node GetNextNode(Node current);

        /// <summary>
        /// Relations a parent with a child though an action.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="child"></param>
        /// <param name="action"></param>
        /// <param name="proceduralEffects"></param>
        void AddChildToParent(Node parent, Node child);
        
        /// <summary>   
        /// Retrieves the custom heuristic of the generator if exists.
        /// </summary>
        /// <returns>Custom heuristic.</returns>
        Func<GoapGoal, GoapState, int> GetCustomHeuristic();
    }
}