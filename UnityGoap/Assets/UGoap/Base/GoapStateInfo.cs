namespace UGoap.Base
{
    public class GoapStateInfo<TKey, TValue>
    {
        public PropertyGroup<TKey, TValue> State;
        public GoapGoal<TKey, TValue> Goal;

        public GoapStateInfo(PropertyGroup<TKey, TValue> worldState,
            GoapGoal<TKey, TValue> currentGoal = null)
        {
            State = worldState;
            Goal = currentGoal;
        }
    }
}