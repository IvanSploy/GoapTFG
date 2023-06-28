namespace GoapTFG.Base{
    /// <summary>
    /// Info useful for every agent in the goap system.
    /// </summary>
    /// <typeparam name="TVector">Vector3 class</typeparam>
    /// <typeparam name="TObject">GameObject class</typeparam>
    public class MemoryFact<TVector, TObject>
        {
            public string Name;
            public TVector Position;
            public TVector Direction;
            public TObject Object;
        }
}