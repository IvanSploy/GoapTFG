namespace UGoap.Base
{
    /// <summary>
    /// Represents an Enitity with an goapState.
    /// </summary>
    public interface IGoapEntity
    {
        public string Name { get; }
        
        /// <summary>
        /// Represents the goapState associated with the current entity.
        /// </summary>
        public GoapState CurrentGoapState { get; set; }
    }
}