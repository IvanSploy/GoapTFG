using System.Threading;
using System.Threading.Tasks;
using UGoap.Base;

namespace UGoap.Planner
{
    public class PlanNode
    {
        private readonly GoapConditions _goal;
        private readonly GoapAction _action;
        private readonly GoapActionInfo _info;

        public GoapConditions Goal => _goal;
        public GoapAction Action => _action;

        public PlanNode(GoapConditions goal, GoapAction action, GoapActionInfo info)
        {
            _goal = goal;
            _action = action;
            _info = info;
        }

        /// <summary>
        /// To execute an action related to a certain node.
        /// </summary>
        /// <param name="goapAction"></param>
        /// <param name="state"></param>
        /// <param name="agent"></param>
        /// <returns></returns>
        public Task<GoapState> ExecuteAction(GoapState state, IGoapAgent agent, CancellationToken token)
        {
            if (!CheckAction(state, agent)) return null;

            state += _info.Effects;
            return _action.Execute(state, agent, token);
        }
        
        private bool CheckAction(GoapState state, IGoapAgent agent)
        {
            if (!_info.Conditions.CheckConflict(state))
            {
                bool valid = _action.Validate(state, _info, agent);
                if (!valid)
                {
                    DebugRecord.AddRecord("La acción no ha podido completarse, plan detenido :(");
                }
                return valid;
            }
            
            DebugRecord.AddRecord("El agente no cumple con las precondiciones necesarias, plan detenido :(");
            //Debug.Log("Accion:" + Name + " | Estado actual: " + stateInfo.WorldState + " | Precondiciones accion: " + _preconditions);
            return false;
        }
    }
}