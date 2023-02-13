using System;
using System.Collections.Generic;
using System.Linq;
using GoapTFG.Base;
using UnityEngine;

namespace GoapTFG.Planner
{
    public class AStar<TA, TB> : INodeGenerator<TA, TB>
    {
        private const int ACTION_LIMIT = -1;
        
        private Node<TA, TB> _current;
        private readonly SortedSet<Node<TA, TB>> _openList; //Para acceder más rapidamente al elemento prioritario.
        private readonly HashSet<Node<TA, TB>> _expandedNodes;
        private readonly Func<Goal<TA, TB>, PropertyGroup<TA, TB>, int> _customHeuristic;

        private AStar(Func<Goal<TA, TB>, PropertyGroup<TA, TB>, int> newHeuristic)
        {
            _openList = new SortedSet<Node<TA, TB>>();
            _expandedNodes = new HashSet<Node<TA, TB>>();
            _customHeuristic = newHeuristic;
        }

        /// <summary>
        /// Creates a plan that finds using A* the path that finds the cheapest way to reach it.
        /// </summary>
        /// <param name="currentState">Current state of the world.</param>
        /// <param name="goal">Goal that is going to be reached.</param>
        /// <param name="actions">Actions aviable for the agent.</param>
        /// <param name="newHeuristic">Custom heuristic if needed</param>
        /// <returns>Stack of the plan actions.</returns>
        public static Stack<Base.Action<TA, TB>> CreatePlan(PropertyGroup<TA, TB> currentState, Goal<TA, TB> goal,
            List<Base.Action<TA, TB>> actions, Func<Goal<TA, TB>, PropertyGroup<TA, TB>, int> newHeuristic = null)
        {
            if (goal.IsReached(currentState)) return null;
            AStar<TA, TB> planner = new AStar<TA, TB>(newHeuristic);
            return planner.DoCreatePlan(currentState, goal, actions);
        }

        public Stack<Base.Action<TA, TB>> DoCreatePlan(PropertyGroup<TA, TB> currentState, Goal<TA, TB> goal,
            List<Base.Action<TA, TB>> actions)
        {
            if (currentState == null || goal == null || actions == null) throw new ArgumentNullException();
            if (actions.Count == 0) return null;
            
            _current = new Node<TA, TB>(currentState, this);
            var initialHeuristic = _current.GetHeuristic(goal);
            _current.EstimatedCost = initialHeuristic;
            _current.TotalCost = initialHeuristic;
            
            while (_current != null && !_current.IsGoal)
            {
                ExpandCurrentNode(actions, goal);
                _current = Pop(); //Get next node.
                if (ACTION_LIMIT > 0 && _current.ActionCount >= ACTION_LIMIT)  _current.IsGoal = true; //To avoid recursive loop behaviour.
            }

            if (_current == null) return null; //Plan doesnt exist.
            return GetPlan(_current); //Gets the plan of the goal node.
        }
        
        public Node<TA, TB> Pop()
        {
            if (_openList.Count == 0) return null;
            Node<TA, TB> node = _openList.Min;
            _openList.Remove(node);
            Console.Out.WriteLine("Extracted node:\n" + node);
            return node;
        }

        public void ExpandCurrentNode(List<Base.Action<TA, TB>> actions, Goal<TA, TB> goal)
        {
            _expandedNodes.Add(_current);
            foreach (var action in actions)
            {
                Node<TA, TB> newNode = _current.ApplyAction(action, goal);
                if(newNode == null) continue;
                
                if (_expandedNodes.Contains(newNode))
                {
                    _expandedNodes.TryGetValue(newNode, out var original);
                    //En caso de que el nodo expandido sea de menor coste,
                    //se actualiza el nodo original con la nueva información.
                    if (newNode.TotalCost < original.TotalCost)
                    {
                        original.Update(_current, action, goal);
                        UpdateOrder(original);
                    }
                }
                else
                    _openList.Add(newNode);
            }
        }

        /// <summary>
        /// Fixes the SortedSet problem of not allowing changing the values inside the Set.
        /// </summary>
        /// <param name="node"></param>
        private void UpdateOrder(Node<TA, TB> node)
        {
            if (node.GetChildren().Count != 0)
            {
                foreach (var child in node.GetChildren())
                {
                    UpdateOrder(child);
                }
            }

            if (_openList.Remove(node))
            {
                _openList.Add(node);
            }
        }
        
        private static Stack<Base.Action<TA, TB>> GetPlan(Node<TA, TB> nodeGoal)
        {
            Stack<Base.Action<TA, TB>> plan = new Stack<Base.Action<TA, TB>>();
            while (nodeGoal.Parent != null)
            {
                plan.Push(nodeGoal.Action);
                nodeGoal = nodeGoal.Parent;
            }

            return plan;
        }

        //Getter
        public Func<Goal<TA, TB>, PropertyGroup<TA, TB>, int> GetCustomHeuristic()
        {
            return _customHeuristic;
        }
    }
}