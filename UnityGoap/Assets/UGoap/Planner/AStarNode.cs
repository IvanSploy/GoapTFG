using System;
using System.Linq;
using UGoap.Base;

namespace UGoap.Planner
{
    public class AStarNode<TKey, TValue> : Node<TKey, TValue>, IComparable
    {
        //Properties
        public int HCost { get; set; }
        public int GCost { get; set; }

        public override int TotalCost => GCost + HCost;

        //Constructor
        public AStarNode(StateGroup<TKey, TValue> state,
            GoapGoal<TKey, TValue> goal, INodeGenerator<TKey, TValue> generator) : base(state, goal, generator)
        {
            GCost = 0;
            HCost = 0;
        }

        protected override Node<TKey, TValue> CreateChildNode(StateGroup<TKey, TValue> state, GoapGoal<TKey, TValue> goapGoal,
            IGoapAction<TKey, TValue> goapAction, int cost = -1)
        {
            var aStarNode = new AStarNode<TKey, TValue>(state, goapGoal, Generator);
            aStarNode.Update(this, goapAction);
            if(cost >= 0) aStarNode.GCost = cost;
            aStarNode.IsGoal = goapGoal.GetState().IsEmpty();
            Children.Add(aStarNode);
            return aStarNode;
        }

        public override void Update(Node<TKey, TValue> parent)
        {
            base.Update(parent);
            
            AStarNode<TKey, TValue> asnParent = (AStarNode<TKey, TValue>) parent;
            HCost = GetHeuristic();
            GCost = ParentAction.GetCost(new GoapStateInfo<TKey, TValue>(parent.State, parent.Goal)) + asnParent.GCost;
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
            if (ParentAction == null) text += "Initial Node";
            else text += ParentAction.Name;
            text += " | Costes: " + GCost + " | " + HCost + " | " + TotalCost + "\n";
            text += " | Objetivo: " + Goal + "\n";
            return text;
        }
        #endregion
    }
}