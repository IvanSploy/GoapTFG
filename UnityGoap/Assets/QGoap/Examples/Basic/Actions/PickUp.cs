using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using LUGoap.Base;
using LUGoap.Unity;

public class PickUpAction : Action
{
    public string Target;

    protected override void Init() { }
    
    protected override Task<EffectGroup> OnExecute(EffectGroup effectGroup, string[] parameters, CancellationToken token)
    {
        GoapEntity entityKey = WorkingMemoryManager.Get(Target).Object;
        Object.Destroy(entityKey.gameObject);
        return Task.FromResult(effectGroup);
    }
}