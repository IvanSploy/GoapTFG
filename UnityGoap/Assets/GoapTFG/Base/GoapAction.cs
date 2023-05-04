namespace GoapTFG.Base
{
    public abstract class GoapAction<TA, TB> : IGoapAction<TA, TB>
    {
        public string Name { get; }
        private readonly PropertyGroup<TA, TB> _preconditions;
        private readonly PropertyGroup<TA, TB> _effects;
        private int _cost = 1;
        public bool IsCompleted { get; } = false;

        protected GoapAction(PropertyGroup<TA, TB> preconditions, PropertyGroup<TA, TB> effects)
        {
            _preconditions = preconditions;
            _effects = effects;
        }
        
        //Procedural related.
        protected virtual bool ProceduralConditions(PropertyGroup<TA, TB> worldState) => true;
        protected virtual PropertyGroup<TA, TB> ProceduralEffects(PropertyGroup<TA, TB> worldState) => null;
        protected abstract void PerformedActions(PropertyGroup<TA, TB> worldState);
        
        //Cost related.
        public virtual int GetCost() => _cost;
        public virtual int SetCost(int cost) => _cost = cost;
        
        //Getters
        public PropertyGroup<TA, TB> GetPreconditions() => _preconditions;
        public PropertyGroup<TA, TB> GetEffects() => _effects;

        //GOAP utilities.
        public PropertyGroup<TA, TB> ApplyAction(PropertyGroup<TA, TB> worldState)
        {
            if (!CheckAction(worldState)) return null;
            return DoApplyAction(worldState);
        }

        public PropertyGroup<TA, TB> Execute(PropertyGroup<TA, TB> worldState)
        {
            worldState = ApplyAction(worldState);
            PerformedActions(worldState);
            return worldState;
        }

        //Internal methods.
        private bool CheckAction(PropertyGroup<TA, TB> worldState)
        {
            if (!worldState.CheckConflict(_preconditions))
            {
                return ProceduralConditions(worldState);
            }
            return false;
        }
        
        private PropertyGroup<TA, TB> DoApplyAction(PropertyGroup<TA, TB> worldState)
        {
            worldState += _effects;
            var lastWorldState = ProceduralEffects(worldState);
            if (lastWorldState != null) worldState = lastWorldState;
            return worldState;
        }
        
        //to do
        public PropertyGroup<TA, TB> ApplyRegressiveAction(PropertyGroup<TA, TB> worldState, ref GoapGoal<TA, TB> goapGoal, out bool reached)
        {
            if (!ProceduralConditions(worldState))
            {
                reached = false;
                return null;
            }
            
            var ws = DoApplyAction(worldState);
            var firstState = goapGoal.GetConflicts(ws);
            ws.CheckConflict(_preconditions, out var lastState);
            if(firstState == null && lastState != null) goapGoal = new GoapGoal<TA, TB>(goapGoal.Name, lastState, goapGoal.PriorityLevel);
            else if (firstState != null && lastState == null) goapGoal = new GoapGoal<TA, TB>(goapGoal.Name, firstState, goapGoal.PriorityLevel);
            else if (firstState == null)
            {
                reached = true;
                return ws;
            }
            else if (lastState.CheckConditionsConflict(firstState))
            {
                reached = false;
                return null;
            }
            else goapGoal = new GoapGoal<TA, TB>(goapGoal.Name, firstState + lastState, goapGoal.PriorityLevel);
            reached = false;
            return ws;
        }
        
        public override string ToString()
        {
            return Name 
                   + " ->\nPreconditions:\n" + _preconditions + "Effects:\n" + _effects
                   ;
        }
    }
}