namespace LUGoap.Base
{
    public struct NodeAction
    {
        public Action Action;
        public Conditions Conditions;
        public Effects Effects;
        public int GlobalLearningCode;
        public int LocalLearningCode;
        public string[] Parameters;
    }
}