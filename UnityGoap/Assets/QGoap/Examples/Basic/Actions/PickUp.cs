using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using QGoap.Base;
using QGoap.Unity;

public class PickUpAction : Action
{
    public string Target;

    protected override void Init() { }
    
    protected override Task<EffectGroup> OnExecute(EffectGroup effectGroup, string[] parameters, CancellationToken token)
    {
        GoapEntity entityKey = WorkingMemoryManager.Get(Target).Object;
        if(entityKey) Object.Destroy(entityKey.gameObject);
        return Task.FromResult(effectGroup);
    }
}