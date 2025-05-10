using System;
using System.Collections.Generic;
using QGoap.Base;
using QGoap.Learning;
using Action = QGoap.Base.Action;
using Random = QGoap.Base.Random;

namespace QGoap.Planning
{
    public class AStar : INodeGenerator
    {
        //Fields
        private readonly SortedSet<Node> _openList = new();
        private readonly HashSet<Node> _expandedNodes = new();
        private QLearning _learning;
        
        //Learning
        private int _explorationCost;
        
        //Factory
        private static readonly ObjectPool<Node> NodeFactory = new(() => new AStarNode());
        
        public Node CreateNode(State initialState, ConditionGroup goal)
        {
            var node = new AStarNode();
            //var node = NodeFactory.Get();
            node.Setup(this, initialState, goal);
            node.IsExploring = _learning?.IsExploring() ?? false;
            return node;
        }

        public void DisposeNode(Node node)
        {
            node.ClearRelationships();
            NodeFactory.Release(node);
        }
        
        public Node Initialize(State initialState, ConditionGroup goal)
        {
            var goalState = new ConditionGroup(goal);
            AStarNode node = (AStarNode) CreateNode(initialState, goalState);
            node.GCost = 0;
            node.HCost = GetHeuristicCost(node);
            return node;
        }
        
        public void SetLearning(QLearning learning, int explorationCost)
        {
            _learning = learning;
            _explorationCost = explorationCost;
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
        
        public ActionSettings CreateSettings(Node node, Action action)
        {
            var actionSettings = new ActionSettings
            {
                InitialState = node.InitialState,
                Goal = node.Goal,
                GlobalLearningCode = GetLearningCode(node)
            };
            
            return action.CreateSettings(actionSettings);
        }
        
        public int GetCost(Node node)
        {
            if (_learning is null) return node.PreviousAction.GetCost(node.Parent.Goal);
            return GetGLearning(node);
        }
        
        public int GetHeuristicCost(Node node)
        {
            var heuristic = 0;
            foreach (var conditionPair in node.Goal)
            {
                heuristic += Math.Abs(conditionPair.Value.GetDistance(node.InitialState.TryGetOrDefault(conditionPair.Key)));
            }

            return heuristic;

            return GetHLearning(node);
        }

        public int GetLearningCode(Node node)
        {
            return _learning?.GetLearningCode(node.InitialState, node.Goal) ?? 0;
        }
        
        private int GetGLearning(Node node)
        {
            if (node.Parent == null) return 0;
            
            if(node.Parent.IsExploring)
                return node.Parent.Children.Count * _explorationCost + 1;
            
            var learningCode = GetLearningCode(node.Parent);
            return GetCost(_learning.Get(learningCode, node.PreviousAction.Name));
        }
        
        private int GetHLearning(Node node)
        {
            var learningCode = GetLearningCode(node);
            return GetCost(_learning.GetMax(learningCode));;
        }

        private int GetCost(float qValue)
        {
            var value = -(int)Math.Round(qValue) + (int)Math.Round(_learning.MaxValue) + 1;
            return Math.Max(value, 1);
        }
        
        private void UpdateChildrenCost(Node node)
        {
            if (node.Children.Count == 0) return;
            
            foreach (var child in node.Children)
            {
                var aStarChild = (AStarNode)child;
                aStarChild.UpdateCost();
                
                if (aStarChild.Children.Count != 0)
                {
                    UpdateChildrenCost(aStarChild);
                }
                else
                {
                    if (!_openList.Remove(aStarChild)) continue;
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