namespace UGoap.Base
{
    public struct GoapActionData<TKey, TValue>
    {
        public IGoapAction<TKey, TValue> Action;
        public GoapGoal<TKey, TValue> Goal;
        public GoapState<TKey, TValue> PredictedState;

        public GoapActionData(IGoapAction<TKey, TValue> action, GoapGoal<TKey, TValue> goal, GoapState<TKey, TValue> predictedState)
        {
            Action = action;
            Goal = goal;
            PredictedState = predictedState;
        }
    }
}