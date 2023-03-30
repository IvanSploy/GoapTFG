namespace GoapTFG.Base
{
    /// <summary>
    /// Represents an Enitity with an state.
    /// </summary>
    /// <typeparam name="TA"></typeparam>
    /// <typeparam name="TB"></typeparam>
    public interface IEntity<TA, TB>
    {
        public string Name { get; }
        
        /// <summary>
        /// Represents the state associated with the current entity.
        /// </summary>
        public PropertyGroup<TA, TB> CurrentState { get; set; }
    }
}