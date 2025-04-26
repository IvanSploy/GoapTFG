using System;
using System.Collections.Generic;
using LUGoap.Base;
using static LUGoap.Base.PropertyManager;
using Action = LUGoap.Base.Action;

namespace LUGoap.Planning
{
    /// <summary>
    /// Planner used to find the plan required.
    /// </summary>
    public class BackwardPlanner : Planner
    {
        private const int NODES_LIMIT = 10000;
        private readonly bool _greedy;
        private readonly Dictionary<PropertyKey, List<Action>> _actions = new(); 
        private readonly HashSet<string> _actionsVisited = new();
        private readonly IAgent _agent;

        public BackwardPlanner(INodeGenerator nodeGenerator, IAgent agent, bool greedy = false)
            : base(nodeGenerator)
        {
            _greedy = greedy;
            _agent = agent;
        }
        
        private void RegisterActions(List<Action> actions)
        {
            foreach (var action in actions)
            {
                foreach (var key in action.GetAffectedKeys())
                {
                    if (_actions.TryGetValue(key, out var actionList)) actionList.Add(action);
                    else _actions[key] = new List<Action> { action };
                }
            }
        }

        protected override Plan GeneratePlan(List<Action> actions)
        {
            if (InitialState == null || actions == null) throw new ArgumentNullException();
            if (actions.Count == 0) return null;

            RegisterActions(actions);

            _nodesCreated = 0;
            _nodesSkipped = 0;
            
            _current = _nodeGenerator.Initialize(InitialState, _goal.ConditionGroup);
            while (_current != null)
            {
                _actionsVisited.Clear();
                foreach (var goalPair in _current.Goal)
                {
                    PropertyKey key = goalPair.Key;
                    if (!_actions.TryGetValue(key, out var actionList)) break;
                    
                    foreach (var action in actionList)
                    {
                        //If action checked on other goal condition.
                        if(_actionsVisited.Contains(action.Name)) continue;
                        
                        //Generate action parameters.
                        var actionSettings = _nodeGenerator.CreateSettings(_current, action);
                        
                        //Check effect compatibility with initial state (the one getting closer).
                        EffectGroup actionEffectGroup = action.GetEffects(actionSettings);
                        if(!CheckEffectCompatibility(InitialState.TryGetOrDefault(key),
                               actionEffectGroup[key].Type, actionEffectGroup[key].Value, goalPair.Value))
                            continue;
                        
                        _actionsVisited.Add(action.Name);
                            
                        var child = _current.ApplyAction(action, actionSettings);
                        _actionsApplied++;
                        
                        if(child == null) continue;
                        
                        //Greedy check for goal plan.
                        if (child.IsGoal(InitialState))
                        { 
                            //DebugPlan(child, _goal.Name);
                            //If greedy, plan is returned.
                            if (_greedy)
                            {
                                return new Plan(_agent, child);
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
                        return new Plan(_agent, _current);
                    }

                    //If no more actions can be checked.
                    if (NODES_LIMIT > 0 && _nodesCreated > NODES_LIMIT)
                    {
                        DebugInfo(_current);
                        return new Plan(_agent, _current);
                    }
                }
            }

            return null; //Plan doesnt exist.
        }
    }
}