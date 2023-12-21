using System;
using GoapTFG.Base;

namespace GoapTFG.Planner
{
    public interface INodeGenerator<TKey, TValue>
    {
        /// <summary>
        /// Peforms the main planning cycle from the start to the end.
        /// </summary>
        /// <param name="currentState">Initial state of the world.</param>
        /// <param name="goal">Goal expected to reach.</param>
        /// <returns>Stack of actions for the agent.</returns>
        Node<TKey, TValue> CreateInitialNode(PropertyGroup<TKey, TValue> currentState, GoapGoal<TKey, TValue> goal);

        /// <summary>
        /// Retrieves the next node that has to be analyzed by the generator.
        /// </summary>
        /// <param name="current">The last node analized</param>
        /// <returns></returns>
        Node<TKey, TValue> GetNextNode(Node<TKey, TValue> current);

        /// <summary>
        /// Relations a parent with a child though an action.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="child"></param>
        /// <param name="action"></param>
        /// <param name="proceduralEffects"></param>
        void AddChildToParent(Node<TKey, TValue> parent, Node<TKey, TValue> child);
        
        /// <summary>   
        /// Retrieves the custom heuristic of the generator if exists.
        /// </summary>
        /// <returns>Custom heuristic.</returns>
        Func<GoapGoal<TKey, TValue>, PropertyGroup<TKey, TValue>, int> GetCustomHeuristic();
    }
}