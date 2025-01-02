namespace UGoap.Base
{
    public struct ActionSettings
    {
        public State InitialState;
        public Conditions Goal;
        public int LearningCode;
        public string[] Parameters;

        public static ActionSettings CreateDefault(Action action)
        {
            var settings = new ActionSettings()
            {
                InitialState = new State(),
                Goal = new Conditions(),
            };
            var parameters = action.CreateParameters(0);
            settings.Parameters = parameters;
            return settings;
        }
        
        
    }
}