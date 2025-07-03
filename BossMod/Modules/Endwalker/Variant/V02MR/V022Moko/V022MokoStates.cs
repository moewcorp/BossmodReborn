namespace BossMod.Endwalker.VariantCriterion.V02MR.V022Moko;

abstract class V022MokoStates : StateMachineBuilder
{
    public V022MokoStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChange>()
            // Route 1
            .ActivateOnEnter<Unsheathing>()
            .ActivateOnEnter<VeilSever>()
            // Route 2
            .ActivateOnEnter<ScarletAuspice>()
            .ActivateOnEnter<Clearout>()
            .ActivateOnEnter<Explosion>()
            // Route 3
            .ActivateOnEnter<GhastlyGrasp>()
            .ActivateOnEnter<YamaKagura>()
            // Route 4
            .ActivateOnEnter<Spiritflame>()
            .ActivateOnEnter<Spiritflames>()
            // Standard
            .ActivateOnEnter<SpearmanOrdersFast>()
            .ActivateOnEnter<SpearmanOrdersSlow>()
            .ActivateOnEnter<KenkiReleaseMoonlessNight>()
            .ActivateOnEnter<IronRain>()
            .ActivateOnEnter<Giri>()
            .ActivateOnEnter<AzureAuspice>()
            .ActivateOnEnter<BoundlessScarletAzure>()
            .ActivateOnEnter<UpwellFirst>()
            .ActivateOnEnter<UpwellRest>();
    }
}

sealed class V022MokoPath2States(BossModule module) : V022MokoStates(module) { }
sealed class V022MokoOtherPathsStates(BossModule module) : V022MokoStates(module) { }
