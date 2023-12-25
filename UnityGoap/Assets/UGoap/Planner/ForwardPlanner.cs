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
    public class ForwardPlanner<TKey, TValue> : Planner<TKey, TValue>
    {
        private const int ACTION_LIMIT = 500;

        private ForwardPlanner(GoapGoal<TKey, TValue> goal, INodeGenerator<TKey, TValue> nodeGenerator)
            : base(goal, nodeGenerator) { }

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
            ForwardPlanner<TKey, TValue> regressiveForwardPlannerBase = new ForwardPlanner<TKey, TValue>(goapGoal, new AStar<TKey, TValue>(newHeuristic));
            return regressiveForwardPlannerBase.GeneratePlan(currentState, actions);
        }

        public override Stack<GoapActionData<TKey, TValue>> GeneratePlan(PropertyGroup<TKey, TValue> initialState,
            List<IGoapAction<TKey, TValue>> actions)
        {
            if (initialState == null || actions == null) throw new ArgumentNullException();
            if (actions.Count == 0) return null;
            
            _current = _nodeGenerator.CreateInitialNode(initialState, _goal);
            
            while (_current != null)
            {
                foreach (var action in actions)
                {
                    Node<TKey, TValue> child = _current.ApplyAction(action);
                    if(child == null) continue;
                    if(child.IsGoal) return GetPlan(child); //Fin de la búsqueda.
                    _nodeGenerator.AddChildToParent(_current, child);
                }
                _current = _nodeGenerator.GetNextNode(_current); //Get next node.
                if (_current != null && ACTION_LIMIT > 0 && _current.ActionCount >= ACTION_LIMIT)  _current.IsGoal = true; //To avoid recursive loop behaviour.
            }
            return null; //Plan doesnt exist.
        }
    }
}