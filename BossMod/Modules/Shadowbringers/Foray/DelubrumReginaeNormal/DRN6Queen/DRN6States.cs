namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRN6Queen;

class DRN6QueenStates : StateMachineBuilder
{
    public DRN6QueenStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChange>()
            .ActivateOnEnter<NorthswainsGlowPawnOff>()
            .ActivateOnEnter<GodsSaveTheQueen>()
            .ActivateOnEnter<OptimalPlaySword>()
            .ActivateOnEnter<OptimalPlayShield>()
            .ActivateOnEnter<JudgmentBlade>()
            .ActivateOnEnter<HeavensWrathKnockback>()
            //.ActivateOnEnter<Chess>()
            //.ActivateOnEnter<QueensWill>()
            //.ActivateOnEnter<QueensEdict>()
            .ActivateOnEnter<TurretsTour>()
            .ActivateOnEnter<AboveBoard>()
            ;
    }
}
