using UGoap.Base;

namespace UGoap.Learning
{
    /// <summary>
    /// Represents a NPC with the hability to plan. Can also be treated as IEntity.
    /// </summary>
    public interface ILearningAgent : IAgent
    {
        /// <summary>
        /// Represents the current Goal associated with the current entity.
        /// </summary>
        QLearning Learning { get; }
    }
}