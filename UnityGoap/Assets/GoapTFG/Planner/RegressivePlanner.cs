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
        private readonly Goal<TA, TB> _goal;
        private readonly INodeGenerator<TA, TB> _nodeGenerator; 
        private readonly Dictionary<TA, List<Base.Action<TA, TB>>> _actions; 
        private readonly HashSet<string> _actionsVisited; 

        private RegressivePlanner(Goal<TA, TB> goal, INodeGenerator<TA, TB> nodeGenerator)
        {
            _goal = goal;
            _nodeGenerator = nodeGenerator;
            _actions = new Dictionary<TA, List<Base.Action<TA, TB>>>();
            _actionsVisited = new HashSet<string>();
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
            var regressivePlanner = new RegressivePlanner<TA, TB>(goal, new AStar<TA, TB>(newHeuristic));
            return regressivePlanner.GeneratePlan(currentState, actions);
        }

        public Stack<Base.Action<TA, TB>> GeneratePlan(PropertyGroup<TA, TB> initialState,
            List<Base.Action<TA, TB>> actions)
        {
            if (initialState == null || actions == null) throw new ArgumentNullException();
            if (actions.Count == 0) return null;

            RegisterActions(actions);
            
            _current = _nodeGenerator.CreateInitialNode(initialState, _goal);
            
            while (_current != null)
            {
                foreach (var key in _current.Goal.GetState().GetKeys())
                {
                    foreach (var action in _actions[key])
                    {
                        if(_actionsVisited.Contains(action.Name)) continue;
                        _actionsVisited.Add(action.Name);
                        var child = _current.ApplyRegressiveAction(action, _current.Goal, out var reached);
                        if(child == null) continue;
                        if (reached)
                        {
                            return IPlanner<TA, TB>.GetInvertedPlan(child);
                        }
                        _nodeGenerator.AddChildToParent(_current, child, action);
                    }
                }
                _actionsVisited.Clear();
                _current = _nodeGenerator.GetNextNode(_current); //Get next node.
                if (_current != null && ACTION_LIMIT > 0 && _current.ActionCount >= ACTION_LIMIT)  _current.IsGoal = true; //To avoid recursive loop behaviour.
            }
            
            return null; //Plan doesnt exist.
        }

        private void RegisterActions(List<Base.Action<TA, TB>> actions)
        {
            foreach (var action in actions)
            {
                foreach (var key in action.GetEffects().GetKeys())
                {
                    if(!_actions.ContainsKey(key))
                        _actions[key] = new List<Base.Action<TA, TB>>{action};
                    else
                        _actions[key].Add(action);
                }
            }
        }
    }
}