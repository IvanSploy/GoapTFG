using System.Collections.Generic;
using System.Linq;
using GoapTFG.Base;

namespace GoapTFG.Planner
{
    //Handles the GOAP planification and is who realices the actions.
    public class Agent<TA, TB>
    {
        private NodeGenerator<TA, TB> _nodeGenerator;
        private List<Action<TA, TB>> _currentPlan;
        private readonly List<Action<TA, TB>> _actions;
        private Goal<TA, TB> _goal;
        
        public Agent(NodeGenerator<TA, TB> planner, Goal<TA, TB> goal, List<Action<TA, TB>> actions = null)
        {
            _nodeGenerator = planner;
            _actions = actions == null ? new List<Action<TA, TB>>() : new List<Action<TA, TB>>(actions);
            _goal = goal;
            _currentPlan = null;
        }

        /// <summary>
        /// Add posible action to the agent, this can be used by the sensors.
        /// </summary>
        /// <param name="action">Action to be added</param>
        public void AddAction(Action<TA,TB> action)
        {
            _actions.Add(action);
        }
        
        /// <summary>
        /// Add posible actions to the agent, this can be used by the sensors.
        /// </summary>
        /// <param name="actions">Actions to be added</param>
        public void AddActions(List<Action<TA,TB>> actions)
        {
            _actions.AddRange(actions);
        }

        public void SetGoal(Goal<TA, TB> goal)
        {
            _goal = goal;
        }

        public bool CreatePlan(PropertyGroup<TA, TB> initialState)
        {
            if (_goal == null || _actions.Count == 0) return false;
            _currentPlan = _nodeGenerator.CreatePlan(initialState, _goal, _actions);
            return _currentPlan != null;
        }
        
        //Plan follower
        public bool DoPlan()
        {
            if (_currentPlan == null) return false;

            for (int i = 0; i < _currentPlan.Count(); i++)
            {
                _currentPlan[i].PerformAction();
            }
            return true;
        }

        public int Count()
        {
            if (_currentPlan == null) return 0;
            return _currentPlan.Count;
        }
    }
}