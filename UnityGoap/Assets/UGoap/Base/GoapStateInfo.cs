namespace UGoap.Base
{
    public class GoapStateInfo<TKey, TValue>
    {
        public StateGroup<TKey, TValue> State;
        public GoapGoal<TKey, TValue> Goal;

        public GoapStateInfo(StateGroup<TKey, TValue> worldState,
            GoapGoal<TKey, TValue> currentGoal = null)
        {
            State = worldState;
            Goal = currentGoal;
        }
    }
}