using System;
using UGoap.Base;

namespace UGoap.Planner
{
    public interface INodeGenerator
    {
        //Properties
        GoapState InitialState { get; }

        //Methods
        /// <summary>
        /// Performs the main planning cycle from the start to the end.
        /// </summary>
        Node CreateInitialNode(GoapConditions initialGoal);

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
    }
}