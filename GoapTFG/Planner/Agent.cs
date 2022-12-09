using System.Collections.Generic;
using System.Linq;
using GoapTFG.Base;

namespace GoapTFG.Planner
{
    //Handles the GOAP planification and is who realices the actions.
    public class Agent<TA, TB>
    {
        private NodeGenerator<TA, TB> _nodeGenerator;
        private readonly List<Action<TA, TB>> _actions;
        private List<Action<TA, TB>> _currentPlan;

        public Agent(NodeGenerator<TA, TB> planner)
        {
            _nodeGenerator = planner;
            _actions = new List<Action<TA, TB>>();
            _currentPlan = null;
        }

        /// <summary>
        /// Add posible actions to the agent, this can be used by the sensors.
        /// </summary>
        /// <param name="actions">Actions to be added</param>
        public void AddActions(List<Action<TA,TB>> actions)
        {
            _actions.AddRange(actions);
        }

        public List<Action<TA,TB>> GetPlan()
        {
            return _currentPlan = _nodeGenerator.CreatePlan(_actions);
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
    }
}