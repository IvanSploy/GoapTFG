using System.Collections.Generic;
using UGoap.Base;
using UGoap.Planner;

namespace UGoap.CSharp
{
    //Handles the GOAP planification and is who realices the actions.
    public class GoapAgent<TKey, TValue> : IGoapAgent<TKey, TValue>
    {
        private Stack<GoapActionData<TKey, TValue>> _currentPlan;
        private readonly List<IGoapAction<TKey, TValue>> _actions;
        private readonly List<GoapGoal<TKey, TValue>> _goals;

        public string Name { get; }
        public StateGroup<TKey, TValue> CurrentState { get; set; }
        public GoapGoal<TKey, TValue> CurrentGoal { get; set; }
        
        public GoapAgent(string name, List<GoapGoal<TKey, TValue>> goals, List<IGoapAction<TKey, TValue>> actions = null)
        {
            Name = name;
            _actions = actions == null ? new List<IGoapAction<TKey, TValue>>() : new List<IGoapAction<TKey, TValue>>(actions);
            _goals = new List<GoapGoal<TKey, TValue>>(goals);
            SortGoals();
            _currentPlan = new Stack<GoapActionData<TKey, TValue>>();
        }

        public GoapAgent(string name, GoapGoal<TKey, TValue> goal, List<IGoapAction<TKey, TValue>> actions = null)
        {
            Name = name;
            _actions = actions == null ? new List<IGoapAction<TKey, TValue>>() : new List<IGoapAction<TKey, TValue>>(actions);
            _goals = new List<GoapGoal<TKey, TValue>> { goal };
            _currentPlan = new Stack<GoapActionData<TKey, TValue>>();
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
        
        public int CreateNewPlan(StateGroup<TKey, TValue> worldState)
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

        private bool CreatePlan(StateGroup<TKey, TValue> initialState, GoapGoal<TKey, TValue> goal)
        {
            var plan = RegressivePlanner<TKey, TValue>.CreatePlan(initialState, goal, _actions);
            if (plan == null) return false;
            _currentPlan = plan;
            return true;
        }
        
        //Plan follower
        public StateGroup<TKey, TValue> DoPlan(StateGroup<TKey, TValue> worldState)
        {
            if (_currentPlan.Count == 0) return null;

            var stateInfo = new GoapStateInfo<TKey, TValue>(worldState, CurrentGoal);
            foreach (var actionData in _currentPlan)
            {
                worldState = actionData.Action.Execute(stateInfo, this);
            }
            _currentPlan.Clear();
            return worldState;
        }

        public StateGroup<TKey, TValue> PlanStep(StateGroup<TKey, TValue> worldState)
        {
            if (_currentPlan.Count == 0) return null;

            GoapActionData<TKey, TValue> actionData = _currentPlan.Pop();
            var stateInfo = new GoapStateInfo<TKey, TValue>(worldState, CurrentGoal);
            worldState = actionData.Action.Execute(stateInfo, this);
            return worldState;
        }

        public int Count()
        {
            return _currentPlan.Count;
        }
    }
}