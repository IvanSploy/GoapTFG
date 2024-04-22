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
        public AStarNode(INodeGenerator nodeGenerator, GoapConditions goal, Func<GoapConditions,GoapState,int> heuristic) :
            base(nodeGenerator, goal, heuristic)
        {
            GCost = 0;
            HCost = 0;
        }
        
        public AStarNode(INodeGenerator nodeGenerator, GoapConditions goal, IQLearning qLearning) :
            base(nodeGenerator, goal, qLearning)
        {
            GCost = 0;
            HCost = 0;
        }

        protected override Node CreateChildNode(GoapConditions goal, IGoapAction action)
        {
            var aStarNode = UseLearning ? new AStarNode(NodeGenerator, goal, QLearning)
                : new AStarNode(NodeGenerator, goal, CustomHeuristic);
            aStarNode.Update(this, action);
            Children.Add(aStarNode);
            return aStarNode;
        }

        public override void Update(Node parent)
        {
            base.Update(parent);
            
            AStarNode asnParent = (AStarNode) parent;
            HCost = UseLearning ? GetLearning() : GetHeuristic(NodeGenerator.InitialState);
            GCost = PreviousAction.GetCost(parent.Goal) + asnParent.GCost;
        }

        /// <summary>
        /// Retrieves the value of the heuristic applied to this node.
        /// </summary>
        /// <returns>Heuristic cost.</returns>
        public int GetHeuristic(GoapState initialState)
        {
            return CustomHeuristic?.Invoke(Goal, initialState) ?? Goal.CountConflicts(initialState);
        }

        public int GetLearning() => -(int)(QLearning?.GetQValue(QLearning.ParseToStateCode(Parent.Goal), PreviousAction.Name) ?? 0);

        #region Overrides
        
        public override string ToString()
        {
            string text = "";
            if (PreviousAction == null) text += "Initial Node";
            else text += PreviousAction.Name;
            text += " | Costes: " + GCost + " | " + HCost + " | " + TotalCost + "\n";
            text += " | Objetivo: " + Goal + "\n";
            return text;
        }
        #endregion
    }
}