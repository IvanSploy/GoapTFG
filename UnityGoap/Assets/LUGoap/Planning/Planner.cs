using System.Collections.Generic;
using System.Threading.Tasks;
using LUGoap.Base;
using static LUGoap.Base.BaseTypes;

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

        public static bool CheckEffectCompatibility(object initialValue, EffectType effectType, object actionValue,
            List<ConditionValue> conditions)
        {
            bool compatible = true;
            object resultValue = Evaluate(initialValue, effectType, actionValue);

            for (var i = 0; i < conditions.Count && compatible; i++)
            {
                var condition = conditions[i];
                
                //Check if condition will be fulfilled.
                if (Evaluate(resultValue, condition.ConditionType, condition.Value))
                {
                    continue;
                }

                //Is condition is not reached after evaluation.
                switch (effectType)
                {
                    case EffectType.Add:
                    case EffectType.Multiply:
                        switch (condition.ConditionType)
                        {
                            //TODO Decide if not equal should be allowed.
                            case ConditionType.NotEqual:
                            case ConditionType.GreaterThan:
                            case ConditionType.GreaterOrEqual:
                                break;
                            default:
                                compatible = false;
                                break;
                        }

                        break;
                    case EffectType.Subtract:
                    case EffectType.Divide:
                        switch (condition.ConditionType)
                        {
                            case ConditionType.NotEqual:
                            case ConditionType.LessThan:
                            case ConditionType.LessOrEqual:
                                break;
                            default:
                                compatible = false;
                                break;
                        }
                        break;
                    default:
                        compatible = false;
                        break;
                }
            }

            if (!compatible)
            {
                _nodesSkipped++;
                //Debug.Log( currentValue + " | " + effectType + " | " + actionValue + " || " + resultValue + " | " + conditionType + " | " + desiredValue);
            }
            return compatible;
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