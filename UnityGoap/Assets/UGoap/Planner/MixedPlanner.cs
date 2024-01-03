using System;
using System.Collections.Generic;
using UGoap.Base;

namespace UGoap.Planner
{
    /// <summary>
    /// Planner used to find the plan required.
    /// </summary>
    /// <typeparam name="TKey">Key type</typeparam>
    /// <typeparam name="TValue">String type</typeparam>
    public class MixedPlanner<TKey, TValue> : Planner<TKey, TValue>
    {
        private const int ACTION_LIMIT = 50000;
        private bool _greedy;
        private readonly Dictionary<TKey, List<IGoapAction<TKey, TValue>>> _actions; 
        private readonly HashSet<string> _actionsVisited; 

        private MixedPlanner(GoapGoal<TKey, TValue> goal, INodeGenerator<TKey, TValue> nodeGenerator, bool greedy = false)
            : base(goal, nodeGenerator)
        {
            _actions = new Dictionary<TKey, List<IGoapAction<TKey, TValue>>>();
            _actionsVisited = new HashSet<string>();
            _greedy = greedy;
        }

        /// <summary>
        /// Creates a plan that finds using A* the path that finds the cheapest way to reach it.
        /// </summary>
        /// <param name="currentState">Current state of the world.</param>
        /// <param name="goapGoal">Goal that is going to be reached.</param>
        /// <param name="actions">Actions aviable for the agent.</param>
        /// <param name="newHeuristic">Custom heuristic if needed</param>
        /// <returns>Stack of the plan actions.</returns>
        public static Stack<GoapActionData<TKey, TValue>> CreatePlan(StateGroup<TKey, TValue> initialState, GoapGoal<TKey, TValue> goapGoal,
            List<IGoapAction<TKey, TValue>> actions, Func<GoapGoal<TKey, TValue>, StateGroup<TKey, TValue>, int> newHeuristic = null, bool greedy = false)
        {
            if (goapGoal.IsReached(currentState)) return null;
            var mixedPlanner = new MixedPlanner<TKey, TValue>(goapGoal, new AStar<TKey, TValue>(newHeuristic), greedy);
            return mixedPlanner.GeneratePlan(currentState, actions);
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

        public override Stack<GoapActionData<TKey, TValue>> GeneratePlan(StateGroup<TKey, TValue> initialState,
            List<IGoapAction<TKey, TValue>> actions)
        {
            if (initialState == null || actions == null) throw new ArgumentNullException();
            if (actions.Count == 0) return null;

            RegisterActions(actions);

            nodesCreated = 0;
            
            _current = _nodeGenerator.CreateInitialNode(initialState, _goal);
            while (_current != null)
            {
                foreach (var goalPair in _current.Goal)
                {
                    TKey key = goalPair.Key;
                    foreach (var action in _actions[goalPair.Key])
                    {
                        //If action checked on other goal condition.
                        if(_actionsVisited.Contains(action.Name)) continue;
                        
                        //If current state has key or is not a procedural effect.
                        EffectGroup<TKey, TValue> actionEffects =
                            action.GetEffects(new GoapStateInfo<TKey, TValue>(initialState, _current.Goal));
                        if (_current.State.HasKey(key))
                        {
                            if(!CheckEffectCompatibility(_current.State[key].Value, actionEffects[key].EffectType, actionEffects[key].Value,
                                   goalPair.Value)) continue;
                        }
                        
                        _actionsVisited.Add(action.Name);
                            
                        var child = _current.ApplyMixedAction(initialState, action);
                        actionsApplied++;
                        
                        if(child == null) continue;
                        
                        if (child.IsGoal && child.CanBeGoal) //Greedy result, could be worst.
                        {
                            DebugPlan(child);
                            if (_greedy)
                            {
                                return GetInvertedPlan(child);
                            }
                        }
                        
                        _nodeGenerator.AddChildToParent(_current, child);
                        nodesCreated += 1;
                    }
                }
                
                //Begins another check
                _actionsVisited.Clear();
                _current = _nodeGenerator.GetNextNode(_current); //Get next node.
                
                if (_current != null)
                {
                    //If is goal
                    if (_current.IsGoal && _current.CanBeGoal)
                    {
                        DebugInfo(_current);
                        return GetInvertedPlan(_current);
                    }
                    //If no more actions can be checked.

                    if (ACTION_LIMIT > 0 && _current.ActionCount >= ACTION_LIMIT)
                    {
                        DebugInfo(_current);
                        _current.IsGoal = true; //To avoid recursive loop behaviour.
                        return GetInvertedPlan(_current);
                    }
                }
            }

            return null; //Plan doesnt exist.
        }
    }
}