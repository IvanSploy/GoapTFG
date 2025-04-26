using System;
using LUGoap.Base;
using Action = LUGoap.Base.Action;

namespace LUGoap.Planning
{
    public interface INodeGenerator : IDisposable
    {
        /// <summary>
        /// Creates a new Node from the factory.
        /// </summary>
        /// <returns></returns>
        Node CreateNode(State initialState, ConditionGroup goal);
        
        /// <summary>
        /// Disposes a node to be used by the factory.
        /// </summary>
        /// <returns></returns>
        void DisposeNode(Node node);
        
        //Methods
        /// <summary>
        /// Performs the main planning cycle from the start to the end.
        /// </summary>
        Node Initialize(State initialState, ConditionGroup initialGoal);
        
        /// <summary>
        /// Add node to generator.
        /// </summary>
        /// <param name="node"></param>
        void Add(Node node);

        /// <summary>
        /// Retrieves the next node that has to be analyzed by the generator.
        /// </summary>
        /// <param name="current">The last node analized</param>
        /// <returns></returns>
        Node GetNextNode(Node current);

        /// <summary>
        /// Generate Action Settings related to this node generator.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        ActionSettings CreateSettings(Node node, Action action);
        
        /// <summary>
        /// Gets the heuristic according to a certain node.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        int GetCost(Node node);
        
        /// <summary>
        /// Gets the heuristic according to a certain node.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        int GetHeuristicCost(Node node);
        
        /// <summary>
        /// Retrieves the learning code related to a certain node.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        int GetLearningCode(Node node);
    }
}