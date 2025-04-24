using LUGoap.Base;

namespace LUGoap.Learning
{
    /// <summary>
    /// Represents a NPC with the hability to plan. Can also be treated as IEntity.
    /// </summary>
    public interface ILearningAgent : IAgent
    {
        /// <summary>
        /// Gets the learning that 
        /// </summary>
        QLearning Learning { get; }
        
        /// <summary>
        /// If learning actions of the plan should also apply a reward relative to the result of the plan.
        /// </summary>
        bool ApplyRewardsToLocal { get; }
    }
}