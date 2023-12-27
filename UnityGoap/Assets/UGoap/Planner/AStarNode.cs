using System;
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
        public AStarNode(PropertyGroup<TKey, TValue> state,
            GoapGoal<TKey, TValue> goal, INodeGenerator<TKey, TValue> generator) : base(state, goal, generator)
        {
            GCost = 0;
            HCost = 0;
        }

        protected override Node<TKey, TValue> CreateChildNode(PropertyGroup<TKey, TValue> state, GoapGoal<TKey, TValue> goapGoal,
            IGoapAction<TKey, TValue> goapAction, int cost = -1)
        {
            var aStarNode = new AStarNode<TKey, TValue>(state, goapGoal, Generator);
            aStarNode.Update(this, goapAction);
            if(cost >= 0) aStarNode.GCost = cost;
            Children.Add(aStarNode);
            return aStarNode;
        }

        public override void Update(Node<TKey, TValue> parent)
        {
            base.Update(parent);
            
            AStarNode<TKey, TValue> asnParent = (AStarNode<TKey, TValue>) parent;
            HCost = GetHeuristic();
            IsGoal = HCost == 0;
            GCost = Action.GetCost(new GoapStateInfo<TKey, TValue>(parent.State, parent.Goal)) + asnParent.GCost;
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
            text += " | Objetivo: " + Goal + "\n";
            return text;
        }
        #endregion
    }
}