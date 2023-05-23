namespace GoapTFG.Base
{
    public class GoapStateInfo<TA, TB>
    {
        public PropertyGroup<TA, TB> WorldState;
        public GoapGoal<TA, TB> CurrentGoal;

        public GoapStateInfo(PropertyGroup<TA, TB> worldState, GoapGoal<TA, TB> currentGoal = null)
        {
            WorldState = worldState;
            CurrentGoal = currentGoal;
        }
    }
}