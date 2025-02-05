namespace LUGoap.Planning
{
    public class AStarNode : Node
    {
        //Properties
        public int HCost { get; set; }
        public int GCost { get; set; }

        public override int TotalCost => GCost + HCost;
        
        //Methods
        public override void UpdateCost()
        {
            AStarNode parent = (AStarNode) Parent;
            GCost = _nodeGenerator.GetCost(this) + parent.GCost;
            HCost = _nodeGenerator.GetHeuristicCost(this);
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