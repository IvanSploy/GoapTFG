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

        //Constructor
        public AStarNode(PropertyGroup<TKey, TValue> state,
            GoapGoal<TKey, TValue> goal, INodeGenerator<TKey, TValue> generator) : base(state, goal, generator)
        {
            GCost = 0;
            HCost = 0;
        }

        protected override Node<TKey, TValue> CreateChildNode(PropertyGroup<TKey, TValue> pg, GoapGoal<TKey, TValue> goapGoal, IGoapAction<TKey, TValue> goapAction)
        {
            var aStarNode = new AStarNode<TKey, TValue>(pg, goapGoal, Generator);
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
            GCost = Action.GetCost(new GoapStateInfo<TKey, TValue>(State, Goal)) + asnParent.GCost;
            TotalCost = HCost + GCost;
        }

        /// <summary>
        /// Retrieves the value of the heuristic applied to this node.
        /// </summary>
        /// <returns>Heuristic cost.</returns>
        public int GetHeuristic()
        {
            return Generator.GetCustomHeuristic()?.Invoke(Goal, State) ?? Goal.CountConflicts(State);
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