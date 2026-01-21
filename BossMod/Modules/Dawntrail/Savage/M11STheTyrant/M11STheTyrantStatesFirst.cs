//namespace BossMod.Dawntrail.Savage.M11STheTyrant;
/*
sealed class M11STheTyrantStates : StateMachineBuilder
{
    public M11STheTyrantStates(BossModule module) : base(module)
    {
        SimplePhase(0, Phase1, "P1: Opening / Raw Steel");
        SimplePhase(1, Phase2, "P2: Ultimate Trophy Weapons");
        SimplePhase(2, Phase3, "P3: Fire / Meteorain");
    }

    // ==================================================
    // Phase 1 — Opening → Raw Steel → Charybdistopia
    // Ends on Ultimate Trophy Weapons cast
    // ==================================================
    private void Phase1(uint id)
    {
        // 00:05 Crown of Arcadia
        Cast(id, (uint)AID.CrownOfArcadia, 0f, 5f, "Crown of Arcadia")
            .ActivateOnEnter<CrownOfArcadia>();

        // Activate all Raw Steel / Assault / Stardust mechanics
        SimpleState(id + 0x1000, 0f, "Raw Steel Active")
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
            .ActivateOnEnter<Explosion>();

        // 02:55 Charybdistopia (does not end phase)
        Cast(id + 0x2000, (uint)AID.Charybdistopia, 0f, 5f, "Charybdistopia");

        // 03:03 Ultimate Trophy Weapons → PHASE TRANSITION
        Cast(id + 0x3000, (uint)AID.UltimateTrophyWeapons, 0f, 5f, "Ultimate Trophy Weapons")
            .DeactivateOnExit<RawSteelTrophyAxe>()
            .DeactivateOnExit<RawSteelTrophyScythe>()
            .DeactivateOnExit<AssaultEvolvedSword>()
            .DeactivateOnExit<AssaultEvolvedAxeStack>()
            .DeactivateOnExit<AssaultEvolved_AxeAOE>()
            .DeactivateOnExit<AssaultEvolvedScythe>()
            //.DeactivateOnExit<VoidStardust>()
            .DeactivateOnExit<Cometite>()
            .DeactivateOnExit<CrushingComet>()
            .DeactivateOnExit<EyeOfTheHurricane>()
            .DeactivateOnExit<Explosion>();
    }

    // ==================================================
    // Phase 2 — Ultimate Trophy + Maelstrom
    // Ends on One and Only cast
    // ==================================================
    private void Phase2(uint id)
    {
        SimpleState(id, 0f, "Ultimate Trophy Active")
            .ActivateOnEnter<AssaultEvolvedSword>()
            .ActivateOnEnter<AssaultEvolvedAxeStack>()
            .ActivateOnEnter<AssaultEvolved_AxeAOE>()
            .ActivateOnEnter<AssaultEvolvedScythe>()
            .ActivateOnEnter<MaelstromVoidZones>()
            .ActivateOnEnter<MaelstromGustCones>();

        // 03:53 One and Only → PHASE TRANSITION
        Cast(id + 0x1000, (uint)AID.OneAndOnly, 0f, 5f, "One and Only")
            .DeactivateOnExit<AssaultEvolvedSword>()
            .DeactivateOnExit<AssaultEvolvedAxeStack>()
            .DeactivateOnExit<AssaultEvolved_AxeAOE>()
            .DeactivateOnExit<AssaultEvolvedScythe>()
            .DeactivateOnExit<MaelstromVoidZones>()
            .DeactivateOnExit<MaelstromGustCones>();
    }

    // ==================================================
    // Phase 3 — Fire / Meteorain → Arena Split
    // ==================================================
    private void Phase3(uint id)
    {
        SimpleState(id, 0f, "Fire / Meteorain Active")
            .ActivateOnEnter<GreatWallOfFire>()
            .ActivateOnEnter<OrbitalOmen>()
            .ActivateOnEnter<FireAndFury>()
            .ActivateOnEnter<MeteorainComets>()
            .ActivateOnEnter<FearsomeFireball>()
            .ActivateOnEnter<CosmicKiss>()
            .ActivateOnEnter<CometTethers>()
            .ActivateOnEnter<TripleTyrannhilation>();

        // 05:58 Arena Split
        Cast(id + 0x1000, (uint)AID.Flatliner, 0f, 5f, "Flatliner")
            .DeactivateOnExit<GreatWallOfFire>()
            .DeactivateOnExit<OrbitalOmen>()
            .DeactivateOnExit<FireAndFury>()
            .DeactivateOnExit<MeteorainComets>()
            .DeactivateOnExit<FearsomeFireball>()
            .DeactivateOnExit<CosmicKiss>()
            .DeactivateOnExit<CometTethers>()
            .DeactivateOnExit<TripleTyrannhilation>()
            .ActivateOnEnter<Flatliner>();
    }
}
*/