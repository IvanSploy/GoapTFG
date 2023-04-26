using System.Collections.Generic;
using GoapTFG.Planner;

namespace GoapTFG.Base
{
    //Handles the GOAP planification and is who realices the actions.
    public class BasicAgent<TA, TB> : IAgent<TA, TB>
    {
        private Stack<GoapAction<TA, TB>> _currentPlan;
        private readonly List<GoapAction<TA, TB>> _actions;
        private readonly List<GoapGoal<TA, TB>> _goals;

        public string Name { get; }
        public PropertyGroup<TA, TB> CurrentState { get; set; }
        
        public BasicAgent(string name, List<GoapGoal<TA, TB>> goals, List<GoapAction<TA, TB>> actions = null)
        {
            Name = name;
            _actions = actions == null ? new List<GoapAction<TA, TB>>() : new List<GoapAction<TA, TB>>(actions);
            _goals = new List<GoapGoal<TA, TB>>(goals);
            SortGoals();
            _currentPlan = new Stack<GoapAction<TA, TB>>();
        }

        public BasicAgent(string name, GoapGoal<TA, TB> goapGoal, List<GoapAction<TA, TB>> actions = null)
        {
            Name = name;
            _actions = actions == null ? new List<GoapAction<TA, TB>>() : new List<GoapAction<TA, TB>>(actions);
            _goals = new List<GoapGoal<TA, TB>> { goapGoal };
            _currentPlan = new Stack<GoapAction<TA, TB>>();
        }

        public void AddAction(GoapAction<TA,TB> goapAction)
        {
            _actions.Add(goapAction);
        }
        
        public void AddActions(List<GoapAction<TA,TB>> actionList)
        {
            _actions.AddRange(actionList);
        }

        public void AddGoal(GoapGoal<TA, TB> goapGoal)
        {
            _goals.Add(goapGoal);
            SortGoals();
        }
        
        public void AddGoals(List<GoapGoal<TA, TB>> goalList)
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

        private bool CreatePlan(PropertyGroup<TA, TB> initialState, GoapGoal<TA, TB> goapGoal)
        {
            var plan = RegressivePlanner<TA, TB>.CreatePlan(initialState, goapGoal, _actions);
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