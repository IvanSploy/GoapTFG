namespace GoapTFG.Base
{
    public struct Attribute<TA>
    {
        public TA Value;
        public float Confidence;
    }
    
    public class MemoryFact<TA, TB>
    {
        public string Name;
        public Attribute<TA> Position = new();
        public Attribute<TA> Direction = new();
        public Attribute<TB> Object = new();
        //public float Desire;
        public float UpdateTime;
    }
}