namespace UGoap.Base
{
    public struct GoapActionData
    {
        public IGoapAction Action;
        public GoapGoal Goal;
        public GoapState PredictedState;

        public GoapActionData(IGoapAction action, GoapGoal goal, GoapState predictedState)
        {
            Action = action;
            Goal = goal;
            PredictedState = predictedState;
        }
    }
}