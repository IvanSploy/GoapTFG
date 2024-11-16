using System.Collections.Generic;
using UGoap.Base;
using static UGoap.Base.BaseTypes;

namespace UGoap.Planner
{
    public abstract class Planner
    {
        protected readonly INodeGenerator _nodeGenerator;
        protected readonly IGoapAgent _agent;
        
        //Plan data
        protected IGoapGoal _goal;
        protected Node _current;
        
        //Stats
        protected static int _actionsApplied;
        protected static int _nodesCreated;
        private static int _nodesSkipped;

        protected Planner(INodeGenerator nodeGenerator, IGoapAgent agent)
        {
            _nodeGenerator = nodeGenerator;
            _agent = agent;
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
        public abstract Plan GeneratePlan(GoapState initialState,
            List<IGoapAction> actions);

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

            DebugRecord.AddRecord(debugLog);
        }
        
        protected void DebugInfo(Node node)
        {
            string info = "";
            info += "NODES CREATED: " + _nodesCreated + "\n";
            info += "NODES SKIPPED: " + _nodesSkipped + "\n";
            info += "ACTIONS APPLIED: " + _actionsApplied + "\n";
            _actionsApplied = 0;
            DebugPlan(node, _goal.Name);
            DebugRecord.AddRecord(info);
        }
    }
}