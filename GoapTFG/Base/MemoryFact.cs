namespace GoapTFG.Base
{
    public struct Attribute<TA>
    {
        public TA Value;
        public float Confidence;
    }
    
    public class MemoryFact<TA, TB>
    {
        public Attribute<TA> Position;
        public Attribute<TA> Direction;
        public Attribute<TB> Object;
        public Attribute<float> Desire;
        public float UpdateTime;
    }
}