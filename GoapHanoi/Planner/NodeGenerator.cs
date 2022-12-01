using System.Collections.Generic;
using GoapHanoi.Base;

namespace GoapHanoi.Planner
{
    public class NodeGenerator<TA, TB>
    {
        private Node<TA, TB> _current;
        public List<Node<TA, TB>> SortedNodes;

        public NodeGenerator(Node<TA, TB> current)
        {
            _current = current;
        }
    }
}