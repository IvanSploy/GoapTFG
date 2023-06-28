using System;
using System.Collections.Generic;
using GoapTFG.Base;

namespace GoapTFG.Planner
{
    /// <summary>
    /// Planner used to find the plan required.
    /// </summary>
    /// <typeparam name="TKey">Key type</typeparam>
    /// <typeparam name="TValue">String type</typeparam>
    public class Planner<TKey, TValue> : IPlanner<TKey, TValue>
    {
        private const int ACTION_LIMIT = 500;
        
        private Node<TKey, TValue> _current;
        private readonly GoapGoal<TKey, TValue> _goapGoal;
        private readonly INodeGenerator<TKey, TValue> _nodeGenerator; 

        private Planner(GoapGoal<TKey, TValue> goapGoal, INodeGenerator<TKey, TValue> nodeGenerator)
        {
            _goapGoal = goapGoal;
            _nodeGenerator = nodeGenerator;
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
            Planner<TKey, TValue> regressivePlanner = new Planner<TKey, TValue>(goapGoal, new AStar<TKey, TValue>(newHeuristic));
            return regressivePlanner.GeneratePlan(currentState, actions);
        }

        public Stack<IGoapAction<TKey, TValue>> GeneratePlan(PropertyGroup<TKey, TValue> initialState,
            List<IGoapAction<TKey, TValue>> actions)
        {
            if (initialState == null || actions == null) throw new ArgumentNullException();
            if (actions.Count == 0) return null;
            
            _current = _nodeGenerator.CreateInitialNode(initialState, _goapGoal);
            
            while (_current != null)
            {
                foreach (var action in actions)
                {
                    Node<TKey, TValue> child = _current.ApplyAction(action);
                    if(child == null) continue;
                    if(child.IsGoal) return IPlanner<TKey, TValue>.GetPlan(child); //Fin de la búsqueda.
                    _nodeGenerator.AddChildToParent(_current, child, action);
                }
                _current = _nodeGenerator.GetNextNode(_current); //Get next node.
                if (_current != null && ACTION_LIMIT > 0 && _current.ActionCount >= ACTION_LIMIT)  _current.IsGoal = true; //To avoid recursive loop behaviour.
            }
            return null; //Plan doesnt exist.
        }
    }
}