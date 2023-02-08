namespace GoapTFG.Base
{
    /*public struct Attribute<TA>
    {
        public TA Value;
        public float Confidence;
    }*/
    
    public class MemoryFact<TA, TB>
    {
        public string Name;
        public TA Position;
        public TA Direction;
        public TB Object;
        //public float Desire;
        public float UpdateTime;
    }
}