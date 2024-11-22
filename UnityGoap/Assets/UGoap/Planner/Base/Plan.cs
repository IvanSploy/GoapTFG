using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UGoap.Base;

namespace UGoap.Planner
{
    public class Plan
    {
        public GoapState InitialState { get; private set; }
        public Node CurrentNode { get; private set; }
        public Stack<Node> ExecutedNodes { get; } = new();
        public IGoapEntity CurrentEntity { get; private set; }
        public bool IsDone { get; set; }
        
        private readonly Stack<Node> _nodes = new();
        private readonly IGoapAgent _agent;

        private CancellationTokenSource _cancellationTokenSource;

        //Constructor
        public Plan(GoapState initialState, IGoapAgent agent, Node finalNode)
        {
            InitialState = initialState;
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
        public Task<GoapState> ExecuteNext(GoapState currentState)
        {
            if (Count == 0)
            {
                IsDone = true;
                return null;
            }
            
            CurrentNode = _nodes.Pop();
            ExecutedNodes.Push(CurrentNode);

            _cancellationTokenSource = new CancellationTokenSource();
            
            var result = CurrentNode.ExecuteAction(currentState, _agent, _cancellationTokenSource.Token);
            if (result == null) return null;
            
            result.ContinueWith(goapState =>
            {
                if (goapState.Result != null) DebugRecord.AddRecord(goapState.ToString());
            });
            
            return result;
        }

        public void Interrupt()
        {
            _cancellationTokenSource.Cancel();
        }
    }
}