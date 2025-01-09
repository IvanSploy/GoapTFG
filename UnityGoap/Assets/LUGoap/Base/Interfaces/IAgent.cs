using System.Collections.Generic;
using System.Threading.Tasks;

namespace LUGoap.Base
{
    /// <summary>
    /// Represents a NPC with the hability to plan. Can also be treated as IEntity.
    /// </summary>
    public interface IAgent : IEntity
    {
        /// <summary>
        /// Represents the current Goal associated with the current entity.
        /// </summary>
        IGoal CurrentGoal { get; }
        
        /// <summary>
        /// Gets the current action node.
        /// </summary>
        NodeAction CurrentAction { get; }
        
        /// <summary>
        /// If current plan is completed.
        /// </summary>
        bool IsCompleted { get; }
        
        /// <summary>
        /// If current action executed has been interrupted.
        /// </summary>
        bool IsInterrupted { get; }
        
        /// <summary>
        /// Add posible action to the agent.
        /// </summary>
        /// <param name="action">Action to be added</param>
        void AddAction(Action action);

        /// <summary>
        /// Add posible actions to the agent.
        /// </summary>
        /// <param name="actionList">Actions to be added</param>
        void AddActions(List<Action> actionList);
        
        /// <summary>
        /// Add goal to the agent.
        /// </summary>
        /// <param name="goal"></param>
        void AddGoal(IGoal goal);
        
        /// <summary>
        /// Add goals to the agent.
        /// </summary>
        /// <param name="goalList"></param>
        void AddGoals(List<IGoal> goalList);

        /// <summary>
        /// Manages to create a new plan for the Agent.
        /// </summary>
        /// <param name="initialState"></param>
        /// <returns>Id of the goal whose plan has been created.</returns>
        int CreatePlan(State initialState);
        
        /// <summary>
        /// Manages to create a new plan for the Agent.
        /// </summary>
        /// <param name="initialState"></param>
        /// <returns>Id of the goal whose plan has been created.</returns>
        Task<int> CreatePlanAsync(State initialState);

        /// <summary>
        /// Tries to stop current plan execution and waits x seconds.
        /// </summary>
        void Interrupt(float seconds);
    }
}