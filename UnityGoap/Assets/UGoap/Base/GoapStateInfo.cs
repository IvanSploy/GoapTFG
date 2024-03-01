namespace UGoap.Base
{
    public class GoapStateInfo
    {
        public GoapState State;
        public GoapGoal Goal;
        public GoapState PredictedState;

        public GoapStateInfo(GoapState state,
            GoapGoal currentGoal,
            GoapState predictedState)
        {
            State = state;
            Goal = currentGoal;
            PredictedState = predictedState;
        }
    }
}