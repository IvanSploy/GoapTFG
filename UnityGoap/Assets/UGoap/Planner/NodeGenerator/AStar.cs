using System;
using System.Collections.Generic;
using UGoap.Base;
using UGoap.Learning;

namespace UGoap.Planning
{
    public class AStar : INodeGenerator
    {
        public enum HeuristicMode
        {
            Default,
            Custom,
            Learning
        }
        
        //Properties
        public HeuristicMode Mode { get; private set; }
        
        //Fields
        private readonly SortedSet<Node> _openList = new();
        private readonly HashSet<Node> _expandedNodes = new();
        private Func<Conditions, State, int> _customHeuristic;
        private QLearning _qLearning;
        
        //Factory
        private static readonly ObjectPool<Node> NodeFactory = new(() => new AStarNode());
        
        public Node CreateNode(State initialState, Conditions goal)
        {
            var node = NodeFactory.Get();
            node.Setup(this, initialState, goal);
            return node;
        }

        public void DisposeNode(Node node)
        {
            NodeFactory.Release(node);
        }

        public void SetHeuristic(Func<Conditions, State, int> customHeuristic)
        {
            Mode = HeuristicMode.Custom;
            _customHeuristic = customHeuristic;
        }
        
        public void SetLearning(QLearning qLearning)
        {
            Mode = HeuristicMode.Learning;
            _qLearning = qLearning;
        }

        public Node Initialize(State initialState, Conditions goal)
        {
            var goalState = new Conditions(goal);
            AStarNode node = (AStarNode) CreateNode(initialState, goalState);
            node.GCost = 0;
            node.HCost = GetHeuristicCost(node);
            return node;
        }
        
        public Node GetNextNode(Node current)
        {
            _expandedNodes.Add(current);
            if (_openList.Count == 0) return null;
            Node node = _openList.Min;
            _openList.Remove(node);
            return node;
        }

        public void Add(Node node)
        {
            //Already expanded.
            if (_expandedNodes.Contains(node))
            {
                _expandedNodes.TryGetValue(node, out var original);
                
                //Update tree from affected node to open list nodes.
                if (node.TotalCost < original.TotalCost)
                {
                    original.Update(node.Parent, node.ActionData);
                    UpdateChildrenCost(original);
                }
                DisposeNode(node);
            }
            //Open list.
            else if (_openList.Contains(node))
            {
                _openList.TryGetValue(node, out var original);
                
                //Replace for the one in the open list.
                if (node.TotalCost < original.TotalCost)
                {
                    _openList.Remove(original);
                    _openList.Add(node);
                    DisposeNode(original);
                }
                else
                {
                    DisposeNode(node);
                }
            }
            //Completely new
            else _openList.Add(node);
        }
        
        public int GetHeuristicCost(Node node)
        {
            return Mode switch
            {
                HeuristicMode.Default => node.Goal.CountConflicts(node.InitialState),
                HeuristicMode.Custom => GetHeuristic(node),
                HeuristicMode.Learning => GetLearning(node),
                _ => 0
            };
        }
        
        private int GetHeuristic(Node node)
        {
            return _customHeuristic(node.Goal, node.InitialState);
        }
        
        private int GetLearning(Node node)
        {
            if (_qLearning.IsExploring()) return (int)Math.Round(_qLearning.GetExploreValue());
            return -(int)Math.Round(_qLearning.GetMaxValue(node.InitialState, node.Goal));
        }
        
        private void UpdateChildrenCost(Node node)
        {
            if (node.Children.Count == 0) return;
            
            foreach (var child in node.Children)
            {
                var aStarChild = (AStarNode)child;
                if (aStarChild.Children.Count != 0)
                {
                    aStarChild.SetParent(node);
                    UpdateChildrenCost(aStarChild);
                }
                else
                {
                    if (!_openList.Remove(aStarChild)) continue;
                    
                    aStarChild.SetParent(node);
                    _openList.Add(aStarChild);
                }
            }
        }

        public void Dispose()
        {
            foreach (var node in _openList)
            {
                DisposeNode(node);
            }
            _openList.Clear();
            
            foreach (var node in _expandedNodes)
            {
                DisposeNode(node);
            }
            _expandedNodes.Clear();
        }
    }
}