namespace UGoap.Base
{
    public struct ActionSettings
    {
        public State InitialState;
        public Conditions Goal;
        public string[] Parameters;

        public static ActionSettings CreateDefault(Action action)
        {
            var settings = new ActionSettings()
            {
                InitialState = new State(),
                Goal = new Conditions(),
            };
            var parameters = action.CreateParameters(settings.InitialState, settings.Goal);
            settings.Parameters = parameters;
            return settings;
        }
        
        public static ActionSettings Create(State initialState, Conditions goal, Action action)
        {
            return new ActionSettings()
            {
                InitialState = initialState,
                Goal = goal,
                Parameters = action.CreateParameters(initialState, goal),
            };
        }
    }
}