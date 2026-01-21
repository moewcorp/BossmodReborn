namespace BossMod.Dawntrail.Savage.M11STheTyrant;

sealed class M11STheTyrantStates : StateMachineBuilder
{
    public M11STheTyrantStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<CrownOfArcadia>()
            .ActivateOnEnter<RawSteelTrophyAxe>()
            .ActivateOnEnter<RawSteelTrophyScythe>()
            .ActivateOnEnter<AssaultEvolvedSword>()
            .ActivateOnEnter<AssaultEvolvedAxeStack>()
            .ActivateOnEnter<AssaultEvolved_AxeAOE>()
            .ActivateOnEnter<AssaultEvolvedScythe>()
            //.ActivateOnEnter<VoidStardust>()
            .ActivateOnEnter<Cometite>()
            .ActivateOnEnter<CrushingComet>()
            .ActivateOnEnter<EyeOfTheHurricane>()
            .ActivateOnEnter<Explosion>()
            .ActivateOnEnter<AssaultEvolvedSword>()
            .ActivateOnEnter<AssaultEvolvedAxeStack>()
            .ActivateOnEnter<AssaultEvolved_AxeAOE>()
            .ActivateOnEnter<AssaultEvolvedScythe>()
            .ActivateOnEnter<MaelstromVoidZones>()
            .ActivateOnEnter<MaelstromGustCones>()
            .ActivateOnEnter<GreatWallOfFire>()
            .ActivateOnEnter<OrbitalOmen>()
            .ActivateOnEnter<FireAndFury>()
            .ActivateOnEnter<MeteorainComets>()
            .ActivateOnEnter<FearsomeFireball>()
            .ActivateOnEnter<CosmicKiss>()
            .ActivateOnEnter<CometTethers>()
            .ActivateOnEnter<TripleTyrannhilation>()
            .ActivateOnEnter<Flatliner>();
    }
}