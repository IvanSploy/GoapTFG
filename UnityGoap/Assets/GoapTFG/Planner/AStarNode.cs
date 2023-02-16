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
            Goal<TA, TB> goal, INodeGenerator<TA, TB> generator) : base(propertyGroup, goal)
        {
            _generator = generator;
            GCost = 0;
            HCost = 0;
        }

        protected override Node<TA, TB> CreateChildNode(PropertyGroup<TA, TB> pg, Goal<TA, TB> goal, Base.Action<TA, TB> action)
        {
            AStarNode<TA,TB> aStarNode = new AStarNode<TA, TB>(pg, goal, _generator);
            aStarNode.Update(this, action);
            Children.Add(aStarNode);
            return aStarNode;
        }

        public override void Update(Node<TA, TB> parent)
        {
            base.Update(parent);
            
            AStarNode<TA, TB> asnParent = (AStarNode<TA, TB>) parent;
            HCost = GetHeuristic();
            IsGoal = HCost == 0;
            GCost = Action.GetCost(PropertyGroup) + asnParent.GCost;
            TotalCost = HCost + GCost;
        }

        /// <summary>
        /// Retrieves the value of the heuristic applied to this node.
        /// </summary>
        /// <returns>Heuristic cost.</returns>
        public int GetHeuristic()
        {
            return _generator.GetCustomHeuristic()?.Invoke(Goal, PropertyGroup) ?? Goal.CountConflicts(PropertyGroup);
        }

        #region Overrides

        public override string ToString()
        {
            string text = "";
            if (Action == null) text += "Initial Node";
            else text += Action.Name;
            text += " | Costes: " + GCost + " | " + HCost + " | " + TotalCost + "\n";
            return text;
        }
        #endregion
    }
}