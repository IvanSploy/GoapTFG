using System;
using System.Collections.Generic;
using GoapTFG.Base;

namespace GoapTFG.Planner
{
    /// <summary>
    /// Planner used to find the plan required.
    /// </summary>
    /// <typeparam name="TA">Key type</typeparam>
    /// <typeparam name="TB">String type</typeparam>
    public class Planner<TA, TB> : IPlanner<TA, TB>
    {
        private const int ACTION_LIMIT = 9999;
        
        private Node<TA, TB> _current;
        private readonly GoapGoal<TA, TB> _goapGoal;
        private readonly INodeGenerator<TA, TB> _nodeGenerator; 

        private Planner(GoapGoal<TA, TB> goapGoal, INodeGenerator<TA, TB> nodeGenerator)
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
        public static Stack<Base.GoapAction<TA, TB>> CreatePlan(PropertyGroup<TA, TB> currentState, GoapGoal<TA, TB> goapGoal,
            List<Base.GoapAction<TA, TB>> actions, Func<GoapGoal<TA, TB>, PropertyGroup<TA, TB>, int> newHeuristic = null)
        {
            if (goapGoal.IsReached(currentState)) return null;
            Planner<TA, TB> regressivePlanner = new Planner<TA, TB>(goapGoal, new AStar<TA, TB>(newHeuristic));
            return regressivePlanner.GeneratePlan(currentState, actions);
        }

        public Stack<Base.GoapAction<TA, TB>> GeneratePlan(PropertyGroup<TA, TB> initialState,
            List<Base.GoapAction<TA, TB>> actions)
        {
            if (initialState == null || actions == null) throw new ArgumentNullException();
            if (actions.Count == 0) return null;
            
            _current = _nodeGenerator.CreateInitialNode(initialState, _goapGoal);
            
            while (_current != null)
            {
                foreach (var action in actions)
                {
                    Node<TA, TB> child = _current.ApplyAction(action);
                    if(child == null) continue;
                    if(child.IsGoal) return IPlanner<TA, TB>.GetPlan(child); //Fin de la búsqueda.
                    _nodeGenerator.AddChildToParent(_current, child, action);
                }
                _current = _nodeGenerator.GetNextNode(_current); //Get next node.
                if (_current != null && ACTION_LIMIT > 0 && _current.ActionCount >= ACTION_LIMIT)  _current.IsGoal = true; //To avoid recursive loop behaviour.
            }
            return null; //Plan doesnt exist.
        }
    }
}