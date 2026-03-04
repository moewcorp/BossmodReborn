namespace BossMod.Modules.Dawntrail.Advanced.Ad01TheMerchantsTale.Ad011PariofPlenty;
sealed class PariOfPlentyStates : StateMachineBuilder
{
    public PariOfPlentyStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<HeatBurst>()
            .ActivateOnEnter<RightFireFlight>();
    }
}
