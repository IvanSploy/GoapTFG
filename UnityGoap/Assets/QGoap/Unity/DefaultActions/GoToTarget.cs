using UnityEngine;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using QGoap.Base;
using static QGoap.Base.PropertyManager;

namespace QGoap.Unity.Action
{
    public class GoToTargetAction : Base.Action
    {
        public PropertyKey TargetKey;
        public int SpeedFactor;
        public string ExcludedLocation;

        private GoapAgent _goapAgent;
        private Transform _transform;

        protected override void Init()
        {
            if (_agent is not GoapAgent agent) return;
            _goapAgent = agent;
            _transform = agent.transform;
        }

        protected override ConditionGroup GetProceduralConditions(ActionSettings settings)
        {
            var condition = new ConditionGroup();
            condition.SetOrCombine(TargetKey, BaseTypes.ConditionType.NotEqual, ExcludedLocation);
            return condition;
        }
        
        protected override EffectGroup GetProceduralEffects(ActionSettings settings)
        {
            EffectGroup proceduralEffectGroup = new EffectGroup();
            string target = ExcludedLocation;
            
            var closest = settings.Goal.Get(TargetKey)?.RequiredValue;
            if (closest != null) target = (string)closest;
            
            proceduralEffectGroup[TargetKey] = new Effect(target, BaseTypes.EffectType.Set);
            return proceduralEffectGroup;
        }
        
        public override int GetCost(ConditionGroup goal)
        {
            if (!goal.Has(TargetKey)) return 50 / SpeedFactor;

            string target = goal[TargetKey].RequiredValue as string ?? "";

            var pos = WorkingMemoryManager.Get(target).Position;
            
            var cost = Math.Max(3, (int)(Vector3.Distance(Vector3.zero, pos) / SpeedFactor));
            return cost;
        }

        protected override async Task<EffectGroup> OnExecute(EffectGroup effectGroup, string[] parameters, CancellationToken token)
        {
            var targetName = (string)effectGroup.TryGetOrDefault(TargetKey, "None").Value;
            GoapEntity targetEntity = WorkingMemoryManager.Get(targetName).Object;
            var target = targetEntity.transform.position;

            target.y = _transform.position.y;
            var rotationTarget = Quaternion.LookRotation(target - _transform.position, Vector3.up);
            
            bool reached = false;

            var speed = _goapAgent.Speed * SpeedFactor;
            
            //Rotate
            while (!reached)
            {
                if (token.IsCancellationRequested) return null;
                
                _transform.rotation = Quaternion.RotateTowards(_transform.rotation, rotationTarget, speed * 45 * Time.deltaTime );
                
                if (Vector3.Distance(_transform.eulerAngles, rotationTarget.eulerAngles) < float.Epsilon)
                {
                    reached = true;
                }
                await Task.Yield();
            }

            reached = false;
            
            //Move
            while (!reached)
            {
                if (token.IsCancellationRequested) return null;
                    
                var p = _transform.position;
                target.y = p.y;
                _transform.position = Vector3.MoveTowards(p, target, speed * Time.deltaTime);
                target.y = p.y;
                if (Vector3.Distance(_transform.position, target) < float.Epsilon)
                {
                    reached = true;
                }
                await Task.Yield();
            }

            return effectGroup;
        }
    }
}