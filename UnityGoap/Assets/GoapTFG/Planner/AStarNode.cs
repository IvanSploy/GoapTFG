using System;
using System.Collections.Generic;
using GoapTFG.Base;

namespace GoapTFG.Planner
{
    public class AStarNode<TKey, TValue> : Node<TKey, TValue>
    {
        //Properties
        public int HCost { get; set; }
        public int GCost { get; set; }
        
        //Fields
        private readonly INodeGenerator<TKey, TValue> _generator;
        
        //Constructor
        public AStarNode(PropertyGroup<TKey, TValue> propertyGroup,
            GoapGoal<TKey, TValue> goapGoal, INodeGenerator<TKey, TValue> generator) : base(propertyGroup, goapGoal)
        {
            _generator = generator;
            GCost = 0;
            HCost = 0;
        }

        protected override Node<TKey, TValue> CreateChildNode(PropertyGroup<TKey, TValue> pg, GoapGoal<TKey, TValue> goapGoal, IGoapAction<TKey, TValue> goapAction)
        {
            var aStarNode = new AStarNode<TKey, TValue>(pg, goapGoal, _generator);
            aStarNode.Update(this, goapAction);
            Children.Add(aStarNode);
            return aStarNode;
        }

        public override void Update(Node<TKey, TValue> parent)
        {
            base.Update(parent);
            
            AStarNode<TKey, TValue> asnParent = (AStarNode<TKey, TValue>) parent;
            HCost = GetHeuristic();
            IsGoal = HCost == 0;
            GCost = GoapAction.GetCost(new GoapStateInfo<TKey, TValue>(PropertyGroup, GoapGoal)) + asnParent.GCost;
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