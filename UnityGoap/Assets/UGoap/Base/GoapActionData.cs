namespace UGoap.Base
{
    public struct GoapActionData<TKey, TValue>
    {
        public IGoapAction<TKey, TValue> Action;
        public GoapGoal<TKey, TValue> Goal;
        public EffectGroup<TKey, TValue> ProceduralEffects;

        public GoapActionData(IGoapAction<TKey, TValue> action, GoapGoal<TKey, TValue> goal, EffectGroup<TKey, TValue> proceduralEffects)
        {
            Action = action;
            Goal = goal;
            ProceduralEffects = proceduralEffects;
        }
    }
}