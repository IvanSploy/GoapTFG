namespace LUGoap.Base
{
    public struct NodeAction
    {
        public Action Action;
        public Conditions Conditions;
        public Effects Effects;
        public string[] Parameters;
        public int LearningCode;
        public int ActionLearningCode;
    }
}