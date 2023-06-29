namespace GoapTFG.Base
{
    public class GoapStateInfo<TKey, TValue>
    {
        public PropertyGroup<TKey, TValue> WorldState;
        public GoapGoal<TKey, TValue> CurrentGoal;

        public GoapStateInfo(PropertyGroup<TKey, TValue> worldState, GoapGoal<TKey, TValue> currentGoal = null)
        {
            WorldState = worldState;
            CurrentGoal = currentGoal;
        }
    }
}