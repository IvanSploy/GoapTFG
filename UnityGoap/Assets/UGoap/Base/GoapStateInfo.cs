namespace GoapTFG.Base
{
    public class GoapStateInfo<TKey, TValue>
    {
        public PropertyGroup<TKey, TValue> State;
        public GoapGoal<TKey, TValue> Goal;
        public PropertyGroup<TKey, TValue> ProceduralEffects;

        public GoapStateInfo(PropertyGroup<TKey, TValue> worldState,
            GoapGoal<TKey, TValue> currentGoal = null,
            PropertyGroup<TKey, TValue> proceduralEffects = null)
        {
            State = worldState;
            Goal = currentGoal;
            ProceduralEffects = proceduralEffects;
        }
    }
}