using System.Collections.Generic;

namespace UGoap.Base
{
    /// <summary>
    /// Represents a NPC with the hability to plan. Can also be treated as IEntity.
    /// </summary>
    /// <typeparam name="PropertyKey">Key type</typeparam>
    /// <typeparam name="TValue">Value type</typeparam>
    public interface IGoapAgent : IGoapEntity
    {
        /// <summary>
        /// If agent is performing action.
        /// </summary>
        bool PerformingAction { get; set; }
        
        /// <summary>
        /// If current action executed has been interrupted.
        /// </summary>
        bool Interrupted { get; set; }
        
        /// <summary>
        /// Add posible action to the agent.
        /// </summary>
        /// <param name="action">Action to be added</param>
        void AddAction(IGoapAction action);

        /// <summary>
        /// Add posible actions to the agent.
        /// </summary>
        /// <param name="actionList">Actions to be added</param>
        void AddActions(List<IGoapAction> actionList);
        
        /// <summary>
        /// Add goal to the agent.
        /// </summary>
        /// <param name="goal"></param>
        void AddGoal(IGoapGoal goal);
        
        /// <summary>
        /// Add goals to the agent.
        /// </summary>
        /// <param name="goalList"></param>
        void AddGoals(List<IGoapGoal> goalList);

        /// <summary>
        /// Manages to create a new plan for the Agent.
        /// </summary>
        /// <param name="worldGoapState"></param>
        /// <returns>Id of the goal whose plan has been created.</returns>
        int CreateNewPlan(GoapState worldGoapState);

        /// <summary>
        /// Completes current action.
        /// </summary>
        void Complete();

        /// <summary>
        /// Tries to stop current plan execution and waits x seconds.
        /// </summary>
        void Interrupt(float seconds = 0f);
    }
}