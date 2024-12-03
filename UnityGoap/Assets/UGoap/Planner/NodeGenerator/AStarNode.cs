using UGoap.Base;

namespace UGoap.Planner
{
    public class AStarNode : Node
    {
        //Properties
        public int HCost { get; set; }
        public int GCost { get; set; }

        public override int TotalCost => GCost + HCost;
        
        //Methods
        protected override Node CreateChild(GoapConditions goal, GoapAction action, GoapActionInfo actionInfo)
        {
            var child = _nodeGenerator.CreateNode(goal);
            child.Update(this, action, actionInfo);
            Children.Add(child);
            return child;
        }

        public override void UpdateCost()
        {
            AStarNode parent = (AStarNode) Parent;
            HCost = _nodeGenerator.GetHeuristicCost(this);
            GCost = PreviousAction.GetCost(parent.Goal) + parent.GCost;
        }
        
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