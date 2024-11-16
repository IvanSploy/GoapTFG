namespace UGoap.Base
{
    public struct GoapSettings
    {
        public GoapState InitialState;
        public GoapConditions Goal;

        public static GoapSettings GetDefault()
        {
            return new GoapSettings()
            {
                InitialState = new GoapState(),
                Goal = new GoapConditions(),
            };
        }
    }
}