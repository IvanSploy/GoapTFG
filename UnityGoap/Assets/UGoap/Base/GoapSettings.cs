namespace UGoap.Base
{
    public struct GoapSettings
    {
        public GoapConditions Goal;
        public bool IsUsingLearning;
        public int LearningStateCode;

        public static GoapSettings GetDefault()
        {
            return new GoapSettings()
            {
                Goal = new GoapConditions(),
                LearningStateCode = 0,
            };
        }
    }
}