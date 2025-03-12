namespace BossMod.Endwalker.VariantCriterion.V01SS.V011Geryon;

class V011GeryonStates : StateMachineBuilder
{
    public V011GeryonStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SubterraneanShudder>()
            .ActivateOnEnter<ColossalStrike>()
            .ActivateOnEnter<ColossalCharge1>()
            .ActivateOnEnter<ColossalCharge2>()
            .ActivateOnEnter<ColossalLaunch>()
            .ActivateOnEnter<ColossalSlam>()
            .ActivateOnEnter<ColossalSwing>()
            .ActivateOnEnter<ExplosionAOE>()
            .ActivateOnEnter<ExplosionDonut>()
            .ActivateOnEnter<RunawaySludge>()
            .ActivateOnEnter<Shockwave>()
            .ActivateOnEnter<ColossalStrike>();
    }
}
