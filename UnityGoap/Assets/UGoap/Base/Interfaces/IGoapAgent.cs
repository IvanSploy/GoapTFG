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
        /// Add posible action to the agent.
        /// </summary>
        /// <param name="action">Action to be added</param>
        public void AddAction(IGoapAction action);

        /// <summary>
        /// Add posible actions to the agent.
        /// </summary>
        /// <param name="actionList">Actions to be added</param>
        public void AddActions(List<IGoapAction> actionList);
        
        /// <summary>
        /// Add goal to the agent.
        /// </summary>
        /// <param name="goal"></param>
        public void AddGoal(IGoapGoal goal);
        
        /// <summary>
        /// Add goals to the agent.
        /// </summary>
        /// <param name="goalList"></param>
        public void AddGoals(List<IGoapGoal> goalList);

        /// <summary>
        /// Manages to create a new plan for the Agent.
        /// </summary>
        /// <param name="worldGoapState"></param>
        /// <returns>Id of the goal whose plan has been created.</returns>
        public int CreateNewPlan(GoapState worldGoapState);
        
        /// <summary>
        /// Current number of action that the actual plan has.
        /// </summary>
        /// <returns>Number of actions left</returns>
        public int Count();
    }
}