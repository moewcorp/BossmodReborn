namespace BossMod.Shadowbringers.Alliance.A36FalseIdol;

class A36FalseIdolStates : StateMachineBuilder
{
    public A36FalseIdolStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<MadeMagic1>()
            .ActivateOnEnter<MadeMagic2>()
            .ActivateOnEnter<ScreamingScore>()
            .ActivateOnEnter<ScatteredMagic>()
            .ActivateOnEnter<DarkerNote2>();
    }
}
