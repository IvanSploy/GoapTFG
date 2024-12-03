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
        public GoapState InitialState { get; }
        public HeuristicMode Mode { get; private set; }
        
        //Fields
        private readonly SortedSet<Node> _openList = new();
        private readonly HashSet<Node> _expandedNodes = new();
        private Func<GoapConditions, GoapState, int> _customHeuristic;
        private ILearningConfig _learningConfig;
        
        //Factory
        private static readonly ObjectPool<Node> NodeFactory = new(() => new AStarNode());
        
        public AStar(GoapState initialState)
        {
            InitialState = initialState;
        }
        
        public Node CreateNode(GoapConditions goal)
        {
            var node = NodeFactory.Get();
            node.Setup(this, InitialState, goal);
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

        public Node Initialize(GoapConditions goal)
        {
            var goalState = new GoapConditions(goal);
            AStarNode node = (AStarNode) CreateNode(goalState);
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
                HeuristicMode.Default => node.Goal.CountConflicts(InitialState),
                HeuristicMode.Custom => GetHeuristic(node),
                HeuristicMode.Learning => GetLearning(node),
                _ => 0
            };
        }
        
        /// <summary>
        /// Apply all the children of a node after a change of the parent.
        /// It could change the order of the nodes in the Open List.
        /// </summary>
        /// <param name="node">Parent Node</param>
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

        private int GetHeuristic(Node node)
        {
            return _customHeuristic.Invoke(node.Goal, InitialState);
        }
        
        private int GetLearning(Node node)
        {
            var state = _learningConfig.GetLearningStateCode(InitialState, node.Goal);
            return -(int)_learningConfig.Get(state, node.PreviousAction.Name);
        }

        public void Dispose()
        {
            foreach (var node in _openList)
            {
                DisposeNode(node);
            }
            
            foreach (var node in _expandedNodes)
            {
                DisposeNode(node);
            }
        }
    }
}