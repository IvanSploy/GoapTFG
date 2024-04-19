using System.Collections.Generic;
using UGoap.Base;

namespace UGoap.Planner
{
    public class Plan
    {
        public Node CurrentNode { get; private set; }
        public IGoapEntity CurrentEntity { get; private set; }
        
        private readonly Stack<Node> _nodes = new();
        public List<Node> ExecutedNodes { get; private set; } = new();

        private readonly IGoapAgent _agent;

        //Constructor
        public Plan(IGoapAgent agent, Node finalNode)
        {
            _agent = agent;
            
            //Get nodes
            Stack<Node> aux = new();
            while (finalNode.Parent != null)
            {
                //Debug.Log("Estado: " + nodeGoal.State + "| Goal: " + nodeGoal.Goal);
                aux.Push(finalNode);
                finalNode = finalNode.Parent;
            }

            //Stores the nodes in order.
            while (aux.Count > 0)
            {
                _nodes.Push(aux.Pop());
            }
        }
        
        //Accesors
        public int Count => _nodes.Count;
        public bool IsDone => _nodes.Count == 0;
        
        //Methods
        public GoapState DoPlan(GoapState worldGoapState)
        {
            if (Count == 0) return null;

            foreach (var node in _nodes)
            {
                var stateInfo = new GoapStateInfo(worldGoapState, node.Parent.Goal, node.Parent.State);
                worldGoapState = node.ParentAction.Execute(stateInfo, _agent);
            }

            ExecutedNodes.AddRange(_nodes);
            _nodes.Clear();
            return worldGoapState;
        }

        public GoapState PlanStep(GoapState worldGoapState)
        {
            if (Count == 0) return null;
            if(CurrentNode != null) ExecutedNodes.Add(CurrentNode);
            
            CurrentNode = _nodes.Pop();
            var stateInfo = new GoapStateInfo(worldGoapState, CurrentNode.Parent.Goal, CurrentNode.Parent.State);
            worldGoapState = CurrentNode.ParentAction.Execute(stateInfo, _agent);
            if(worldGoapState != null) DebugRecord.AddRecord(worldGoapState.ToString());
            return worldGoapState;
        }
    }
}