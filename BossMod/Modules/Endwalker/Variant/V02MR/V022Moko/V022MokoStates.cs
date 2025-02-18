namespace BossMod.Endwalker.VariantCriterion.V02MR.V022Moko;

class V022MokoOtherPathsStates : StateMachineBuilder
{
    public V022MokoOtherPathsStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChange>()
            //Route 1
            .ActivateOnEnter<Unsheathing>()
            .ActivateOnEnter<VeilSever>()
            //Route 2
            .ActivateOnEnter<ScarletAuspice>()
            .ActivateOnEnter<MoonlessNight>()
            .ActivateOnEnter<Clearout>()
            .ActivateOnEnter<BoundlessScarlet>()
            .ActivateOnEnter<Explosion>()
            // Route 3
            .ActivateOnEnter<YamaKagura>()
            .ActivateOnEnter<GhastlyGrasp>()
            // Route 4
            .ActivateOnEnter<Spiritflame>()
            .ActivateOnEnter<Spiritflames>()
            //Standard
            .ActivateOnEnter<SpearmanOrdersFast>()
            .ActivateOnEnter<SpearmanOrdersSlow>()
            .ActivateOnEnter<KenkiRelease>()
            .ActivateOnEnter<IronRain>()
            .ActivateOnEnter<Giri>()
            .ActivateOnEnter<AzureAuspice>()
            .ActivateOnEnter<BoundlessAzure>()
            .ActivateOnEnter<UpwellFirst>()
            .ActivateOnEnter<UpwellRest>();
    }
}

class V022MokoPath2States(BossModule module) : V022MokoOtherPathsStates(module) { }
