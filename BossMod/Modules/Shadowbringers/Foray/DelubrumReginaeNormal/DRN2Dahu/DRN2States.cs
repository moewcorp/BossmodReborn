namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRN2Dahu;

class DRN2DahuStates : StateMachineBuilder
{
    public DRN2DahuStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<FallingRock>()
            .ActivateOnEnter<HotCharge>()
            .ActivateOnEnter<Firebreathe>()
            .ActivateOnEnter<HeadDown>()
            .ActivateOnEnter<FirebreatheRotating>()
            .ActivateOnEnter<Shockwave>()
            .ActivateOnEnter<HuntersClaw>()
            .ActivateOnEnter<FeralHowl>();
    }
}
