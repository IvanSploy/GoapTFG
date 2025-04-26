namespace LUGoap.Base
{
    /// <summary>
    /// Represents an Enitity with an goapState.
    /// </summary>
    public interface IEntity
    {
        public string Name { get; }
        
        /// <summary>
        /// Represents the current State associated with the current entity.
        /// </summary>
        public State CurrentState { get; }
    }
}