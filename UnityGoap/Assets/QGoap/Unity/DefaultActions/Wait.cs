using System.Threading;
using System.Threading.Tasks;
using QGoap.Base;

namespace QGoap.Unity.Actions
{
    public class WaitAction : Base.Action
    {
        public int WaitSeconds = 1;

        protected override void Init() { }

        protected override async Task<EffectGroup> OnExecute(EffectGroup effectGroup, string[] parameters, CancellationToken token)
        {
            await Task.Delay(WaitSeconds * 1000, token);
            return effectGroup;
        }
    }
}