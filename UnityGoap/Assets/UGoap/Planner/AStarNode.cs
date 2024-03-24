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

        public override int TotalCost => GCost + HCost;

        //Constructor
        public AStarNode(GoapState state,
            GoapGoal goal, Func<GoapGoal,GoapState,int> generator) : base(state, goal, generator)
        {
            GCost = 0;
            HCost = 0;
        }
        
        public AStarNode(GoapState state,
            GoapGoal goal, IQLearning qLearning) : base(state, goal, qLearning)
        {
            GCost = 0;
            HCost = 0;
        }

        protected override Node CreateChildNode(GoapState goapState, GoapGoal goapGoal,
            IGoapAction goapAction, int cost = -1)
        {
            var aStarNode = UseLearning ? new AStarNode(goapState, goapGoal, QLearning)
                : new AStarNode(goapState, goapGoal, CustomHeuristic);
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
            HCost = UseLearning ? GetLearning() : GetHeuristic();
            GCost = ParentAction.GetCost(parent.State, parent.Goal) + asnParent.GCost;
        }

        /// <summary>
        /// Retrieves the value of the heuristic applied to this node.
        /// </summary>
        /// <returns>Heuristic cost.</returns>
        public int GetHeuristic()
        {
            return CustomHeuristic?.Invoke(Goal, State) ?? Goal.CountConflicts(State);
        }

        public int GetLearning() => -(int)(QLearning?.GetQValue(QLearning.ParseToStateCode(State), ParentAction.Name) ?? 0);

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