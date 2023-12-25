using System;
using System.Collections.Generic;
using UGoap.Base;

namespace UGoap.Planner
{
    public class AStar<TKey, TValue> : INodeGenerator<TKey, TValue>
    {
        private readonly SortedSet<Node<TKey, TValue>> _openList; //Para acceder más rapidamente al elemento prioritario.
        private readonly HashSet<Node<TKey, TValue>> _expandedNodes;
        private readonly Func<GoapGoal<TKey, TValue>, PropertyGroup<TKey, TValue>, int> _customHeuristic;

        public AStar(Func<GoapGoal<TKey, TValue>, PropertyGroup<TKey, TValue>, int> customHeuristic = null)
        {
            _openList = new SortedSet<Node<TKey, TValue>>();
            _expandedNodes = new HashSet<Node<TKey, TValue>>();
            _customHeuristic = customHeuristic;
        }

        public Node<TKey, TValue> CreateInitialNode(PropertyGroup<TKey, TValue> currentState, GoapGoal<TKey, TValue> goal)
        {
            var initialGoalState = goal.GetConflicts(currentState);
            var initialGoal = new GoapGoal<TKey, TValue>(goal.Name, initialGoalState, goal.PriorityLevel);
            AStarNode<TKey, TValue> node = new AStarNode<TKey, TValue>(currentState, initialGoal, this);
            var initialHeuristic = node.GetHeuristic();
            node.HCost = initialHeuristic;
            node.TotalCost = initialHeuristic;
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
                    //Sin embargo si el nodo ya ha sido expandido y los objetivos no coindiden es mejor dejar dos ramas
                    //abiertas debido a que habría que eliminar toda la descendencia pues no se puede actualizar a los hijos.
                    if (!child.Goal.Equals(original.Goal))
                    {
                        _openList.Add(child);
                        return;
                    }
                    
                    original.Update(parent, child.Action, child.ProceduralEffects);
                    UpdateChildren(original);
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
                    original.Update(parent, child.Goal, child.Action, child.ProceduralEffects);
                    _openList.Add(original);
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
        private void UpdateChildren(Node<TKey, TValue> node)
        {
            if (node.Children.Count == 0) return;
            
            foreach (var child in node.Children)
            {
                if (child.Children.Count != 0)
                {
                    child.Update(node);
                    UpdateChildren(child);
                }
                else
                {
                    if (!_openList.Remove(child)) continue;
                    
                    child.Update(node);
                    _openList.Add(child);
                }
            }
        }
        
        public Func<GoapGoal<TKey, TValue>, PropertyGroup<TKey, TValue>, int> GetCustomHeuristic()
        {
            return _customHeuristic;
        }
    }
}