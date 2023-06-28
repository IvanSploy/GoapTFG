using System;
using System.Collections.Generic;
using GoapTFG.Base;
using Unity.VisualScripting;

namespace GoapTFG.Planner
{
    /// <summary>
    /// Planner used to find the plan required.
    /// </summary>
    /// <typeparam name="TKey">Key type</typeparam>
    /// <typeparam name="TValue">String type</typeparam>
    public class RegressivePlanner<TKey, TValue> : IPlanner<TKey, TValue>
    {
        private const int ACTION_LIMIT = 500;
        
        private Node<TKey, TValue> _current;
        private readonly GoapGoal<TKey, TValue> _goapGoal;
        private readonly INodeGenerator<TKey, TValue> _nodeGenerator; 
        private readonly Dictionary<TKey, List<IGoapAction<TKey, TValue>>> _actions; 
        private readonly HashSet<string> _actionsVisited; 

        private RegressivePlanner(GoapGoal<TKey, TValue> goapGoal, INodeGenerator<TKey, TValue> nodeGenerator)
        {
            _goapGoal = goapGoal;
            _nodeGenerator = nodeGenerator;
            _actions = new Dictionary<TKey, List<IGoapAction<TKey, TValue>>>();
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
        public static Stack<IGoapAction<TKey, TValue>> CreatePlan(PropertyGroup<TKey, TValue> currentState, GoapGoal<TKey, TValue> goapGoal,
            List<IGoapAction<TKey, TValue>> actions, Func<GoapGoal<TKey, TValue>, PropertyGroup<TKey, TValue>, int> newHeuristic = null)
        {
            if (goapGoal.IsReached(currentState)) return null;
            var regressivePlanner = new RegressivePlanner<TKey, TValue>(goapGoal, new AStar<TKey, TValue>(newHeuristic));
            return regressivePlanner.GeneratePlan(currentState, actions);
        }

        public Stack<IGoapAction<TKey, TValue>> GeneratePlan(PropertyGroup<TKey, TValue> initialState,
            List<IGoapAction<TKey, TValue>> actions)
        {
            if (initialState == null || actions == null) throw new ArgumentNullException();
            if (actions.Count == 0) return null;

            RegisterActions(actions);
            
            _current = _nodeGenerator.CreateInitialNode(initialState, _goapGoal);
            
            while (_current != null)
            {
                foreach (var key in _current.GoapGoal)
                {
                    foreach (var action in _actions[key])
                    {
                        var clonedAction = action.Clone();
                        if(_actionsVisited.Contains(clonedAction.Name)) continue;
                        _actionsVisited.Add(clonedAction.Name);
                        var child = _current.ApplyRegressiveAction(clonedAction, _current.GoapGoal, out var reached);
                        if(child == null) continue;
                        if (reached)
                        {
                            return IPlanner<TKey, TValue>.GetInvertedPlan(child);
                        }
                        _nodeGenerator.AddChildToParent(_current, child, clonedAction);
                    }
                }
                _actionsVisited.Clear();
                _current = _nodeGenerator.GetNextNode(_current); //Get next node.
                if (_current != null && ACTION_LIMIT > 0 && _current.ActionCount >= ACTION_LIMIT)  _current.IsGoal = true; //To avoid recursive loop behaviour.
            }
            
            return null; //Plan doesnt exist.
        }

        private void RegisterActions(List<IGoapAction<TKey, TValue>> actions)
        {
            foreach (var action in actions)
            {
                foreach (var key in action.GetAffectedKeys())
                {
                    if(!_actions.ContainsKey(key))
                        _actions[key] = new List<IGoapAction<TKey, TValue>>{action};
                    else
                        _actions[key].Add(action);
                }
            }
        }
    }
}