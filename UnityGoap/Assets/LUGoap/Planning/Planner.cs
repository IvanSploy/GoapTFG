using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LUGoap.Base;
using static LUGoap.Base.BaseTypes;
using Action = LUGoap.Base.Action;

namespace LUGoap.Planning
{
    public abstract class Planner
    {
        protected readonly INodeGenerator _nodeGenerator;
        
        //Plan data
        protected State InitialState;
        protected Goal _goal;
        protected Node _current;
        
        //Stats
        protected static int _actionsApplied;
        protected static int _nodesCreated;
        private static int _nodesSkipped;

        protected Planner(INodeGenerator nodeGenerator)
        {
            _nodeGenerator = nodeGenerator;
        }
        
        public Plan CreatePlan(State initialState, Goal goal, List<Action> actions)
        {
            InitialState = initialState;
            _goal = goal;
            
            if (goal.IsGoal(initialState)) return null;
            var plan = GeneratePlan(actions);
            _nodeGenerator.Dispose();
            return plan;
        }
        
        public async Task<Plan> CreatePlanAsync(State initialState, Goal goal, List<Action> actions)
        {
            InitialState = initialState;
            _goal = goal;
            
            if (goal.IsGoal(initialState)) return null;
            var plan = await Task.Run(() => GeneratePlan(actions));
            
            _nodeGenerator.Dispose();
            return plan;
        }

        public static bool CheckEffectCompatibility(object initialValue, EffectType effectType,
            object actionValue, Condition condition)
        {
            object resultValue = Evaluate(initialValue, effectType, actionValue);
            
            if (condition.Check(resultValue)) return true;

            var initialDistance = Math.Abs(condition.GetDistance(initialValue));
            var finalDistance = Math.Abs(condition.GetDistance(resultValue));

            if (finalDistance >= initialDistance) return false;
            
            _nodesSkipped++;
            //Debug.Log( currentValue + " | " + effectType + " | " + actionValue + " || " + resultValue + " | " + conditionType + " | " + desiredValue);
            return true;
        }
        
        /// <summary>
        /// Generates the plan using the generator and the actions provided.
        /// </summary>
        /// <param name="initialState"></param>
        /// <param name="actions"></param>
        /// <returns></returns>
        protected abstract Plan GeneratePlan(List<Action> actions);

        public void DebugPlan(Node node, string goalName)
        {
            var debugLog = "Actions to reach goal: " + goalName + "\n";
            var actionNames = "";
            int count = 0;
            int cost = node.TotalCost;
            
            while (node.Parent != null)
            {
                actionNames += node.PreviousAction.Name + "\n";
                count++;
                node = node.Parent;
            }

            debugLog += count + "\n";
            debugLog += $"with cost: {cost}\n";
            debugLog += $"{actionNames}\n";

            DebugRecord.Record(debugLog);
        }
        
        protected void DebugInfo(Node node)
        {
            string info = "";
            info += "NODES CREATED: " + _nodesCreated + "\n";
            info += "NODES SKIPPED: " + _nodesSkipped + "\n";
            info += "ACTIONS APPLIED: " + _actionsApplied + "\n";
            _actionsApplied = 0;
            DebugPlan(node, _goal.Name);
            DebugRecord.Record(info);
        }
    }
}