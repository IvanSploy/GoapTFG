using System.Collections.Generic;
using GoapTFG.Base;

namespace GoapTFG.Planner
{
    //Handles the GOAP planification and is who realices the actions.
    public class Agent<TA, TB> : IAgent<TA, TB>
    {
        private List<Action<TA, TB>> _currentPlan;
        private readonly List<Action<TA, TB>> _actions;
        private readonly List<Goal<TA, TB>> _goals;
        
        public Agent(List<Goal<TA, TB>> goals, List<Action<TA, TB>> actions = null)
        {
            _actions = actions == null ? new List<Action<TA, TB>>() : new List<Action<TA, TB>>(actions);
            _goals = new List<Goal<TA, TB>>(goals);
            OrderGoals();
            _currentPlan = new List<Action<TA, TB>>();
        }

        public Agent(Goal<TA, TB> goal, List<Action<TA, TB>> actions = null)
        {
            _actions = actions == null ? new List<Action<TA, TB>>() : new List<Action<TA, TB>>(actions);
            _goals = new List<Goal<TA, TB>> { goal };
            _currentPlan = new List<Action<TA, TB>>();
        }

        public void AddAction(Action<TA,TB> action)
        {
            _actions.Add(action);
        }
        
        public void AddActions(List<Action<TA,TB>> actions)
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
            _goals.Sort((g1, g2) => g2.PriorityLevel.CompareTo(g1.PriorityLevel));
        }

        /// <summary>
        /// Manages to create a new plan for the Agent.
        /// </summary>
        /// <param name="initialState"></param>
        /// <returns>Id of the goal whose plan has been created.</returns>
        public int CreateNewPlan(PropertyGroup<TA, TB> initialState)
        {
            if (_goals == null || _actions.Count == 0) return -1;
            int i = 0;
            bool created = false;
            while (i < _goals.Count && _currentPlan.Count == 0)
            {
                created = CreatePlan(initialState, _goals[i]);
                i++;
            }

            if (!created) return -1;
            return i-1;
        }

        private bool CreatePlan(PropertyGroup<TA, TB> initialState, Goal<TA, TB> goal)
        {
            var plan = NodeGenerator<TA, TB>.CreatePlan(initialState, goal, _actions);
            if (plan == null) return false;
            _currentPlan = plan;
            return true;
        }
        
        //Plan follower
        public PropertyGroup<TA, TB> DoPlan(PropertyGroup<TA, TB> worldState)
        {
            if (_currentPlan.Count == 0) return null;

            foreach (var action in _currentPlan)
            {
                worldState = action.PerformAction(worldState);
            }
            _currentPlan.Clear();
            return worldState;
        }

        public PropertyGroup<TA, TB> PlanStep(PropertyGroup<TA, TB> worldState)
        {
            if (_currentPlan.Count == 0) return null;

            worldState = _currentPlan[0].PerformAction(worldState);
            _currentPlan.RemoveAt(0);
            return worldState;
        }

        public int Count()
        {
            return _currentPlan.Count;
        }
    }
}