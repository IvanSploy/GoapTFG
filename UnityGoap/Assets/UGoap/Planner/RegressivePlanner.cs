using System;
using System.Collections.Generic;
using GoapTFG.Base;
using GoapTFG.UGoap;
using UnityEngine;
using static GoapTFG.Base.BaseTypes;

namespace GoapTFG.Planner
{
    /// <summary>
    /// Planner used to find the plan required.
    /// </summary>
    /// <typeparam name="TKey">Key type</typeparam>
    /// <typeparam name="TValue">String type</typeparam>
    public class RegressivePlanner<TKey, TValue> : Planner<TKey, TValue>
    {
        private const int ACTION_LIMIT = 50000;
        private readonly Dictionary<TKey, List<IGoapAction<TKey, TValue>>> _actions; 
        private readonly HashSet<string> _actionsVisited; 

        private RegressivePlanner(GoapGoal<TKey, TValue> goal, INodeGenerator<TKey, TValue> nodeGenerator)
            : base(goal, nodeGenerator)
        {
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
        public static Stack<GoapActionData<TKey, TValue>> CreatePlan(PropertyGroup<TKey, TValue> currentState, GoapGoal<TKey, TValue> goapGoal,
            List<IGoapAction<TKey, TValue>> actions, Func<GoapGoal<TKey, TValue>, PropertyGroup<TKey, TValue>, int> newHeuristic = null)
        {
            if (goapGoal.IsReached(currentState)) return null;
            var regressivePlanner = new RegressivePlanner<TKey, TValue>(goapGoal, new AStar<TKey, TValue>(newHeuristic));
            return regressivePlanner.GeneratePlan(currentState, actions);
        }

        public override Stack<GoapActionData<TKey, TValue>> GeneratePlan(PropertyGroup<TKey, TValue> initialState,
            List<IGoapAction<TKey, TValue>> actions)
        {
            if (initialState == null || actions == null) throw new ArgumentNullException();
            if (actions.Count == 0) return null;

            RegisterActions(actions);
            
            int nodesCreated = 1;
            
            _current = _nodeGenerator.CreateInitialNode(initialState, _goal);
            while (_current != null)
            {
                foreach (var key in _current.Goal)
                {
                    foreach (var action in _actions[key])
                    {
                        if(_current.State.HasKey(key) && action.GetEffects().HasKey(key))
                            if(!CheckEffectCompatibility(_current[key], _current.Goal[key],
                                   action.GetEffects().GetEffect(key))) continue;

                        if(_actionsVisited.Contains(action.Name)) continue;
                        _actionsVisited.Add(action.Name);
                            
                        var child = _current.ApplyRegressiveAction(action,
                            _current.Goal, out var reached);
                        
                        if(child == null) continue;
                        
                        _nodeGenerator.AddChildToParent(_current, child);
                        nodesCreated++;
                    }
                }
                _actionsVisited.Clear();
                _current = _nodeGenerator.GetNextNode(_current); //Get next node.
                
                if (_current != null)
                {
                    if (_current.IsGoal)
                    {
                        Debug.Log("NODOS CREADOS: " + nodesCreated);
                        Debug.Log("ACCIONES RECORRIDAS: " + UGoapAction.actionsApplied);
                        return GetInvertedPlan(_current);
                    }
                    else if (ACTION_LIMIT > 0 && _current.ActionCount >= ACTION_LIMIT)
                    {
                        _current.IsGoal = true; //To avoid recursive loop behaviour.
                        return GetInvertedPlan(_current);
                    }
                }
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
        
        public static bool CheckEffectCompatibility(object currentValue, object valueDesired, EffectType effectType)
        {
            bool compatible;
            switch (effectType)
            {
                case EffectType.Add:
                case EffectType.Multiply:
                    compatible = currentValue switch
                    {
                        float floatValue => floatValue < (float)valueDesired,
                        int intValue => intValue < (int)valueDesired,
                        _ => true
                    };
                    break;
                case EffectType.Subtract:
                case EffectType.Divide:
                    compatible = currentValue switch
                    {
                        float floatValue => floatValue > (float)valueDesired,
                        int intValue => intValue > (int)valueDesired,
                        _ => true
                    };
                    break;
                default:
                    compatible = true;
                    break;
                }
                return compatible;
        }
    }
}