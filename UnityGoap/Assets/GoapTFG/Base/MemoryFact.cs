namespace GoapTFG.Base{
    /// <summary>
    /// Info useful for every agent in the goap system.
    /// </summary>
    /// <typeparam name="TA">Vector3 class</typeparam>
    /// <typeparam name="TB">GameObject class</typeparam>
    public class MemoryFact<TA, TB>
        {
            public string Name;
            public TA Position;
            public TA Direction;
            public TB Object;
        }
}