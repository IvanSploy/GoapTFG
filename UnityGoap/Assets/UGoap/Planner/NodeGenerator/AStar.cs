using System;
using System.Collections.Generic;
using UGoap.Base;
using UGoap.Learning;

namespace UGoap.Planner
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
        private Func<GoapConditions, GoapState, int> _customHeuristic;
        private ILearningConfig _learningConfig;
        
        //Factory
        private static readonly ObjectPool<Node> NodeFactory = new(() => new AStarNode());
        
        public AStar() { }
        
        public Node CreateNode(GoapState initialState, GoapConditions goal)
        {
            var node = NodeFactory.Get();
            node.Setup(this, initialState, goal);
            return node;
        }

        public void DisposeNode(Node node)
        {
            NodeFactory.Release(node);
        }

        public void SetHeuristic(Func<GoapConditions, GoapState, int> customHeuristic)
        {
            _customHeuristic = customHeuristic;
            Mode = HeuristicMode.Custom;
        }
        
        public void SetLearning(ILearningConfig learningConfig)
        {
            _learningConfig = learningConfig;
            Mode = HeuristicMode.Learning;
        }

        public Node Initialize(GoapState initialState, GoapConditions goal)
        {
            var goalState = new GoapConditions(goal);
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
                    original.Update(node.Parent, node.PreviousAction, node.PreviousActionInfo);
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
            return _customHeuristic.Invoke(node.Goal, node.InitialState);
        }
        
        private int GetLearning(Node node)
        {
            var state = _learningConfig.GetLearningStateCode(node.InitialState, node.Goal);
            return -(int)_learningConfig.Get(state, node.PreviousAction.Name);
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