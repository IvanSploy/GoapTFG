using System;
using System.Collections.Generic;
using GoapTFG.Base;

namespace GoapTFG.Planner
{
    public class AStarNode<TA, TB> : Node<TA, TB>
    {
        //Properties
        public int HCost { get; set; }
        public int GCost { get; set; }
        
        //Fields
        private readonly INodeGenerator<TA, TB> _generator;
        
        //Constructor
        public AStarNode(PropertyGroup<TA, TB> propertyGroup,
            GoapGoal<TA, TB> goapGoal, INodeGenerator<TA, TB> generator) : base(propertyGroup, goapGoal)
        {
            _generator = generator;
            GCost = 0;
            HCost = 0;
        }

        protected override Node<TA, TB> CreateChildNode(PropertyGroup<TA, TB> pg, GoapGoal<TA, TB> goapGoal, Base.GoapAction<TA, TB> goapAction)
        {
            AStarNode<TA,TB> aStarNode = new AStarNode<TA, TB>(pg, goapGoal, _generator);
            aStarNode.Update(this, goapAction);
            Children.Add(aStarNode);
            return aStarNode;
        }

        public override void Update(Node<TA, TB> parent)
        {
            base.Update(parent);
            
            AStarNode<TA, TB> asnParent = (AStarNode<TA, TB>) parent;
            HCost = GetHeuristic();
            IsGoal = HCost == 0;
            GCost = GoapAction.GetCost(PropertyGroup) + asnParent.GCost;
            TotalCost = HCost + GCost;
        }

        /// <summary>
        /// Retrieves the value of the heuristic applied to this node.
        /// </summary>
        /// <returns>Heuristic cost.</returns>
        public int GetHeuristic()
        {
            return _generator.GetCustomHeuristic()?.Invoke(GoapGoal, PropertyGroup) ?? GoapGoal.CountConflicts(PropertyGroup);
        }

        #region Overrides

        public override string ToString()
        {
            string text = "";
            if (GoapAction == null) text += "Initial Node";
            else text += GoapAction.Name;
            text += " | Costes: " + GCost + " | " + HCost + " | " + TotalCost + "\n";
            return text;
        }
        #endregion
    }
}