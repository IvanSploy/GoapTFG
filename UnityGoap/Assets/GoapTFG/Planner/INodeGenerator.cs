using GoapTFG.Base;

namespace GoapTFG.Planner
{
    public interface INodeGenerator<TA, TB>
    {
        /// <summary>
        /// Peforms the main planning cycle from the start to the end.
        /// </summary>
        /// <param name="currentState">Initial state of the world.</param>
        /// <param name="goal">Goal expected to reach.</param>
        /// <returns>Stack of actions for the agent.</returns>
        Node<TA, TB> CreateInitialNode(PropertyGroup<TA, TB> currentState, Goal<TA, TB> goal);

        /// <summary>
        /// Retrieves the next node that has to be analyzed by the generator.
        /// </summary>
        /// <param name="current">The last node analized</param>
        /// <returns></returns>
        Node<TA, TB> GetNextNode(Node<TA, TB> current);

        /// <summary>
        /// Relations a parent with a child though an action.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="child"></param>
        /// <param name="action"></param>
        void AddChildToParent(Node<TA, TB> parent, Node<TA, TB> child, Action<TA, TB> action);
        
        /// <summary>
        /// Retrieves the custom heuristic of the generator if exists.
        /// </summary>
        /// <returns>Custom heuristic.</returns>
        System.Func<Goal<TA, TB>, PropertyGroup<TA, TB>, int> GetCustomHeuristic();
    }
}