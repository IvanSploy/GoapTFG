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
        public (GoapState, bool) DoPlan(GoapState currentState)
        {
            bool accomplished = true;
            if (Count == 0) return (null, false);

            foreach (var node in _nodes)
            {
                (currentState, accomplished) = node.PreviousAction.Execute(currentState, node.Goal, _agent);
                if (!accomplished) break;
            }

            ExecutedNodes.AddRange(_nodes);
            _nodes.Clear();
            return (currentState, accomplished);
        }

        public (GoapState, bool) PlanStep(GoapState currentState)
        {
            bool accomplished;
            if (Count == 0) return (null, false);
            if(CurrentNode != null) ExecutedNodes.Add(CurrentNode);
            
            CurrentNode = _nodes.Pop();
            (currentState, accomplished) = CurrentNode.PreviousAction.Execute(currentState, CurrentNode.Goal, _agent);
            if(currentState != null) DebugRecord.AddRecord(currentState.ToString());
            return (currentState, accomplished);
        }
    }
}