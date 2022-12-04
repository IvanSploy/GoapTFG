using System;
using System.Collections.Generic;
using GoapHanoi.Base;

namespace GoapHanoi.Planner
{
    public class Node<TA, TB> : IComparable
    {
        //Properties
        public PropertyGroup<TA, TB> PropertyGroup;
        public Node<TA, TB> Parent;
        public Base.Action<TA, TB> Action;
        public readonly List<Node<TA, TB>> Children;

        public int Cost;
        public bool IsGoal;
        
        //Constructor
        public Node(PropertyGroup<TA, TB> propertyGroup)
        {
            PropertyGroup = propertyGroup;
            Children = new List<Node<TA, TB>>();
            Cost = 0;
            IsGoal = false;
        }

        //AStar
        public void ApplyAction(Base.Action<TA, TB> action)
        {
            PropertyGroup.App
        }
        
        public void Update(Base.Action<TA, TB> action, Goal<TA, TB> goal)
        {
            Action = action;
            Cost = GetHeuristic(goal);
            IsGoal = Cost == 0;
        }

        public int GetHeuristic(Goal<TA, TB> goal)
        {
            return goal.GetMismatches(PropertyGroup).Count();
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
            return Cost.CompareTo(objNode.Cost);
        }
    }
}