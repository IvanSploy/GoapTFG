using System.Collections.Generic;
using GoapTFG.Planner;

namespace GoapTFG.Base
{
    //Handles the GOAP planification and is who realices the actions.
    public class Agent<TA, TB> : IAgent<TA, TB>
    {
        private Stack<Action<TA, TB>> _currentPlan;
        private readonly List<Action<TA, TB>> _actions;
        private readonly List<Goal<TA, TB>> _goals;

        public string Name { get; }
        public PropertyGroup<TA, TB> CurrentState { get; set; }
        
        public Agent(string name, List<Goal<TA, TB>> goals, List<Action<TA, TB>> actions = null)
        {
            Name = name;
            _actions = actions == null ? new List<Action<TA, TB>>() : new List<Action<TA, TB>>(actions);
            _goals = new List<Goal<TA, TB>>(goals);
            SortGoals();
            _currentPlan = new Stack<Action<TA, TB>>();
        }

        public Agent(string name, Goal<TA, TB> goal, List<Action<TA, TB>> actions = null)
        {
            Name = name;
            _actions = actions == null ? new List<Action<TA, TB>>() : new List<Action<TA, TB>>(actions);
            _goals = new List<Goal<TA, TB>> { goal };
            _currentPlan = new Stack<Action<TA, TB>>();
        }

        public void AddAction(Action<TA,TB> action)
        {
            _actions.Add(action);
        }
        
        public void AddActions(List<Action<TA,TB>> actionList)
        {
            _actions.AddRange(actionList);
        }

        public void AddGoal(Goal<TA, TB> goal)
        {
            _goals.Add(goal);
            SortGoals();
        }
        
        public void AddGoals(List<Goal<TA, TB>> goalList)
        {
            _goals.AddRange(goalList);
            SortGoals();
        }

        private void SortGoals()
        {
            _goals.Sort((g1, g2) => g2.PriorityLevel.CompareTo(g1.PriorityLevel));
        }
        
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
            var plan = RegressivePlanner<TA, TB>.CreatePlan(initialState, goal, _actions);
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

            worldState = _currentPlan.Pop().PerformAction(worldState);
            return worldState;
        }

        public int Count()
        {
            return _currentPlan.Count;
        }
    }
}