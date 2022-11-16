using System;

namespace GoapHanoi.Core
{
    public class Action<TA, TB>
    {
        private string _id;
        private readonly State<TA, TB> _preconditionState;
        private readonly State<TA, TB> _effectState;

        public event Action PerformedActions;

        public Action(string id, State<TA, TB> preState = null, State<TA, TB> effectState = null)
        {
            _id = id;
            _preconditionState = preState ?? new State<TA, TB>();
            _effectState = effectState ?? new State<TA, TB>();
            PerformedActions += () => System.Console.Out.WriteLine("Acción ejecutada: " + this);
        }
        
        //GOAP utilities.
        public bool CheckAction(State<TA, TB> state)
        {
            return state.CheckConflict(_preconditionState);
        }

        public State<TA, TB> ApplyAction(State<TA, TB> state)
        {
            OnPerformedActions();
            return state + _effectState;;
        }
        
        public State<TA, TB> CheckApplyAction(State<TA, TB> state)
        {
            if (state.CheckConflict(_preconditionState)) return null;
            OnPerformedActions();
            return state + _effectState;
        }

        protected virtual void OnPerformedActions()
        {
            PerformedActions?.Invoke();
        }
        
        //Overrides
        public override string ToString()
        {
            return _id + " ->\n" + _effectState;
        }
    }
}