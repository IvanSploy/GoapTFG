﻿using System.Threading;
using System.Threading.Tasks;
using LUGoap.Base;
using LUGoap.Unity;
using Panda.Examples.Shooter;

public class SetDestinationCoverAction : Action
{
    private AI _ai;
    
    protected override void Init()
    {
        if (_agent is not GoapAgent agent) return;
        _ai = agent.GetComponent<AI>();
    }

    protected override async Task<Effects> OnExecute(Effects effects, string[] parameters, CancellationToken token)
    {
        _ai.SetDestination_Cover();
        return effects;
    }
}