using System;
using System.Collections.Generic;
using GoapTFG.Base;

namespace GoapTFG.Planner
{
    /// <summary>
    /// Planner used to find the plan required.
    /// </summary>
    /// <typeparam name="TA">Key type</typeparam>
    /// <typeparam name="TB">String type</typeparam>
    public class RegressivePlanner<TA, TB> : IPlanner<TA, TB>
    {
        private const int ACTION_LIMIT = 9999;
        
        private Node<TA, TB> _current;
        private Goal<TA, TB> _goal;
        private INodeGenerator<TA, TB> _nodeGenerator; 

        private RegressivePlanner(Goal<TA, TB> goal, INodeGenerator<TA, TB> nodeGenerator)
        {
            _goal = goal;
            _nodeGenerator = nodeGenerator;
        }

        /// <summary>
        /// Creates a plan that finds using A* the path that finds the cheapest way to reach it.
        /// </summary>
        /// <param name="currentState">Current state of the world.</param>
        /// <param name="goal">Goal that is going to be reached.</param>
        /// <param name="actions">Actions aviable for the agent.</param>
        /// <param name="newHeuristic">Custom heuristic if needed</param>
        /// <returns>Stack of the plan actions.</returns>
        public static Stack<Base.Action<TA, TB>> CreatePlan(PropertyGroup<TA, TB> currentState, Goal<TA, TB> goal,
            List<Base.Action<TA, TB>> actions, Func<Goal<TA, TB>, PropertyGroup<TA, TB>, int> newHeuristic = null)
        {
            if (goal.IsReached(currentState)) return null;
            RegressivePlanner<TA, TB> regressivePlanner = new RegressivePlanner<TA, TB>(goal, new AStar<TA, TB>(newHeuristic));
            return regressivePlanner.GeneratePlan(currentState, actions);
        }

        public Stack<Base.Action<TA, TB>> GeneratePlan(PropertyGroup<TA, TB> initialState,
            List<Base.Action<TA, TB>> actions)
        {
            if (initialState == null || actions == null) throw new ArgumentNullException();
            if (actions.Count == 0) return null;
            
            _current = _nodeGenerator.CreateInitialNode(initialState, _goal);
            
            while (_current != null)
            {
                for (int i = 0; i < actions.Count; i++)
                {
                    Node<TA, TB> child = _current.ApplyRegressiveAction(actions[i], _current.Goal, out bool reached);
                    if (!reached) goto IsGoal;
                    _nodeGenerator.AddChildToParent(_current, child, actions[i]);
                }
                _current = _nodeGenerator.GetNextNode(_current); //Get next node.
                if (ACTION_LIMIT > 0 && _current.ActionCount >= ACTION_LIMIT)  _current.IsGoal = true; //To avoid recursive loop behaviour.
            }
            
            if (_current == null) return null; //Plan doesnt exist.
            IsGoal: return IPlanner<TA, TB>.GetPlan(_current); //Gets the plan of the goal node.
        }
    }
}