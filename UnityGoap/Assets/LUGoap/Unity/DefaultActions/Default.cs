using System.Threading;
using System.Threading.Tasks;
using LUGoap.Base;

namespace LUGoap.Unity.Actions
{
    public class DefaultAction : Base.Action
    {
        protected override void Init() { }

        protected override Task<Effects> OnExecute(Effects effects, string[] parameters, CancellationToken token)
        {
            return Task.FromResult(effects);
        }
    }
}