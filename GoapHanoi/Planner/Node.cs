using System.Collections.Generic;
using GoapHanoi.Base;

namespace GoapHanoi.Planner
{
    public class Node<TA, TB>
    {
        //Properties
        public PropertyGroup<TA, TB> PropertyGroup;
        public Node<TA, TB> Parent;
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
        public void Update(Goal<TA, TB> goal)
        {
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
    }
}