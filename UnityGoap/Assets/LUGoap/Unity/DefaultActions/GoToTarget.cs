﻿using UnityEngine;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LUGoap.Base;
using static LUGoap.Base.PropertyManager;

namespace LUGoap.Unity.Action
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

        protected override Conditions GetProceduralConditions(ActionSettings settings)
        {
            var condition = new Conditions();
            condition.Set(TargetKey, BaseTypes.ConditionType.NotEqual, ExcludedLocation);
            return condition;
        }
        
        protected override Effects GetProceduralEffects(ActionSettings settings)
        {
            Effects proceduralEffects = new Effects();
            string target = ExcludedLocation;
            var targetList = settings.Goal.TryGetOrDefault(TargetKey, "");
            if (targetList != null)
            {
                var condition = targetList.FirstOrDefault(condition => condition.ConditionType == BaseTypes.ConditionType.Equal);
                if(condition != null) target = (string)condition.Value;
            }
            proceduralEffects[TargetKey] = new EffectValue(target, BaseTypes.EffectType.Set);
            return proceduralEffects;
        }
        
        public override int GetCost(Conditions goal)
        {
            if (!goal.Has(TargetKey)) return 50 / SpeedFactor;
            
            var target = (string) goal[TargetKey].First(condition => condition.ConditionType == BaseTypes.ConditionType.Equal).Value;

            var pos = WorkingMemoryManager.Get(target).Position;
            
            var cost = Math.Max(3, (int)(Vector3.Distance(Vector3.zero, pos) / SpeedFactor));
            return cost;
        }

        protected override async Task<Effects> OnExecute(Effects effects, string[] parameters, CancellationToken token)
        {
            var targetName = (string)effects.TryGetOrDefault(TargetKey, "None").Value;
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

            return effects;
        }
    }
}