namespace BossMod.Shadowbringers.Alliance.A35FalseIdol;

sealed class A35FalseIdolStates : StateMachineBuilder
{
    public A35FalseIdolStates(A35FalseIdol module) : base(module)
    {
        TrivialPhase(default)
            .ActivateOnEnter<MagicalInterference>()
            .ActivateOnEnter<MadeMagic>()
            .ActivateOnEnter<ScreamingScore>()
            .ActivateOnEnter<ScatteredMagic>()
            .ActivateOnEnter<DarkerNote>();
        TrivialPhase(1u)
            .ActivateOnEnter<MagicalInterference>()
            .ActivateOnEnter<UnevenFooting>()
            .ActivateOnEnter<Crash>()
            .ActivateOnEnter<ScreamingScore>()
            .ActivateOnEnter<DarkerNote>()
            .ActivateOnEnter<HeavyArms1>()
            .ActivateOnEnter<HeavyArms2>()
            .ActivateOnEnter<PlaceOfPower>()
            .ActivateOnEnter<ShockwaveKB>()
            .ActivateOnEnter<ShockwaveAOE>()
            .ActivateOnEnter<Towerfall>()
            .Raw.Update = () => module.BossBossP2?.IsDeadOrDestroyed ?? true;
    }
}
