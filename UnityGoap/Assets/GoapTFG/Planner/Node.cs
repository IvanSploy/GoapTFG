using System;
using System.Collections.Generic;
using GoapTFG.Base;

namespace GoapTFG.Planner
{
    public class Node<TA, TB> : IComparable
    {
        //Properties
        public readonly PropertyGroup<TA, TB> PropertyGroup;
        public Node<TA, TB> Parent;
        public Base.Action<TA, TB> Action;
        public readonly List<Node<TA, TB>> Children;

        public int TotalCost;
        public int RealCost;
        public int EstimatedCost;
        public int ActionCount;
        public bool IsGoal;
        
        //Constructor
        public Node(PropertyGroup<TA, TB> propertyGroup)
        {
            PropertyGroup = propertyGroup;
            Children = new List<Node<TA, TB>>();
            TotalCost = 0;
            RealCost = 0;
            EstimatedCost = 0;
            ActionCount = 0;
            IsGoal = false;
        }

        //AStar
        public Node<TA, TB> ApplyAction(Base.Action<TA, TB> action, Goal<TA, TB> goal)
        {
            PropertyGroup<TA, TB> pg = action.ApplyAction(PropertyGroup);
            if (pg == null) return null;
            
            Node<TA,TB> node = new Node<TA, TB>(pg);
            node.Update(this, action, goal);
            AddChild(node);
            return node;
        }
        
        public void Update(Node<TA, TB> parent, Base.Action<TA, TB> action, Goal<TA, TB> goal)
        {
            //Se define la relación padre hijo.
            Action = action;
            Parent = parent;
            
            //Se definen los costes
            EstimatedCost = GetHeuristic(goal);
            IsGoal = EstimatedCost == 0;
            RealCost = Action.Cost + parent.RealCost;
            TotalCost = EstimatedCost + RealCost;
            ActionCount = parent.ActionCount + 1;
            
            //En caso de que tenga hijos se actualizan.
            foreach (var child in Children)
            {
                child.Update(this, child.Action, goal);
            }
        }

        public int GetHeuristic(Goal<TA, TB> goal)
        {
            return goal.CountConflicts(PropertyGroup);
        }
        
        //Planner structure
        private void AddChild(Node<TA, TB> child)
        {
            Children.Add(child);
        }

        public int CompareTo(object obj)
        {
            if (obj == null) return -1;
            if (this == obj) return 0;
            if (obj.GetType() != GetType()) return -1;

            Node<TA, TB> objNode = (Node<TA, TB>)obj;
            return TotalCost.CompareTo(objNode.TotalCost);
        }

        #region Overrides
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (this == obj) return true;
            if (obj.GetType() != GetType()) return false;

            Node<TA, TB> objNode = (Node<TA, TB>)obj;
            
            return PropertyGroup.Equals(objNode.PropertyGroup);
        }

        public override int GetHashCode()
        {
            return PropertyGroup.GetHashCode();
        }

        public override string ToString()
        {
            string text = "";
            if (Action == null) text += "Initial Node";
            else text += Action.Name;
            text += " | Costes: " + RealCost + " | " + EstimatedCost + " | " + TotalCost + "\n";
            return text;
        }
        #endregion
    }
}