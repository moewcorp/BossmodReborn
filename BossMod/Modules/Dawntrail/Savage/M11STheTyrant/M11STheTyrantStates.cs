namespace BossMod.Dawntrail.Savage.M11STheTyrant;

sealed class M11STheTyrantStates : StateMachineBuilder
{
    public M11STheTyrantStates(BossModule module) : base(module)
    {
        DeathPhase(default, SinglePhase)
            .ActivateOnEnter<ArenaChanges>();
    }

    private void SinglePhase(uint id)
    {
        CrownOfArcadia(id, 5.18f); //00:05
        RawSteelTrophy(id + 0x1000, 18f); //00:15-00:33
        TrophyWeapons(id + 0x2000, 60f);  //00:33-01:17
        VoidStardust(id + 0x3000, 60f); //01:17-01:48
        CrownOfArcadia(id + 0x4000, 5f); // 01:58
        DanceOfDomination(id + 0x5000, 60f); //02:14-02:35
        RawSteelTrophy(id + 0x6000, 60f); //02:35-02:55
        Charybdistopia(id + 0x7000, 60f); //02:55-03:03
        UltimateTrophyWeapons(id + 0x8000, 60f); //03:03-03:53
        OneAndOnly(id + 0x9000, 60f); //03:53
        Flatliner(id + 0x10000, 460f); //05:58
        EclipticMeteor(id + 0x20000, 300f);
        SimpleState(id + 0xFF0000u, 10000f, "Report Me");
    }
    private void CrownOfArcadia(uint id, float delay)
    {
        Cast(id, (uint)AID.CrownOfArcadia, delay, 5f, "Crown of Arcadia")
            .SetHint(StateMachine.StateHint.Raidwide)
            .ActivateOnEnter<CrownOfArcadia>()
            .DeactivateOnExit<CrownOfArcadia>();
    }
    private void RawSteelTrophy(uint id, float delay) //This will always cast either Axe or Scythe, and then cast the one it didn't
    {
        CastMulti(id, [(uint)AID.RawSteelTrophyAxe, (uint)AID.RawSteelTrophyScythe], delay, 1.7f, "Select Weapon")
            .ActivateOnEnter<RawSteelTrophyAxe>()
            .ActivateOnEnter<RawSteelTrophyScythe>();
        ComponentCondition<RawSteelTrophyAxe>(id + 0x10, 1f, comp => comp.NumCasts != 0, "Raw Steel Trophy Axe resolves")
            .DeactivateOnExit<RawSteelTrophyAxe>()
            .ActivateOnEnter<RawSteelTrophyScythe>();
        ComponentCondition<RawSteelTrophyScythe>(id + 0x10, 1f, comp => comp.NumCasts != 0, "Raw Steel Trophy Scythe resolves")
            .DeactivateOnExit<RawSteelTrophyAxe>()
            .ActivateOnEnter<RawSteelTrophyScythe>();
    }
    private void TrophyWeapons(uint id, float delay)
    {
        Cast(id, (uint)AID.TrophyWeapons, delay, 2f, "Trophy Weapons")
            .ActivateOnEnter<AssaultWeaponTimeline>()
            .ActivateOnEnter<AssaultFloorPredictor>()
            .ActivateOnEnter<AssaultEvolvedSword>()
            .ActivateOnEnter<AssaultEvolvedScythe>()
            .ActivateOnEnter<AssaultEvolvedAxeStack>();
        ComponentCondition<AssaultWeaponTimeline>(id + 0x10, 20f, comp => !comp.Executing, "Trophy Weapons Complete")
            .DeactivateOnExit<AssaultWeaponTimeline>()
            .DeactivateOnExit<AssaultFloorPredictor>()
            .DeactivateOnExit<AssaultEvolvedSword>()
            .DeactivateOnExit<AssaultEvolvedScythe>()
            .DeactivateOnExit<AssaultEvolvedAxeStack>();
    }
    private void VoidStardust(uint id, float delay)
    {
        Cast(id, (uint)AID.TrophyWeapons, delay, 2.7f, "Stack/Spread, 3 Weapons, Spread/Stack")
            .ActivateOnEnter<AssaultWeaponTimeline>()
            .ActivateOnEnter<AssaultFloorPredictor>()
            .ActivateOnEnter<AssaultEvolvedSword>()
            .ActivateOnEnter<AssaultEvolvedScythe>()
            .ActivateOnEnter<AssaultEvolvedAxeStack>();
        Cast(id + 0x10, (uint)AID.VoidStardust, delay, 4f, "Bait Puddles")
            .ActivateOnEnter<Cometite>()
            .ActivateOnEnter<Comet>()
            .ActivateOnEnter<CrushingComet>();

        ComponentCondition<AssaultWeaponTimeline>(id + 0x20, 20f, comp => !comp.Executing, "Trophy Weapons Complete")
            .DeactivateOnExit<AssaultWeaponTimeline>()
            .DeactivateOnExit<AssaultFloorPredictor>()
            .DeactivateOnExit<AssaultEvolvedSword>()
            .DeactivateOnExit<AssaultEvolvedScythe>()
            .DeactivateOnExit<AssaultEvolvedAxeStack>();
        ComponentCondition<Comet>(id + 0x30, 20f, comp => comp.NumFinishedSpreads != 0)
            .DeactivateOnExit<Comet>();
        ComponentCondition<CrushingComet>(id + 0x40, 20f, comp => comp.NumFinishedStacks != 0)
            .DeactivateOnExit<CrushingComet>();
        ComponentCondition<Cometite>(id + 0x60, 20f, comp => comp.NumCasts >= 40) // it's 48 casts if everyone is alive...
            .DeactivateOnExit<Cometite>();
    }
    private void DanceOfDomination(uint id, float delay)
    {
        Cast(id, (uint)AID.DanceOfDominationTrophy, delay, 2f, "Multi-hit Raidwide")
            .ActivateOnEnter<DanceOfDomination>()
            .ActivateOnEnter<Explosion>()
            .ActivateOnEnter<EyeOfTheHurricane>();
        ComponentCondition<EyeOfTheHurricane>(id + 0x10, 20f, comp => comp.NumFinishedStacks != 0)
            .DeactivateOnExit<DanceOfDomination>()
            .DeactivateOnExit<Explosion>()
            .DeactivateOnExit<EyeOfTheHurricane>();
    }
    private void Charybdistopia(uint id, float delay)
    {
        Cast(id, (uint)AID.Charybdistopia, delay, 2f, "Sets HP to 0")
            .ActivateOnEnter<Charybdistopia>()
            .DeactivateOnExit<Charybdistopia>();
    }
    private void UltimateTrophyWeapons(uint id, float delay)
    {
        Cast(id, (uint)AID.UltimateTrophyWeapons, delay, 2f, "Ultimate Trophy Weapons")
            .ActivateOnEnter<AssaultWeaponTimeline>()
            .ActivateOnEnter<AssaultFloorPredictor>()
            .ActivateOnEnter<AssaultEvolvedSword>()
            .ActivateOnEnter<AssaultEvolvedScythe>()
            .ActivateOnEnter<AssaultEvolvedAxeStack>()
            .ActivateOnEnter<MaelstromVoidZones>()
            .ActivateOnEnter<MaelstromGustCones>();

        ComponentCondition<AssaultWeaponTimeline>(id + 0x10, 40f, comp => !comp.Executing, "Weapons Complete, Bait Tornadoes")
            .DeactivateOnExit<AssaultWeaponTimeline>()
            .DeactivateOnExit<AssaultFloorPredictor>()
            .DeactivateOnExit<AssaultEvolvedSword>()
            .DeactivateOnExit<AssaultEvolvedScythe>()
            .DeactivateOnExit<AssaultEvolvedAxeStack>();
        ComponentCondition<MaelstromGustCones>(id + 0x20, 10f, comp => comp.NumCasts != 0)
            .DeactivateOnExit<MaelstromVoidZones>()
            .DeactivateOnExit<MaelstromGustCones>();
    }
    private void OneAndOnly(uint id, float delay)
    {
        Cast(id, (uint)AID.OneAndOnly, delay, 9f)
            .ActivateOnEnter<OneAndOnly>()
            .ActivateOnEnter<GreatWallOfFire>()
            .ActivateOnEnter<GreatWallOfFireExplosion>()
            .ActivateOnEnter<OrbitalOmen>()
            .ActivateOnEnter<FireAndFury>()
            .ActivateOnEnter<MeteorainComets>()
            .ActivateOnEnter<FearsomeFireball>()
            .ActivateOnEnter<CosmicKiss>()
            .ActivateOnEnter<CometTethers>()
            .ActivateOnEnter<Shockwave>()
            .ActivateOnEnter<TripleTyrannhilation>()
            .DeactivateOnExit<OneAndOnly>();
        ComponentCondition<GreatWallOfFireExplosion>(id + 0x10, 10f, comp => comp.NumCasts >= 2)
            .DeactivateOnExit<GreatWallOfFire>()
            .DeactivateOnExit<GreatWallOfFireExplosion>();
        ComponentCondition<OrbitalOmen>(id + 0x20, 12f, comp => comp.NumCasts >= 8)
            .DeactivateOnExit<FireAndFury>()
            .DeactivateOnExit<OrbitalOmen>();
        ComponentCondition<Shockwave>(id + 0x30, 60f, comp => comp.NumCasts >= 3)
            .DeactivateOnExit<MeteorainComets>()
            .DeactivateOnExit<FearsomeFireball>()
            .DeactivateOnExit<CosmicKiss>()
            .DeactivateOnExit<CometTethers>()
            .DeactivateOnExit<Shockwave>()
            .DeactivateOnExit<TripleTyrannhilation>();
    }
    private void Flatliner(uint id, float delay)
    {
        Cast(id, (uint)AID.Flatliner, delay, 6)
            .ActivateOnEnter<Flatliner>()
            .ActivateOnEnter<FlatlinerKB>()
            .ActivateOnEnter<ExplosionTowers>()
            .ActivateOnEnter<ExplosionTowerKnockback>()
            .ActivateOnEnter<MajesticMeteor>()
            .ActivateOnEnter<Tether1>()
            .ActivateOnEnter<Tether2>()
            .ActivateOnEnter<MeteorainPortals>()
            .ActivateOnEnter<MeteorMechanicHints>()
            .ActivateOnEnter<MassiveMeteor>()
            .ActivateOnEnter<MajesticMeteorStorm>()
            .ActivateOnEnter<ArcadionAvalanche>()
            .ActivateOnEnter<ArcadionAvalancheSmash>()
            .DeactivateOnExit<Flatliner>();
        ComponentCondition<FlatlinerKB>(id + 0x10, 10f, comp => comp.NumCasts != 0)
            .DeactivateOnExit<FlatlinerKB>();
        CastMulti(id + 0x30, [(uint)AID.ArcadionAvalanche_Pick1, (uint)AID.ArcadionAvalanche_Pick2, (uint)AID.ArcadionAvalanche_Pick3, (uint)AID.ArcadionAvalanche_Pick4], delay, 15f, "Get to safe platform")
            .DeactivateOnExit<Tether1>()
            .DeactivateOnExit<Tether2>()
            .DeactivateOnExit<MajesticMeteor>()
            .DeactivateOnExit<MeteorainPortals>()
            .DeactivateOnExit<MeteorMechanicHints>()
            .DeactivateOnExit<MassiveMeteor>()
            .DeactivateOnExit<MajesticMeteorStorm>()
            .DeactivateOnExit<ArcadionAvalanche>();
    }
    private void EclipticMeteor(uint id, float delay)
    {
        Cast(id, (uint)AID.CrownOfArcadia, delay, 5f, "Crown of Arcadia")
            .DeactivateOnEnter<ExplosionTowers>()
            .DeactivateOnEnter<ExplosionTowerKnockback>()
            .DeactivateOnEnter<ArcadionAvalanche>()
            .DeactivateOnEnter<ArcadionAvalancheSmash>()
            .ActivateOnExit<GreatWallOfFire>()
            .ActivateOnExit<GreatWallOfFireExplosion>()
            .ActivateOnExit<OrbitalOmen>()
            .ActivateOnExit<FireAndFury>();

        Cast(id + 0x10, (uint)AID.GreatWallOfFire, delay, 5f, "Great Wall Of Fire");
        Cast(id + 0x20, (uint)AID.OrbitalOmen, delay, 5f, "Orbital Omen")
            .DeactivateOnExit<GreatWallOfFire>()
            .DeactivateOnExit<GreatWallOfFireExplosion>();
        Cast(id + 0x30, (uint)AID.FireAndFury, delay, 4f, "Fire and Fury")
            .DeactivateOnExit<FireAndFury>();
        Cast(id + 0x40, (uint)AID.CrownOfArcadia, delay, 5f, "Crown of Arcadia");
        Cast(id + 0x50, (uint)AID.EclipticStampede, delay, 5f, "Ecliptic Stampede")
            .ActivateOnEnter<Tether1>()
            .ActivateOnEnter<Tether2>()
            .ActivateOnEnter<MassiveMeteor>()
            .ActivateOnEnter<MammothMeteor>()
            .ActivateOnEnter<AtomicImpact>()
            .ActivateOnEnter<AtomicImpactVoidZones>()
            .ActivateOnEnter<CosmicKissTowers>()
            .ActivateOnEnter<WeightyImpactTowers>()
            .ActivateOnEnter<TwoWayFireball>()
            .ActivateOnEnter<FourWayFireball>()
            .ActivateOnEnter<HeartBreakerTower>();
    }
}
