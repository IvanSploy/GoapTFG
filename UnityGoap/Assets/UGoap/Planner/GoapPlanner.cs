using System;
using System.Collections.Generic;
using UGoap.Base;
using static UGoap.Base.UGoapPropertyManager;

namespace UGoap.Planner
{
    /// <summary>
    /// Planner used to find the plan required.
    /// </summary>
    /// <typeparam name="PropertyKey">Key type</typeparam>
    /// <typeparam name="TValue">String type</typeparam>
    public class GoapPlanner : Planner
    {
        private const int ACTION_LIMIT = 50000;
        private bool _greedy;
        private readonly Dictionary<PropertyKey, List<IGoapAction>> _actions = new(); 
        private readonly HashSet<string> _actionsVisited = new();

        public GoapPlanner(INodeGenerator nodeGenerator, IGoapAgent agent, bool greedy = false)
            : base(nodeGenerator, agent)
        {
            _greedy = greedy;
        }

        /// <summary>
        /// Creates a plan that finds using A* the path that finds the cheapest way to reach it.
        /// </summary>
        /// <param name="initialState">Current goapState of the world.</param>
        /// <param name="goal">Goal that is going to be reached.</param>
        /// <param name="actions">Actions aviable for the agent.</param>
        /// <returns>Stack of the plan actions.</returns>
        public Plan CreatePlan(GoapState initialState, IGoapGoal goal, List<IGoapAction> actions)
        {
            _goal = goal;
            if (goal.IsGoal(initialState)) return null;
            return GeneratePlan(initialState, actions);
        }
        
        private void RegisterActions(List<IGoapAction> actions)
        {
            foreach (var action in actions)
            {
                foreach (var key in action.GetAffectedKeys())
                {
                    if(!_actions.ContainsKey(key))
                        _actions[key] = new List<IGoapAction>{action};
                    else
                        _actions[key].Add(action);
                }
            }
        }

        public override Plan GeneratePlan(GoapState initialState, List<IGoapAction> actions)
        {
            if (initialState == null || actions == null) throw new ArgumentNullException();
            if (actions.Count == 0) return null;

            RegisterActions(actions);

            nodesCreated = 0;
            
            _current = _nodeGenerator.CreateInitialNode(_goal.Conditions);
            while (_current != null)
            {
                _actionsVisited.Clear();
                foreach (var goalPair in _current.Goal)
                {
                    PropertyKey key = goalPair.Key;
                    foreach (var action in _actions[goalPair.Key])
                    {
                        //If action checked on other goal condition.
                        if(_actionsVisited.Contains(action.Name)) continue;
                        
                        //Check effect compatibility with initial state (the one getting closer).
                        GoapEffects actionEffects = action.GetEffects(_current.Settings);
                        if(!CheckEffectCompatibility(initialState.TryGetOrDefault(key), actionEffects[key].EffectType, 
                               actionEffects[key].Value, goalPair.Value)) 
                            continue;
                        
                        _actionsVisited.Add(action.Name);
                            
                        var child = _current.ApplyAction(action);
                        actionsApplied++;
                        
                        if(child == null) continue;
                        OnNodeCreated?.Invoke(child);
                        
                        //Greedy check for goal plan.
                        if (child.IsGoal(initialState))
                        {
                            DebugPlan(child, _goal.Name);
                            //If greedy, plan is returned.
                            if (_greedy)
                            {
                                return new Plan(initialState, _agent, child);
                            }
                        }
                        
                        _nodeGenerator.AddChildToParent(_current, child);
                        nodesCreated += 1;
                    }
                }
                
                _current = _nodeGenerator.GetNextNode(_current); //Get next node.
                if (_current != null)
                {
                    //If is goal
                    if (_current.IsGoal(initialState))
                    {
                        DebugInfo(_current);
                        OnPlanCreated?.Invoke(_current);
                        return new Plan(initialState, _agent, _current);
                    }
                    
                    //If no more actions can be checked.
                    if (ACTION_LIMIT > 0 && _current.ActionCount >= ACTION_LIMIT)
                    {
                        DebugInfo(_current);
                        return new Plan(initialState, _agent, _current);
                    }
                }
            }

            return null; //Plan doesnt exist.
        }
    }
}