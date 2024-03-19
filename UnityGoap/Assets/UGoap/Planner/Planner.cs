using System;
using System.Collections.Generic;
using UGoap.Base;
using static UGoap.Base.BaseTypes;

namespace UGoap.Planner
{
    public abstract class Planner
    {
        //Stats
        protected static int nodesCreated = 0;
        protected static int nodesSkipped = 0;
        public static int actionsApplied = 0;
        
        //Data
        protected INodeGenerator _nodeGenerator;
        protected IGoapAgent _agent;
        protected Node _current;
        protected GoapGoal _goal;
        
        //Events
        public Action<Node> OnNodeCreated;
        public Action<Node> OnPlanCreated;

        protected Planner(INodeGenerator nodeGenerator, IGoapAgent agent)
        {
            _nodeGenerator = nodeGenerator;
            _agent = agent;
        }

        public static bool CheckEffectCompatibility(object currentValue, EffectType effectType, object actionValue,
            List<ConditionValue> conditions)
        {
            bool compatible = true;
            object resultValue = Evaluate(currentValue, effectType, actionValue);

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
                nodesSkipped++;
                //Debug.Log( currentValue + " | " + effectType + " | " + actionValue + " || " + resultValue + " | " + conditionType + " | " + desiredValue);
            }
            return compatible;
        }
        
        /// <summary>
        /// Generates the plan using the generator and the actions provided.
        /// </summary>
        /// <param name="initialGoapState"></param>
        /// <param name="actions"></param>
        /// <returns></returns>
        public abstract Plan GeneratePlan(GoapState initialGoapState,
            List<IGoapAction> actions);

        public void DebugPlan(Node node)
        {
            var debugLog = "Acciones para conseguir el objetivo: ";
            var actionNames = "";
            int count = 0;
            int cost = node.TotalCost;
            
            while (node.Parent != null)
            {
                actionNames += node.ParentAction.Name + "\n";
                count++;
                node = node.Parent;
            }

            debugLog += count + "\n";
            debugLog += "con coste: " + cost + "\n";
            debugLog += actionNames + "\n";

            DebugRecord.AddRecord(debugLog);
        }
        
        protected void DebugInfo(Node node)
        {
            string info = "";
            info += "NODOS CREADOS: " + nodesCreated + "\n";
            info += "NODOS SALTADOS: " + nodesSkipped + "\n";
            info += "ACCIONES RECORRIDAS: " + actionsApplied + "\n";
            actionsApplied = 0;
            DebugPlan(node);
            DebugRecord.AddRecord(info);
        }
    }
}