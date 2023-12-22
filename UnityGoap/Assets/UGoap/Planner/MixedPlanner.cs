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
    public class MixedPlanner<TKey, TValue> : Planner<TKey, TValue>
    {
        private static int nodesSkipped = 0;
        private const int ACTION_LIMIT = 50000;
        private bool _greedy = false;
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
        public static Stack<GoapActionData<TKey, TValue>> CreatePlan(PropertyGroup<TKey, TValue> currentState, GoapGoal<TKey, TValue> goapGoal,
            List<IGoapAction<TKey, TValue>> actions, Func<GoapGoal<TKey, TValue>, PropertyGroup<TKey, TValue>, int> newHeuristic = null, bool greedy = false)
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
        
        public static bool CheckEffectCompatibility(object currentValue, EffectType effectType, object actionValue,
            object desiredValue, ConditionType conditionType)
        {
            //Check if condition will be fulfilled.
            object resultValue = EvaluateEffect(currentValue, actionValue, effectType);
            if (EvaluateCondition(resultValue, desiredValue, conditionType))
            {
                return true;
            }
            
            //Is condition is not reached after evaluation.
            bool compatible;
            switch (effectType)
            {
                case EffectType.Add:
                case EffectType.Multiply:
                    switch (conditionType)
                    {
                        case ConditionType.GreaterThan:
                        case ConditionType.GreaterOrEqual:
                            compatible = true;
                            break;
                        default:
                            compatible = false;
                            break;
                    }
                    break;
                case EffectType.Subtract:
                case EffectType.Divide:
                    switch (conditionType)
                    {
                        case ConditionType.LessThan:
                        case ConditionType.LessOrEqual:
                            compatible = true;
                            break;
                        default:
                            compatible = false;
                            break;
                    }
                    break;
                default:
                    compatible = false;
                    break;
            }

            if (!compatible)
            {
                nodesSkipped++;
                //Debug.Log( currentValue + " | " + effectType + " | " + actionValue + " || " + resultValue + " | " + conditionType + " | " + desiredValue);
            }
            return compatible;
        }

        public override Stack<GoapActionData<TKey, TValue>> GeneratePlan(PropertyGroup<TKey, TValue> initialState,
            List<IGoapAction<TKey, TValue>> actions)
        {
            if (initialState == null || actions == null) throw new ArgumentNullException();
            if (actions.Count == 0) return null;

            RegisterActions(actions);

            int nodesCreated = 0;
            
            _current = _nodeGenerator.CreateInitialNode(initialState, _goal);
            while (_current != null)
            {
                foreach (var key in _current.Goal)
                {
                    foreach (var action in _actions[key])
                    {
                        //If action checked on other goal condition.
                        if(_actionsVisited.Contains(action.Name)) continue;
                        
                        //If current state has key or is not a procedural effect.
                        if (_current.State.HasKey(key) && action.GetEffects().HasKey(key))
                        {
                            PropertyGroup<TKey, TValue> actionEffects = action.GetEffects();
                            PropertyGroup<TKey, TValue> goalState = _current.Goal.GetState();
                            if(! CheckEffectCompatibility(_current.State[key], actionEffects.GetEffect(key), actionEffects.GetValue(key),
                                   goalState.GetValue(key), goalState.GetCondition(key))) continue;
                        }
                        
                        _actionsVisited.Add(action.Name);
                            
                        var child = _current.ApplyMixedAction(initialState, action);
                        
                        if(child == null) continue;
                        
                        if (_greedy && child.IsGoal && child.CanBeGoal) //Greedy re sult, could be worst.
                        {
                            return GetInvertedPlan(child);
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
                        Debug.Log("NODOS CREADOS: " + nodesCreated);
                        Debug.Log("NODOS SALTADOS: " + nodesSkipped);
                        Debug.Log("ACCIONES RECORRIDAS: " + UGoapAction.actionsApplied);
                        Debug.Log("COSTE: " + _current.TotalCost);
                        UGoapAction.actionsApplied = 0;
                        return GetInvertedPlan(_current);
                    }
                    //If no more actions can be checked.
                    else if (ACTION_LIMIT > 0 && _current.ActionCount >= ACTION_LIMIT)
                    {
                        _current.IsGoal = true; //To avoid recursive loop behaviour.
                        return GetInvertedPlan(_current);
                    }
                }
            }

            return null; //Plan doesnt exist.
        }
    }
}