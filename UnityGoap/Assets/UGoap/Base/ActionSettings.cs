namespace UGoap.Base
{
    public struct ActionSettings
    {
        public State InitialState;
        public Conditions Goal;

        public static ActionSettings GetDefault()
        {
            return new ActionSettings()
            {
                InitialState = new State(),
                Goal = new Conditions(),
            };
        }
    }
}