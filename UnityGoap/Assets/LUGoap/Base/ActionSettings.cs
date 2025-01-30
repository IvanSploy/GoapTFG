namespace LUGoap.Base
{
    public class ActionSettings
    {
        public State InitialState;
        public Conditions Goal;
        public int GlobalLearningCode;
        public int LocalLearningCode;
        public string[] Parameters;
        
        public static ActionSettings CreateDefault(Action action)
        {
            var settings = new ActionSettings()
            {
                InitialState = new State(),
                Goal = new Conditions(),
            };
            return action.CreateSettings(settings);
        }
    }
}