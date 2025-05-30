using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GoapTFG.Base;

namespace GoapTFG.Planner
{
    //Handles the GOAP planification and is who realices the actions.
    public class Agent<TA, TB>
    {
        private List<Base.Action<TA, TB>> _currentPlan;
        private readonly List<Base.Action<TA, TB>> _actions;
        private readonly List<Goal<TA, TB>> _goals;
        
        public Agent(List<Goal<TA, TB>> goals, List<Base.Action<TA, TB>> actions = null)
        {
            _actions = actions == null ? new List<Base.Action<TA, TB>>() : new List<Base.Action<TA, TB>>(actions);
            _goals = new List<Goal<TA, TB>>(goals);
            OrderGoals();
            _currentPlan = null;
        }
        
        public Agent(Goal<TA, TB> goal, List<Base.Action<TA, TB>> actions = null)
        {
            _actions = actions == null ? new List<Base.Action<TA, TB>>() : new List<Base.Action<TA, TB>>(actions);
            _goals = new List<Goal<TA, TB>> { goal };
            _currentPlan = null;
        }

        /// <summary>
        /// Add posible action to the agent, this can be used by the sensors.
        /// </summary>
        /// <param name="action">Action to be added</param>
        public void AddAction(Base.Action<TA,TB> action)
        {
            _actions.Add(action);
        }
        
        /// <summary>
        /// Add posible actions to the agent, this can be used by the sensors.
        /// </summary>
        /// <param name="actions">Actions to be added</param>
        public void AddActions(List<Base.Action<TA,TB>> actions)
        {
            _actions.AddRange(actions);
        }

        public void AddGoal(Goal<TA, TB> goal)
        {
            _goals.Add(goal);
            OrderGoals();
        }
        
        public void AddGoals(List<Goal<TA, TB>> goals)
        {
            _goals.AddRange(goals);
            OrderGoals();
        }

        private void OrderGoals()
        {
            _goals.Sort((g1, g2) =>
            {
                return g2.PriorityLevel.CompareTo(g1.PriorityLevel);
            });
        }

        /// <summary>
        /// Manages to create a new plan for the Agent.
        /// </summary>
        /// <param name="initialState"></param>
        /// <returns>Id of the goal whose plan has been created.</returns>
        public int Update(PropertyGroup<TA, TB> initialState)
        {
            if (_goals == null || _actions.Count == 0) return -1;
            int i = 0;
            bool created = false;
            while (i < _goals.Count && _currentPlan == null)
            {
                created = CreatePlan(initialState, _goals[i]);
                i++;
            }

            if (!created) return -1;
            return i-1;
        }

        private bool CreatePlan(PropertyGroup<TA, TB> initialState, Goal<TA, TB> goal)
        {
            _currentPlan = NodeGenerator<TA, TB>.CreatePlan(initialState, goal, _actions);
            return _currentPlan != null;
        }
        
        //Plan follower
        public PropertyGroup<TA, TB> DoPlan(PropertyGroup<TA, TB> worldState)
        {
            if (_currentPlan == null || _currentPlan.Count == 0) return worldState;

            for (int i = 0; i < _currentPlan.Count; i++)
            {
                worldState = _currentPlan[i].PerformAction(worldState);
            }
            return worldState;
        }

        public PropertyGroup<TA, TB> PlanStep(PropertyGroup<TA, TB> worldState)
        {
            if (_currentPlan == null || _currentPlan.Count == 0) return worldState;

            worldState = _currentPlan[0].PerformAction(worldState);
            _currentPlan.RemoveAt(0);
            return worldState;
        }

        public int Count()
        {
            if (_currentPlan == null) return 0;
            return _currentPlan.Count;
        }
    }
}