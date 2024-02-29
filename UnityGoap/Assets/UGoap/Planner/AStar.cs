using System;
using System.Collections.Generic;
using UGoap.Base;

namespace UGoap.Planner
{
    public class AStar<TKey, TValue> : INodeGenerator<TKey, TValue>
    {
        private readonly SortedSet<Node<TKey, TValue>> _openList; //Para acceder más rapidamente al elemento prioritario.
        private readonly HashSet<Node<TKey, TValue>> _expandedNodes;
        private readonly Func<GoapGoal<TKey, TValue>, GoapState<TKey, TValue>, int> _customHeuristic;
        private readonly GoapState<TKey, TValue> _initialGoapState;
        
        public AStar(GoapState<TKey, TValue> initialGoapState, Func<GoapGoal<TKey, TValue>, GoapState<TKey, TValue>, int> customHeuristic = null)
        {
            _initialGoapState = initialGoapState;
            
            _openList = new SortedSet<Node<TKey, TValue>>();
            _expandedNodes = new HashSet<Node<TKey, TValue>>();
            _customHeuristic = customHeuristic;
        }

        public Node<TKey, TValue> CreateInitialNode(GoapState<TKey, TValue> currentGoapState, GoapGoal<TKey, TValue> goal)
        {
            var goalState = new GoapGoal<TKey, TValue>(goal);
            AStarNode<TKey, TValue> node = new AStarNode<TKey, TValue>(currentGoapState, goalState, this);
            var initialHeuristic = node.GetHeuristic();
            node.GCost = 0;
            node.HCost = initialHeuristic;
            return node;
        }
        
        public Node<TKey, TValue> GetNextNode(Node<TKey, TValue> current)
        {
            _expandedNodes.Add(current);
            if (_openList.Count == 0) return null;
            Node<TKey, TValue> node = _openList.Min;
            _openList.Remove(node);
            return node;
        }

        public void AddChildToParent(Node<TKey, TValue> parent, Node<TKey, TValue> child)
        {
            //Si el nodo ya ha sido explorado.
            if (_expandedNodes.Contains(child))
            {
                _expandedNodes.TryGetValue(child, out var original);
                
                //Se actualiza el nodo original con la nueva información y sus hijos respectivamente
                //pudiendo afectar a algun nodo ubicado en la lista abierta.
                if (child.TotalCost < original.TotalCost)
                {
                    original.Update(parent, child.ParentAction);
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
        private void UpdateChildrenCost(Node<TKey, TValue> node)
        {
            if (node.Children.Count == 0) return;
            
            foreach (var child in node.Children)
            {
                var aStarChild = (AStarNode<TKey, TValue>)child;
                if (aStarChild.Children.Count != 0)
                {
                    aStarChild.Update(node);
                    UpdateChildrenCost(aStarChild);
                }
                else
                {
                    if (!_openList.Remove(aStarChild)) continue;
                    
                    aStarChild.Update(node);
                    var cost = node.GetUpdatedCost(_initialGoapState, child.ParentAction);
                    aStarChild.GCost = cost;
                    _openList.Add(aStarChild);
                }
            }
        }
        
        public Func<GoapGoal<TKey, TValue>, GoapState<TKey, TValue>, int> GetCustomHeuristic()
        {
            return _customHeuristic;
        }
    }
}