using System.Collections.Generic;
using GoapHanoi.Base;

namespace GoapHanoi.Planner
{
    public class NodeGenerator<TA, TB>
    {
        private Goal<TA, TB> _goal;
        private Node<TA, TB> _current;
        public readonly List<Node<TA, TB>> SortedNodes;

        public NodeGenerator(PropertyGroup<TA, TB> currentState, Goal<TA, TB> goal)
        {
            _current = new Node<TA, TB>(currentState);
            _goal = goal;
            SortedNodes = new List<Node<TA, TB>>();
        }

        /// <summary>
        /// Creates a plan that finds using A* the path that finds the cheapest way to reach it.
        /// </summary>
        public Action<TA, TB>[] CreatePlan(List<Action<TA, TB>> actions)
        {
            while (!_current.IsGoal)
            {
                
            }
        }
    }
}