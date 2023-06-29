namespace GoapTFG.Base
{
    /// <summary>
    /// Represents an Enitity with an state.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public interface IGoapEntity<TKey, TValue>
    {
        public string Name { get; }
        
        /// <summary>
        /// Represents the state associated with the current entity.
        /// </summary>
        public PropertyGroup<TKey, TValue> CurrentState { get; set; }
    }
}