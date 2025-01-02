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
        //TODO: Need to store the Goals for learning purposes.
        public NodeAction Current { get; private set; }
        public Stack<NodeAction> ExecutedActions { get; } = new();
        public IEntity CurrentEntity { get; private set; }
        public bool IsCompleted { get; set; }
        
        private readonly Stack<NodeAction> _nodes = new();
        private CancellationTokenSource _cancellationTokenSource;
        private readonly Stopwatch _stopwatch = new();

        public NodeAction? Previous => ExecutedActions.Count > 0 ? ExecutedActions.Peek() : null;
        public NodeAction? Next => _nodes.Count > 0 ? _nodes.Peek() : null;
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
            if(Current.Action != null) ExecutedActions.Push(Current);
            Current = _nodes.Pop();

            _stopwatch.Start();
            return ExecuteCurrent(agent);
        }

        public void Finish(State state, IAgent agent)
        {
            DebugRecord.Record(state != null ? state.ToString() : "Plan failed.");
            if (!IsCompleted && state != null && Count == 0) IsCompleted = true;
            ApplyLearning(state, agent);
            _stopwatch.Stop();
        }
        
        public void ApplyLearning(State state, IAgent agent)
        {
            if (Count == 0)
            {
                if (agent is ILearningAgent { Learning: not null } learningAgent)
                {
                    var reward = state != null ?
                        learningAgent.Learning.SucceedReward : learningAgent.Learning.FailReward;

                    var nextLearningCode = Current.LearningCode;
                    if (Previous.HasValue) nextLearningCode = Previous.Value.LearningCode;
                    
                    learningAgent.Learning.Update(Current.LearningCode,
                        Current.Action.Name, reward, nextLearningCode);
                }
            }
            else
            {
                if (agent is ILearningAgent { Learning: not null } learningAgent)
                {
                    var reward = -((int)Math.Round(_stopwatch.ElapsedMilliseconds / 1000f) + 1);
                    
                    var nextLearningCode = Current.LearningCode;
                    if (Previous.HasValue) nextLearningCode = Previous.Value.LearningCode;
                    
                    learningAgent.Learning.Update(Current.LearningCode, Current.Action.Name,
                        reward, nextLearningCode);
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