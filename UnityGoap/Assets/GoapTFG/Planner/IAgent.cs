using System.Collections.Generic;
using GoapTFG.Base;

namespace GoapTFG.Planner
{
    public interface IAgent<TA, TB>
    {
        /// <summary>
        /// Add posible action to the agent.
        /// </summary>
        /// <param name="action">Action to be added</param>
        public void AddAction(Action<TA, TB> action);

        /// <summary>
        /// Add posible actions to the agent.
        /// </summary>
        /// <param name="actions">Actions to be added</param>
        public void AddActions(List<Action<TA, TB>> actions);
        
        /// <summary>
        /// Add goal to the agent.
        /// </summary>
        /// <param name="goal"></param>
        public void AddGoal(Goal<TA, TB> goal);
        
        /// <summary>
        /// Add goals to the agent.
        /// </summary>
        /// <param name="goals"></param>
        public void AddGoals(List<Goal<TA, TB>> goals);

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