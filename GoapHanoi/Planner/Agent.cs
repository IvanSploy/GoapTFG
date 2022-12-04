using System.Collections.Generic;
using GoapHanoi.Base;

namespace GoapHanoi.Planner
{
    //Handles the GOAP planification and is who realices the actions.
    public class Agent<TA, TB>
    {
        private NodeGenerator<TA, TB> _nodeGenerator;
        private readonly List<Action<TA, TB>> _actions;

        public Agent(NodeGenerator<TA, TB> planner)
        {
            _nodeGenerator = planner;
            _actions = new List<Action<TA, TB>>();
        }

        public void OnUpdate()
        {
            
        }
    }
}