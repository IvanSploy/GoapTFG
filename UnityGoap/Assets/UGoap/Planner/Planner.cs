using System.Collections.Generic;
using UGoap.Base;
using static UGoap.Base.BaseTypes;

namespace UGoap.Planner
{
    public abstract class Planner<TKey, TValue>
    {
        //Stats
        protected static int nodesCreated = 0;
        protected static int nodesSkipped = 0;
        public static int actionsApplied = 0;
        
        //Data
        protected INodeGenerator<TKey, TValue> _nodeGenerator;
        protected Node<TKey, TValue> _current;
        protected GoapGoal<TKey, TValue> _goal;

        protected Planner(GoapGoal<TKey, TValue> goal,
            INodeGenerator<TKey, TValue> nodeGenerator)
        {
            _goal = goal;
            _nodeGenerator = nodeGenerator;
        }

        public static bool CheckEffectCompatibility(TValue currentValue, EffectType effectType, TValue actionValue,
            TValue desiredValue, ConditionType conditionType)
        {
            //Check if condition will be fulfilled.
            object resultValue = EvaluateEffect(currentValue, actionValue, effectType);
            if (EvaluateCondition(resultValue, desiredValue, conditionType))
            {
                return true;
            }
            
            //Is condition is not reached after evaluation.
            bool compatible;
            switch (effectType)
            {
                case EffectType.Add:
                case EffectType.Multiply:
                    switch (conditionType)
                    {
                        case ConditionType.GreaterThan:
                        case ConditionType.GreaterOrEqual:
                            compatible = true;
                            break;
                        default:
                            compatible = false;
                            break;
                    }
                    break;
                case EffectType.Subtract:
                case EffectType.Divide:
                    switch (conditionType)
                    {
                        case ConditionType.LessThan:
                        case ConditionType.LessOrEqual:
                            compatible = true;
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
        /// <param name="initialState"></param>
        /// <param name="actions"></param>
        /// <returns></returns>
        public abstract Stack<GoapActionData<TKey, TValue>> GeneratePlan(PropertyGroup<TKey, TValue> initialState,
            List<IGoapAction<TKey, TValue>> actions);
        
        /// <summary>
        /// Gets the final plan that the researcher has found.
        /// </summary>
        /// <param name="nodeGoal">Objective node</param>
        /// <returns>Stack of actions.</returns>
        public static Stack<GoapActionData<TKey, TValue>> GetPlan(Node<TKey, TValue> nodeGoal)
        {
            Stack<GoapActionData<TKey, TValue>> plan = new Stack<GoapActionData<TKey, TValue>>();
            while (nodeGoal.Parent != null)
            {
                
                //Debug.Log("Estado: " + nodeGoal.State + "| Goal: " + nodeGoal.Goal);
                var actionData = new GoapActionData<TKey, TValue>(nodeGoal.Action, nodeGoal.Parent.Goal);
                plan.Push(actionData);
                nodeGoal = nodeGoal.Parent;
            }
            return plan;
        }
        
        /// <summary>
        /// Gets the final inverted plan that the researcher has found.
        /// </summary>
        /// <param name="nodeGoal"></param>
        /// <returns></returns>
        public static Stack<GoapActionData<TKey, TValue>> GetInvertedPlan(Node<TKey, TValue> nodeGoal)
        {
            Stack<GoapActionData<TKey, TValue>> plan = GetPlan(nodeGoal);
            Stack<GoapActionData<TKey, TValue>> invertedPlan = new Stack<GoapActionData<TKey, TValue>>();
            foreach (var actionData in plan)
            {
                invertedPlan.Push(actionData);
            }
            return invertedPlan;
        }

        public static void DebugPlan(Node<TKey, TValue> node)
        {
            var debugLog = "Acciones para conseguir el objetivo: ";
            var actionNames = "";
            int count = 0;
            int cost = node.TotalCost;
            
            while (node.Parent != null)
            {
                actionNames += node.Action.Name + "\n";
                count++;
                node = node.Parent;
            }

            debugLog += count + "\n";
            debugLog += "con coste: " + cost + "\n";
            debugLog += actionNames + "\n";

            DebugRecord.AddRecord(debugLog);
        }
        
        protected void DebugInfo(Node<TKey, TValue> node)
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