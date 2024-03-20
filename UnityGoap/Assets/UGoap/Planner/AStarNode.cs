using System;
using System.Linq;
using UGoap.Base;
using UGoap.Learning;

namespace UGoap.Planner
{
    public class AStarNode : Node
    {
        //Properties
        public int HCost { get; set; }
        public int GCost { get; set; }
        public int LCost { get; set; }

        public override int TotalCost => GCost + HCost + LCost;

        //Constructor
        public AStarNode(GoapState state,
            GoapGoal goal, Func<GoapGoal,GoapState,int> generator, IQLearning qLearning) : base(state, goal, generator, qLearning)
        {
            GCost = 0;
            HCost = 0;
            LCost = 0;
        }

        protected override Node CreateChildNode(GoapState goapState, GoapGoal goapGoal,
            IGoapAction goapAction, int cost = -1)
        {
            var aStarNode = new AStarNode(goapState, goapGoal, CustomHeuristic, QLearning);
            aStarNode.Update(this, goapAction);
            if(cost >= 0) aStarNode.GCost = cost;
            aStarNode.IsGoal = goapGoal.GetState().IsEmpty();
            Children.Add(aStarNode);
            return aStarNode;
        }

        public override void Update(Node parent)
        {
            base.Update(parent);
            
            AStarNode asnParent = (AStarNode) parent;
            HCost = GetHeuristic();
            LCost = -GetQValue();
            GCost = ParentAction.GetCost(parent.State, parent.Goal) + asnParent.GCost;
        }

        /// <summary>
        /// Retrieves the value of the heuristic applied to this node.
        /// </summary>
        /// <returns>Heuristic cost.</returns>
        public int GetHeuristic()
        {
            return 0;
            //return CustomHeuristic?.Invoke(Goal, State) ?? Goal.CountConflicts(State);
        }

        public int GetQValue() => (int)(QLearning?.GetQValue(QLearning.ParseToStateCode(State), ParentAction.Name) ?? 0);

        #region Overrides
        
        public override string ToString()
        {
            string text = "";
            if (ParentAction == null) text += "Initial Node";
            else text += ParentAction.Name;
            text += " | Costes: " + GCost + " | " + HCost + " | " + TotalCost + "\n";
            text += " | Objetivo: " + Goal + "\n";
            return text;
        }
        #endregion
    }
}