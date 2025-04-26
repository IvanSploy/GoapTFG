using System.Threading;
using System.Threading.Tasks;
using QGoap.Base;

namespace QGoap.Unity.Actions
{
    public class DefaultAction : Base.Action
    {
        protected override void Init() { }

        protected override Task<EffectGroup> OnExecute(EffectGroup effectGroup, string[] parameters, CancellationToken token)
        {
            return Task.FromResult(effectGroup);
        }
    }
}