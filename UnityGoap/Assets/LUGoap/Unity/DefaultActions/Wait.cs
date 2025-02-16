using System.Threading;
using System.Threading.Tasks;
using LUGoap.Base;

namespace LUGoap.Unity.Actions
{
    public class WaitAction : Base.Action
    {
        public int WaitSeconds = 1;

        protected override void Init() { }

        protected override async Task<Effects> OnExecute(Effects effects, string[] parameters, CancellationToken token)
        {
            await Task.Delay(WaitSeconds * 1000, token);
            return effects;
        }
    }
}