namespace UGoap.Base
{
    /// <summary>
    /// Represents an Enitity with an goapState.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public interface IGoapEntity<TKey, TValue>
    {
        public string Name { get; }
        
        /// <summary>
        /// Represents the goapState associated with the current entity.
        /// </summary>
        public GoapState<TKey, TValue> CurrentGoapState { get; set; }
    }
}