namespace BossMod.Shadowbringers.Alliance.A33RedGirl;

sealed class A33RedGirlStates : StateMachineBuilder
{
    public A33RedGirlStates(A33RedGirl module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<CrueltyP1>()
            .ActivateOnEnter<SublimeTranscendence>()
            .ActivateOnEnter<GenerateBarrier1>()
            .ActivateOnEnter<GenerateBarrier2>()
            .ActivateOnEnter<ShockWhiteBlack>()
            .Raw.Update = () => module.PrimaryActor.IsDestroyed || module.RedSphere != null;
        TrivialPhase(1u)
            .ActivateOnEnter<WaveWhite>()
            .ActivateOnEnter<WaveBlack>()
            .ActivateOnEnter<BigExplosion>()
            .Raw.Update = () => (module.BossP2?.IsTargetable ?? false) || module.WorldState.CurrentCFCID != 779u;
        TrivialPhase(2u)
            .ActivateOnEnter<CrueltyP2>()
            .ActivateOnEnter<Explosion>()
            .ActivateOnEnter<GenerateBarrier2>()
            .ActivateOnEnter<GenerateBarrier3>()
            .ActivateOnEnter<GenerateBarrier4>()
            .Raw.Update = () => (module.BossP2?.IsDestroyed ?? true) || module.BossP2?.HPMP.CurHP <= 1u;
    }
}
