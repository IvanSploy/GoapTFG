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
        private Func<ConditionGroup, State, int> _customHeuristic;
        private QLearning _learning;
        
        //Learning
        private readonly Dictionary<string, int> _exploreValues = new();
        private int _explorationMaxValue;
        
        //Factory
        private static readonly ObjectPool<Node> NodeFactory = new(() => new AStarNode());
        
        public Node CreateNode(State initialState, ConditionGroup goal)
        {
            var node = NodeFactory.Get();
            node.Setup(this, initialState, goal);
            return node;
        }

        public void DisposeNode(Node node)
        {
            NodeFactory.Release(node);
        }
        
        public Node Initialize(State initialState, ConditionGroup goal)
        {
            _exploreValues.Clear();
            var goalState = new ConditionGroup(goal);
            AStarNode node = (AStarNode) CreateNode(initialState, goalState);
            node.GCost = 0;
            node.HCost = GetHeuristicCost(node);
            return node;
        }

        public void SetHeuristic(Func<ConditionGroup, State, int> customHeuristic)
        {
            Mode = HeuristicMode.Custom;
            _customHeuristic = customHeuristic;
        }
        
        public void SetLearning(QLearning learning, int explorationMaxValue)
        {
            Mode = HeuristicMode.Learning;
            _learning = learning;
            _explorationMaxValue = explorationMaxValue;
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
            return Mode switch
            {
                HeuristicMode.Learning => GetGLearning(node),
                _ => node.PreviousAction.GetCost(node.Parent.Goal)
            };
        }
        
        public int GetHeuristicCost(Node node)
        {
            return Mode switch
            {
                HeuristicMode.Default => node.Goal.CountConflicts(node.InitialState),
                HeuristicMode.Custom => GetHeuristic(node),
                HeuristicMode.Learning => GetHLearning(node),
                _ => 0
            };
        }

        public int GetLearningCode(Node node)
        {
            return Mode switch
            {
                HeuristicMode.Learning => _learning.GetLearningCode(node.InitialState, node.Goal),
                _ => 0
            };
        }

        private int GetHeuristic(Node node)
        {
            return _customHeuristic(node.Goal, node.InitialState);
        }
        
        private int GetGLearning(Node node)
        {
            if (node.Parent == null) return 0;
            
            var explorationValue = GetExploreValue(node.PreviousAction);
            if(explorationValue > 0) return explorationValue;
            
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
        
        private int GetExploreValue(Action action)
        {
            if (_exploreValues.TryGetValue(action.Name, out var result)) return result;
            if(_learning.IsExploring()) result = Random.RangeToInt(1, _explorationMaxValue);
            _exploreValues[action.Name] = result;
            return result;
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