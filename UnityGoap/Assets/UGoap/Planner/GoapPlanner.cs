﻿using System;
using System.Collections.Generic;
using UGoap.Base;

namespace UGoap.Planner
{
    /// <summary>
    /// Planner used to find the plan required.
    /// </summary>
    /// <typeparam name="TKey">Key type</typeparam>
    /// <typeparam name="TValue">String type</typeparam>
    public class GoapPlanner<TKey, TValue> : Planner<TKey, TValue>
    {
        private const int ACTION_LIMIT = 50000;
        private bool _greedy;
        private readonly Dictionary<TKey, List<IGoapAction<TKey, TValue>>> _actions = new(); 
        private readonly HashSet<string> _actionsVisited = new(); 

        public GoapPlanner(INodeGenerator<TKey, TValue> nodeGenerator, bool greedy = false)
            : base(nodeGenerator)
        {
            _greedy = greedy;
        }

        /// <summary>
        /// Creates a plan that finds using A* the path that finds the cheapest way to reach it.
        /// </summary>
        /// <param name="initialGoapState">Current goapState of the world.</param>
        /// <param name="goapGoal">Goal that is going to be reached.</param>
        /// <param name="actions">Actions aviable for the agent.</param>
        /// <param name="newHeuristic">Custom heuristic if needed</param>
        /// <param name="onNodeCreated">Executed when node is created</param>
        /// <returns>Stack of the plan actions.</returns>
        public Stack<GoapActionData<TKey, TValue>> CreatePlan(GoapState<TKey, TValue> initialGoapState, GoapGoal<TKey, TValue> goal,
            List<IGoapAction<TKey, TValue>> actions)
        {
            _goal = goal;
            if (goal.IsReached(initialGoapState)) return null;
            return GeneratePlan(initialGoapState, actions);
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

        public override Stack<GoapActionData<TKey, TValue>> GeneratePlan(GoapState<TKey, TValue> initialGoapState,
            List<IGoapAction<TKey, TValue>> actions)
        {
            if (initialGoapState == null || actions == null) throw new ArgumentNullException();
            if (actions.Count == 0) return null;

            RegisterActions(actions);

            nodesCreated = 0;
            
            _current = _nodeGenerator.CreateInitialNode(initialGoapState, _goal);
            while (_current != null)
            {
                foreach (var goalPair in _current.Goal)
                {
                    TKey key = goalPair.Key;
                    foreach (var action in _actions[goalPair.Key])
                    {
                        //If action checked on other goal condition.
                        if(_actionsVisited.Contains(action.Name)) continue;
                        
                        //If current goapState has key or is not a procedural effect.
                        GoapEffects<TKey, TValue> actionEffects =
                            action.GetEffects(new GoapStateInfo<TKey, TValue>(initialGoapState, _current.Goal, _current.State));
                        if (_current.State.HasKey(key))
                        {
                            if(!CheckEffectCompatibility(_current.State[key], actionEffects[key].EffectType, actionEffects[key].Value,
                                   goalPair.Value)) continue;
                        }
                        
                        _actionsVisited.Add(action.Name);
                            
                        var child = _current.ApplyAction(initialGoapState, action);
                        actionsApplied++;
                        
                        if(child == null) continue;
                        
                        OnNodeCreated?.Invoke(child);
                        
                        if (child.IsGoal) //Greedy result, could be worst.
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
                    if (_current.IsGoal)
                    {
                        DebugInfo(_current);
                        OnPlanCreated?.Invoke(_current);
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