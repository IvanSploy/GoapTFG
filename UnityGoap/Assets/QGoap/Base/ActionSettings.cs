namespace LUGoap.Base
{
    public class ActionSettings
    {
        public State InitialState;
        public ConditionGroup Goal;
        public int GlobalLearningCode;
        public int LocalLearningCode;
        public string[] Parameters;
        
        public static ActionSettings CreateDefault(Action action)
        {
            var settings = new ActionSettings()
            {
                InitialState = new State(),
                Goal = new ConditionGroup(),
            };
            return action.CreateSettings(settings);
        }
    }
}