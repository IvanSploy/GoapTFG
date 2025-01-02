using System;
using System.Collections.Generic;
using UGoap.Base;
using static UGoap.Base.PropertyManager;
using Action = UGoap.Base.Action;

namespace UGoap.Planning
{
    /// <summary>
    /// Planner used to find the plan required.
    /// </summary>
    public class BackwardPlanner : Planner
    {
        private const int ACTION_LIMIT = 50000;
        private readonly bool _greedy;
        private readonly Dictionary<PropertyKey, List<Action>> _actions = new(); 
        private readonly HashSet<string> _actionsVisited = new();

        public BackwardPlanner(INodeGenerator nodeGenerator, bool greedy = false)
            : base(nodeGenerator)
        {
            _greedy = greedy;
        }
        
        private void RegisterActions(List<Action> actions)
        {
            foreach (var action in actions)
            {
                foreach (var key in action.GetAffectedKeys())
                {
                    if(!_actions.ContainsKey(key))
                        _actions[key] = new List<Action>{action};
                    else
                        _actions[key].Add(action);
                }
            }
        }

        protected override Plan GeneratePlan(List<Action> actions)
        {
            if (InitialState == null || actions == null) throw new ArgumentNullException();
            if (actions.Count == 0) return null;

            RegisterActions(actions);

            _nodesCreated = 0;
            
            _current = _nodeGenerator.Initialize(InitialState, _goal.Conditions);
            while (_current != null)
            {
                _actionsVisited.Clear();
                foreach (var goalPair in _current.Goal)
                {
                    PropertyKey key = goalPair.Key;
                    if (!_actions.ContainsKey(key)) break;
                    
                    foreach (var action in _actions[key])
                    {
                        //If action checked on other goal condition.
                        if(_actionsVisited.Contains(action.Name)) continue;
                        
                        //Generate action parameters.
                        var actionSettings = _nodeGenerator.CreateSettings(_current, action);
                        
                        //Check effect compatibility with initial state (the one getting closer).
                        Effects actionEffects = action.GetEffects(actionSettings);
                        if(!CheckEffectCompatibility(InitialState.TryGetOrDefault(key), actionEffects[key].EffectType, 
                               actionEffects[key].Value, goalPair.Value)) 
                            continue;
                        
                        _actionsVisited.Add(action.Name);
                            
                        var child = _current.ApplyAction(action, actionSettings);
                        _actionsApplied++;
                        
                        if(child == null) continue;
                        
                        //Greedy check for goal plan.
                        if (child.IsGoal(InitialState))
                        {
                            DebugPlan(child, _goal.Name);
                            //If greedy, plan is returned.
                            if (_greedy)
                            {
                                return new Plan(child);
                            }
                        }
                        
                        _nodeGenerator.Add(child);
                        _nodesCreated += 1;
                    }
                }
                
                _current = _nodeGenerator.GetNextNode(_current); //Get next node.
                if (_current != null)
                {
                    //If is goal
                    if (_current.IsGoal(InitialState))
                    {
                        DebugInfo(_current);
                        return new Plan(_current);
                    }
                    
                    //If no more actions can be checked.
                    if (ACTION_LIMIT > 0 && _current.ActionCount >= ACTION_LIMIT)
                    {
                        DebugInfo(_current);
                        return new Plan(_current);
                    }
                }
            }

            return null; //Plan doesnt exist.
        }
    }
}