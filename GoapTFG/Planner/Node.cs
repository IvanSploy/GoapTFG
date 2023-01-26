using System;
using System.Collections.Generic;
using GoapTFG.Base;

namespace GoapTFG.Planner
{
    public class Node<TA, TB> : IComparable
    {
        //Consts
        public const int ACTION_COST = 1;
        
        //Properties
        public readonly PropertyGroup<TA, TB> PropertyGroup;
        public Node<TA, TB> Parent;
        public Base.Action<TA, TB> Action;
        public readonly List<Node<TA, TB>> Children;

        public int TotalCost;
        public int RealCost;
        public int EstimatedCost;
        public bool IsGoal;
        
        //Constructor
        public Node(PropertyGroup<TA, TB> propertyGroup)
        {
            PropertyGroup = propertyGroup;
            Children = new List<Node<TA, TB>>();
            TotalCost = 0;
            RealCost = 0;
            EstimatedCost = 0;
            IsGoal = false;
        }

        //AStar
        public Node<TA, TB> ApplyAction(Base.Action<TA, TB> action)
        {
            PropertyGroup<TA, TB> pg = action.ApplyAction(PropertyGroup);
            if (pg == null) return null;
            
            Node<TA,TB> node = new Node<TA, TB>(pg);
            node.Parent = this;
            AddChild(node);
            node.Action = action;
            return node;
        }
        
        public void Update(int parentRealCost, Goal<TA, TB> goal)
        {
            EstimatedCost = GetHeuristic(goal);
            IsGoal = EstimatedCost == 0;
            RealCost = ACTION_COST + parentRealCost;
            TotalCost = EstimatedCost + RealCost;
        }

        public int GetHeuristic(Goal<TA, TB> goal)
        {
            return goal.CountConflicts(PropertyGroup);
        }
        
        //Planner structure
        public void AddChild(Node<TA, TB> child)
        {
            Children.Add(child);
        }
        
        public void AddChildren(Node<TA,TB>[] children)
        {
            Children.AddRange(children);
        }

        public void ClearChildren()
        {
            Children.Clear();
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
            return PropertyGroup + "Costes: " + RealCost + " | " + EstimatedCost + " | " + TotalCost + "\n";
            //return GetHashCode() + "\n";
        }
        #endregion
    }
}