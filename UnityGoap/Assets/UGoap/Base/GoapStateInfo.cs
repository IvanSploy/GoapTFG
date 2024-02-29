namespace UGoap.Base
{
    public class GoapStateInfo<TKey, TValue>
    {
        public GoapState<TKey, TValue> State;
        public GoapGoal<TKey, TValue> Goal;
        public GoapState<TKey, TValue> PredictedState;

        public GoapStateInfo(GoapState<TKey, TValue> state,
            GoapGoal<TKey, TValue> currentGoal,
            GoapState<TKey, TValue> predictedState)
        {
            State = state;
            Goal = currentGoal;
            PredictedState = predictedState;
        }
    }
}