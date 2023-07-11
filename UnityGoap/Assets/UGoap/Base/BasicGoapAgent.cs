using System.Collections.Generic;
using GoapTFG.Planner;

namespace GoapTFG.Base
{
    //Handles the GOAP planification and is who realices the actions.
    public class BasicGoapAgent<TKey, TValue> : IGoapAgent<TKey, TValue>
    {
        private Stack<IGoapAction<TKey, TValue>> _currentPlan;
        private readonly List<IGoapAction<TKey, TValue>> _actions;
        private readonly List<GoapGoal<TKey, TValue>> _goals;

        public string Name { get; }
        public PropertyGroup<TKey, TValue> CurrentState { get; set; }
        public GoapGoal<TKey, TValue> CurrentGoal { get; set; }
        
        public BasicGoapAgent(string name, List<GoapGoal<TKey, TValue>> goals, List<IGoapAction<TKey, TValue>> actions = null)
        {
            Name = name;
            _actions = actions == null ? new List<IGoapAction<TKey, TValue>>() : new List<IGoapAction<TKey, TValue>>(actions);
            _goals = new List<GoapGoal<TKey, TValue>>(goals);
            SortGoals();
            _currentPlan = new Stack<IGoapAction<TKey, TValue>>();
        }

        public BasicGoapAgent(string name, GoapGoal<TKey, TValue> goal, List<IGoapAction<TKey, TValue>> actions = null)
        {
            Name = name;
            _actions = actions == null ? new List<IGoapAction<TKey, TValue>>() : new List<IGoapAction<TKey, TValue>>(actions);
            _goals = new List<GoapGoal<TKey, TValue>> { goal };
            _currentPlan = new Stack<IGoapAction<TKey, TValue>>();
        }

        public void AddAction(IGoapAction<TKey,TValue> action)
        {
            _actions.Add(action);
        }
        
        public void AddActions(List<IGoapAction<TKey,TValue>> actionList)
        {
            _actions.AddRange(actionList);
        }

        public void AddGoal(GoapGoal<TKey, TValue> goal)
        {
            _goals.Add(goal);
            SortGoals();
        }
        
        public void AddGoals(List<GoapGoal<TKey, TValue>> goalList)
        {
            _goals.AddRange(goalList);
            SortGoals();
        }

        private void SortGoals()
        {
            _goals.Sort((g1, g2) => g2.PriorityLevel.CompareTo(g1.PriorityLevel));
        }
        
        public int CreateNewPlan(PropertyGroup<TKey, TValue> worldState)
        {
            if (_goals == null || _actions.Count == 0) return -1;
            int i = 0;
            bool created = false;
            while (i < _goals.Count && _currentPlan.Count == 0)
            {
                created = CreatePlan(worldState, _goals[i]);
                i++;
            }

            if (!created) return -1;
            return i-1;
        }

        private bool CreatePlan(PropertyGroup<TKey, TValue> initialState, GoapGoal<TKey, TValue> goal)
        {
            var plan = RegressivePlanner<TKey, TValue>.CreatePlan(initialState, goal, _actions);
            if (plan == null) return false;
            _currentPlan = plan;
            return true;
        }
        
        //Plan follower
        public PropertyGroup<TKey, TValue> DoPlan(PropertyGroup<TKey, TValue> worldState)
        {
            if (_currentPlan.Count == 0) return null;

            var stateInfo = new GoapStateInfo<TKey, TValue>(worldState, CurrentGoal);
            foreach (var action in _currentPlan)
            {
                worldState = action.Execute(stateInfo, this);
            }
            _currentPlan.Clear();
            return worldState;
        }

        public PropertyGroup<TKey, TValue> PlanStep(PropertyGroup<TKey, TValue> worldState)
        {
            if (_currentPlan.Count == 0) return null;

            var stateInfo = new GoapStateInfo<TKey, TValue>(worldState, CurrentGoal);
            worldState = _currentPlan.Pop().Execute(stateInfo, this);
            return worldState;
        }

        public int Count()
        {
            return _currentPlan.Count;
        }
    }
}