namespace BossMod.Modules.Dawntrail.Savage.M09SVampFatale;

sealed class M09VampFataleStates : StateMachineBuilder
{
    public M09VampFataleStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChanges>()
            .ActivateOnEnter<KillerVoice>()
            .ActivateOnEnter<Hardcore>()
            .ActivateOnEnter<Hardcore2>()
            .ActivateOnEnter<VampStomp>()
            .ActivateOnEnter<BrutalRain>()
            .ActivateOnEnter<SadisticScreech>()
            .ActivateOnEnter<CoffinMaker>()
            .ActivateOnEnter<CoffinFiller>()
            .ActivateOnEnter<HalfMoon>()
            .ActivateOnEnter<CrowdKill>()
            .ActivateOnEnter<FinaleFatale>()
            // .ActivateOnEnter<AetherlettingCones>()
            .ActivateOnEnter<AetherlettingHit>()
            .ActivateOnEnter<AetherlettingCross>()
            .ActivateOnEnter<Hardcore>()
            .ActivateOnEnter<Hardcore2>()
            .ActivateOnEnter<VampStomp>()
            .ActivateOnEnter<HalfMoon>()
            .ActivateOnEnter<BrutalRain>()
            .ActivateOnEnter<InsatiableThirst>()
            .ActivateOnEnter<SadisticScreech>()
            // Sadistic Screech phase here
            .ActivateOnEnter<CrowdKill>()
            .ActivateOnEnter<FinaleFatale>()
            .ActivateOnEnter<HellInACell>()
            .ActivateOnEnter<CharnelCells>()
            .ActivateOnEnter<UltrasonicSpread>()
            .ActivateOnEnter<UltrasonicAmp>()
            // Hell In A Cell phase here
            .ActivateOnEnter<UndeadDeathmatch>()
            // .ActivateOnEnter<UndeadDeathmatchBats>()
            .ActivateOnEnter<SanguineScratch>()
            .ActivateOnEnter<BrutalRain>()
            .ActivateOnEnter<VampStomp>()
            .ActivateOnEnter<HalfMoon>()
            .ActivateOnEnter<Hardcore>()
            .ActivateOnEnter<Hardcore2>()
            .ActivateOnEnter<SanguineScratch>()
            .ActivateOnEnter<Enrage>();
    }
}
