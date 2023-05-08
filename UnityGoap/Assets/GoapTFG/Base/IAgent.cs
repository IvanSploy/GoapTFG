using System.Collections.Generic;
using GoapTFG.Unity;

namespace GoapTFG.Base
{
    /// <summary>
    /// Represents a NPC with the hability to plan. Can also be treated as IEntity.
    /// </summary>
    /// <typeparam name="TA">Key type</typeparam>
    /// <typeparam name="TB">Value type</typeparam>
    public interface IAgent<TA, TB> : IEntity<TA, TB>
    {
        /// <summary>
        /// Add posible action to the agent.
        /// </summary>
        /// <param name="goapAction">Action to be added</param>
        public void AddAction(IGoapAction<TA, TB> goapAction);

        /// <summary>
        /// Add posible actions to the agent.
        /// </summary>
        /// <param name="actionList">Actions to be added</param>
        public void AddActions(List<IGoapAction<TA, TB>> actionList);
        
        /// <summary>
        /// Add goal to the agent.
        /// </summary>
        /// <param name="goapGoal"></param>
        public void AddGoal(GoapGoal<TA, TB> goapGoal);
        
        /// <summary>
        /// Add goals to the agent.
        /// </summary>
        /// <param name="goalList"></param>
        public void AddGoals(List<GoapGoal<TA, TB>> goalList);

        /// <summary>
        /// Manages to create a new plan for the Agent.
        /// </summary>
        /// <param name="initialState"></param>
        /// <returns>Id of the goal whose plan has been created.</returns>
        public int CreateNewPlan(PropertyGroup<TA, TB> initialState);
        /// <summary>
        /// Performs all of the actions in the actual plan all at once.
        /// </summary>
        /// <param name="worldState"></param>
        /// <returns></returns>
        public PropertyGroup<TA, TB> DoPlan(PropertyGroup<TA, TB> worldState);
        
        /// <summary>
        /// Performs the current action of the actual plan.
        /// </summary>
        /// <param name="worldState"></param>
        /// <returns></returns>
        public PropertyGroup<TA, TB> PlanStep(PropertyGroup<TA, TB> worldState);
        
        /// <summary>
        /// Current number of action that the actual plan has.
        /// </summary>
        /// <returns>Number of actions left</returns>
        public int Count();
    }
}