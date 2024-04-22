using System.Collections.Generic;
using UGoap.Base;

namespace UGoap.Planner
{
    public class Plan
    {
        public Node CurrentNode { get; private set; }
        public IGoapEntity CurrentEntity { get; private set; }
        public bool IsDone { get; private set; }
        
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
        
        //Methods
        public GoapState DoPlan(GoapState currentState)
        {
            if (Count == 0) return null;

            foreach (var node in _nodes)
            {
                currentState = node.PreviousAction.Execute(currentState, node.Goal, _agent);
                if (currentState == null) return null;
            }

            ExecutedNodes.AddRange(_nodes);
            _nodes.Clear();
            IsDone = true;
            return currentState;
        }

        public GoapState PlanStep(GoapState currentState)
        {
            if (Count == 0)
            {
                IsDone = true;
                return null;
            }
            if(CurrentNode != null) ExecutedNodes.Add(CurrentNode);
            
            CurrentNode = _nodes.Pop();
            currentState = CurrentNode.PreviousAction.Execute(currentState, CurrentNode.Parent.Goal, _agent);
            if(currentState != null) DebugRecord.AddRecord(currentState.ToString());
            return currentState;
        }
    }
}