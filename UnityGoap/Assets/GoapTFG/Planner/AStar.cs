using System.Collections.Generic;
using GoapTFG.Base;

namespace GoapTFG.Planner
{
    public class AStar<TA, TB> : INodeGenerator<TA, TB>
    {
        private readonly SortedSet<Node<TA, TB>> _openList; //Para acceder más rapidamente al elemento prioritario.
        private readonly HashSet<Node<TA, TB>> _expandedNodes;
        private readonly System.Func<GoapGoal<TA, TB>, PropertyGroup<TA, TB>, int> _customHeuristic;

        public AStar(System.Func<GoapGoal<TA, TB>, PropertyGroup<TA, TB>, int> customHeuristic = null)
        {
            _openList = new SortedSet<Node<TA, TB>>();
            _expandedNodes = new HashSet<Node<TA, TB>>();
            _customHeuristic = customHeuristic;
        }

        public Node<TA, TB> CreateInitialNode(PropertyGroup<TA, TB> currentState, GoapGoal<TA, TB> goapGoal)
        {
            AStarNode<TA, TB> node = new AStarNode<TA, TB>(currentState, goapGoal, this);
            var initialHeuristic = node.GetHeuristic();
            node.HCost = initialHeuristic;
            node.TotalCost = initialHeuristic;
            return node;
        }
        
        public Node<TA, TB> GetNextNode(Node<TA, TB> current)
        {
            _expandedNodes.Add(current);
            if (_openList.Count == 0) return null;
            Node<TA, TB> node = _openList.Min;
            _openList.Remove(node);
            return node;
        }

        public void AddChildToParent(Node<TA, TB> parent, Node<TA, TB> child, IGoapAction<TA, TB> goapAction)
        {
            //Si el nodo ya ha sido explorado.
            if (_expandedNodes.Contains(child))
            {
                _expandedNodes.TryGetValue(child, out var original);
                //Se actualiza el nodo original con la nueva información y sus hijos respectivamente
                //pudiendo afectar a algun nodo ubicado en la lista abierta.
                if (child.TotalCost < original.TotalCost)
                {
                    original.Update(parent, goapAction);
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
                    original.Update(parent, goapAction);
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
        private void UpdateChildren(Node<TA, TB> node)
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
        
        public System.Func<GoapGoal<TA, TB>, PropertyGroup<TA, TB>, int> GetCustomHeuristic()
        {
            return _customHeuristic;
        }
    }
}