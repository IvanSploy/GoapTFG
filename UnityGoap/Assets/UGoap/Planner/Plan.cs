using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using UGoap.Base;
using UGoap.Learning;

namespace UGoap.Planning
{
    public class Plan
    {
        public NodeAction Current { get; private set; }
        public Stack<NodeAction> ExecutedActions { get; } = new();
        public IEntity CurrentEntity { get; private set; }
        public bool IsCompleted { get; set; }
        
        private readonly Stack<NodeAction> _nodes = new();
        private CancellationTokenSource _cancellationTokenSource;
        private Stopwatch stopwatch = new();

        public NodeAction Next => _nodes.Peek();
        public int Count => _nodes.Count;

        public Plan(Node finalNode)
        {
            //Get nodes
            Stack<NodeAction> aux = new();
            while (finalNode.Parent != null)
            {
                aux.Push(finalNode.ActionData);
                finalNode = finalNode.Parent;
                //Debug.Log("Estado: " + nodeGoal.State + "| Goal: " + nodeGoal.Goal);
            }

            //Stores the actions in order.
            while (aux.Count > 0)
            {
                _nodes.Push(aux.Pop());
            }
        }
        
        public Task<Effects> ExecuteNext(IAgent agent)
        {
            Current = _nodes.Pop();
            ExecutedActions.Push(Current);

            stopwatch.Start();
            return ExecuteCurrent(agent);
        }

        public void Finish(State previousState, State state, IAgent agent)
        {
            DebugRecord.Record(state != null ? state.ToString() : "Plan failed.");
            if (!IsCompleted && state != null && Count == 0) IsCompleted = true;
            ApplyLearning(previousState, state, agent);
            stopwatch.Stop();
        }
        
        public void ApplyLearning(State initialState, State state, IAgent agent)
        {
            if (Count == 0)
            {
                if (agent is ILearningAgent { Learning: not null } learningAgent)
                {
                    var reward = state != null ?
                        learningAgent.Learning.SucceedReward : learningAgent.Learning.FailReward;
                    var finalState = state ?? agent.CurrentState;
                    
                    learningAgent.Learning.Update(agent.CurrentGoal.Conditions, initialState,
                        Current.Action.Name, reward, finalState);
                }
            }
            else
            {
                if (agent is ILearningAgent { Learning: not null } learningAgent)
                {
                    var reward = -((int)Math.Round(stopwatch.ElapsedMilliseconds / 1000f) + 1);
                    var finalState = state ?? agent.CurrentState;
                    
                    learningAgent.Learning.Update(agent.CurrentGoal.Conditions, initialState, Current.Action.Name,
                        reward, finalState);
                }
            }
        }

        public bool VerifyCurrent(IAgent agent)
        {
            var currentAction = ExecutedActions.Peek();
            var isConflict = currentAction.Conditions.CheckConflict(agent.CurrentState);
            if (isConflict) return false;

            var state = agent.CurrentState + currentAction.Effects;
            foreach (var nextAction in _nodes)
            {
                isConflict = nextAction.Conditions.CheckConflict(state);
                if (isConflict) return false;
                state += nextAction.Effects;
            }

            return !agent.CurrentGoal.Conditions.CheckConflict(state);
        }
        
        private Task<Effects> ExecuteCurrent(IAgent agent)
        {
            if (!CheckCurrent(agent)) return null;

            _cancellationTokenSource = new CancellationTokenSource();
            return Current.Action.Execute(Current.Effects, agent, Current.Parameters, _cancellationTokenSource.Token);
        }
        
        private bool CheckCurrent(IAgent agent)
        {
            if (!Current.Conditions.CheckConflict(agent.CurrentState))
            {
                var finalState = agent.CurrentState + Current.Effects;
                bool valid = Current.Action.Validate(finalState, agent, Current.Parameters);
                if (!valid)
                {
                    DebugRecord.Record("[GOAP] Plan detenido. La acción no ha podido completarse.");
                }
                return valid;
            }
            
            DebugRecord.Record("[GOAP] Plan detenido. El agente no cumple con las precondiciones necesarias.");
            //Debug.Log("Accion:" + Name + " | Estado actual: " + stateInfo.WorldState + " | Precondiciones accion: " + _preconditions);
            return false;
        }
        
        public void Interrupt()
        {
            _cancellationTokenSource.Cancel();
        }
    }
}