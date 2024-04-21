using System;
using System.Collections.Generic;
using UGoap.Base;
using UGoap.Learning;

namespace UGoap.Planner
{
    public class AStar : INodeGenerator
    {
        //Properties
        public GoapState InitialState { get; }
        
        //Fields
        private readonly SortedSet<Node> _openList; //Para acceder más rapidamente al elemento prioritario.
        private readonly HashSet<Node> _expandedNodes;
        private readonly Func<GoapConditions, GoapState, int> _customHeuristic;
        private readonly IQLearning _qLearning;
        
        public AStar(GoapState initialState, Func<GoapConditions, GoapState, int> customHeuristic = null)
        {
            InitialState = initialState;
            
            _openList = new SortedSet<Node>();
            _expandedNodes = new HashSet<Node>();
            _customHeuristic = customHeuristic;
        }
        
        public AStar(GoapState initialState, IQLearning qLearning)
        {
            InitialState = initialState;
            
            _openList = new SortedSet<Node>();
            _expandedNodes = new HashSet<Node>();
            _qLearning = qLearning;
        }

        public Node CreateInitialNode(GoapConditions goal)
        {
            var goalState = new GoapConditions(goal);
            AStarNode node = _qLearning != null ? new AStarNode(this, goalState, _qLearning)
                : new AStarNode(this, goalState, _customHeuristic);
            node.GCost = 0;
            node.HCost = _qLearning != null ? 0 : node.GetHeuristic(InitialState);
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

        public void AddChildToParent(Node parent, Node child)
        {
            //Si el nodo ya ha sido explorado.
            if (_expandedNodes.Contains(child))
            {
                _expandedNodes.TryGetValue(child, out var original);
                
                //Se actualiza el nodo original con la nueva información y sus hijos respectivamente
                //pudiendo afectar a algun nodo ubicado en la lista abierta.
                if (child.TotalCost < original.TotalCost)
                {
                    original.Update(parent, child.PreviousAction);
                    UpdateChildrenCost(original);
                }
            }
            //Si el nodo se encuentra en la lista abierta.
            else if (_openList.Contains(child))
            {
                _openList.TryGetValue(child, out var original);
                
                //En caso de que ya exista un nodo igual en la lista abierta,
                //se actualiza por el de menor valor de coste.
                if (child.TotalCost < original.TotalCost)
                {
                    _openList.Remove(original);
                    _openList.Add(child);
                }
            }
            //Si el nodo nunca había sido generado.
            else _openList.Add(child);
        }
        
        /// <summary>
        /// Update all the children of a node after a change of the parent.
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
                    aStarChild.Update(node);
                    UpdateChildrenCost(aStarChild);
                }
                else
                {
                    if (!_openList.Remove(aStarChild)) continue;
                    
                    aStarChild.Update(node);
                    _openList.Add(aStarChild);
                }
            }
        }
    }
}