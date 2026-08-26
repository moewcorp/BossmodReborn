namespace BossMod.Dawntrail.Foray.ForkedTowerMagic.Extreme.FTME2SwordDancer;

[SkipLocalsInit]
sealed class FTME2SwordDancerStates : StateMachineBuilder
{
    public FTME2SwordDancerStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}
