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
        private readonly GoapGoal<TA, TB> _goapGoal;
        private readonly INodeGenerator<TA, TB> _nodeGenerator; 
        private readonly Dictionary<TA, List<IGoapAction<TA, TB>>> _actions; 
        private readonly HashSet<string> _actionsVisited; 

        private RegressivePlanner(GoapGoal<TA, TB> goapGoal, INodeGenerator<TA, TB> nodeGenerator)
        {
            _goapGoal = goapGoal;
            _nodeGenerator = nodeGenerator;
            _actions = new Dictionary<TA, List<IGoapAction<TA, TB>>>();
            _actionsVisited = new HashSet<string>();
        }

        /// <summary>
        /// Creates a plan that finds using A* the path that finds the cheapest way to reach it.
        /// </summary>
        /// <param name="currentState">Current state of the world.</param>
        /// <param name="goapGoal">Goal that is going to be reached.</param>
        /// <param name="actions">Actions aviable for the agent.</param>
        /// <param name="newHeuristic">Custom heuristic if needed</param>
        /// <returns>Stack of the plan actions.</returns>
        public static Stack<IGoapAction<TA, TB>> CreatePlan(PropertyGroup<TA, TB> currentState, GoapGoal<TA, TB> goapGoal,
            List<IGoapAction<TA, TB>> actions, Func<GoapGoal<TA, TB>, PropertyGroup<TA, TB>, int> newHeuristic = null)
        {
            if (goapGoal.IsReached(currentState)) return null;
            var regressivePlanner = new RegressivePlanner<TA, TB>(goapGoal, new AStar<TA, TB>(newHeuristic));
            return regressivePlanner.GeneratePlan(currentState, actions);
        }

        public Stack<IGoapAction<TA, TB>> GeneratePlan(PropertyGroup<TA, TB> initialState,
            List<IGoapAction<TA, TB>> actions)
        {
            if (initialState == null || actions == null) throw new ArgumentNullException();
            if (actions.Count == 0) return null;

            RegisterActions(actions);
            
            _current = _nodeGenerator.CreateInitialNode(initialState, _goapGoal);
            
            while (_current != null)
            {
                foreach (var key in _current.GoapGoal.GetState().GetKeys())
                {
                    foreach (var action in _actions[key])
                    {
                        if(_actionsVisited.Contains(action.Name)) continue;
                        _actionsVisited.Add(action.Name);
                        var child = _current.ApplyRegressiveAction(action, _current.GoapGoal, out var reached);
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

        private void RegisterActions(List<IGoapAction<TA, TB>> actions)
        {
            foreach (var action in actions)
            {
                foreach (var key in action.GetEffects().GetKeys())
                {
                    if(!_actions.ContainsKey(key))
                        _actions[key] = new List<IGoapAction<TA, TB>>{action};
                    else
                        _actions[key].Add(action);
                }
            }
        }
    }
}