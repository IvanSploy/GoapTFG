using System;
using System.Collections.Generic;
using GoapHanoi.Base;

namespace GoapHanoi.Planner
{
    public class Node<TA, TB> : IComparable
    {
        //Consts
        public const int ACTION_COST = 1;
        
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
        public Node<TA, TB> ApplyAction(Base.Action<TA, TB> action)
        {
            PropertyGroup<TA, TB> pg = Action.CheckApplyAction(PropertyGroup);
            if (pg == null) return null;
            
            Node<TA,TB> node = new Node<TA, TB>(pg);
            node.Parent = this;
            AddChild(node);
            node.Action = action;
            return node;
        }
        
        public void Update(Goal<TA, TB> goal)
        {
            Cost = GetHeuristic(goal);
            IsGoal = Cost == 0;
            Cost += ACTION_COST;
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