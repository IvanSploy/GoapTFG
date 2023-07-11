using System.Collections.Generic;
using GoapTFG.UGoap;

namespace GoapTFG.Base
{
    /// <summary>
    /// Represents a NPC with the hability to plan. Can also be treated as IEntity.
    /// </summary>
    /// <typeparam name="TKey">Key type</typeparam>
    /// <typeparam name="TValue">Value type</typeparam>
    public interface IGoapAgent<TKey, TValue> : IGoapEntity<TKey, TValue>
    {
        /// <summary>
        /// Add posible action to the agent.
        /// </summary>
        /// <param name="action">Action to be added</param>
        public void AddAction(IGoapAction<TKey, TValue> action);

        /// <summary>
        /// Add posible actions to the agent.
        /// </summary>
        /// <param name="actionList">Actions to be added</param>
        public void AddActions(List<IGoapAction<TKey, TValue>> actionList);
        
        /// <summary>
        /// Add goal to the agent.
        /// </summary>
        /// <param name="goal"></param>
        public void AddGoal(GoapGoal<TKey, TValue> goal);
        
        /// <summary>
        /// Add goals to the agent.
        /// </summary>
        /// <param name="goalList"></param>
        public void AddGoals(List<GoapGoal<TKey, TValue>> goalList);

        /// <summary>
        /// Manages to create a new plan for the Agent.
        /// </summary>
        /// <param name="worldState"></param>
        /// <returns>Id of the goal whose plan has been created.</returns>
        public int CreateNewPlan(PropertyGroup<TKey, TValue> worldState);
        /// <summary>
        /// Performs all of the actions in the actual plan all at once.
        /// </summary>
        /// <param name="worldState"></param>
        /// <returns></returns>
        public PropertyGroup<TKey, TValue> DoPlan(PropertyGroup<TKey, TValue> worldState);
        
        /// <summary>
        /// Performs the current action of the actual plan.
        /// </summary>
        /// <param name="worldState"></param>
        /// <returns></returns>
        public PropertyGroup<TKey, TValue> PlanStep(PropertyGroup<TKey, TValue> worldState);
        
        /// <summary>
        /// Current number of action that the actual plan has.
        /// </summary>
        /// <returns>Number of actions left</returns>
        public int Count();
    }
}