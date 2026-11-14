namespace BossMod.Dawntrail.Criterion.C01AMT.C013PariOfPlenty;

sealed class C013PariOfPlentyStates : StateMachineBuilder
{
    private static readonly AID[] _fireflight = [AID.FireflightByEmberlightRight, AID.FireflightByEmberlightLeft, AID.FireflightByPyrelightRight, AID.FireflightByPyrelightLeft];
    private static readonly AID[] _kindling = [AID.ScatteredKindlingCast, AID.KindledFlame1Cast];
    private static readonly AID[] _longNights = [AID.FireflightFourLongNightsE, AID.FireflightFourLongNightsW];
    private static readonly AID[] _charmdFlight = [AID.CharmdFlightFourNightsW, AID.CharmdFlightFourNightsE];

    public C013PariOfPlentyStates(BossModule module) : base(module)
    {
        DeathPhase(default, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        Fireflight1(id, 8.2f);
        KindledFlame(id + 0x10000u, 12.7f);
        FourLongNights(id + 0x20000u, 7f);
        ParisCurse(id + 0x30000u, 9.2f);
        SpurningFlames(id + 0x40000u, 6.8f);
        Doubling(id + 0x50000u, 7.2f);
        CharmedFlight(id + 0x60000u, 5.9f);
        Enrage(id + 0x70000u, 8.6f);
    }

    private void Fireflight1(uint id, float delay)
    {
        Cast(id, AID.HeatBurst, delay, 5f, "Raidwide")
            .ActivateOnEnter<HeatBurst>()
            .DeactivateOnExit<HeatBurst>();

        CastStartMulti(id + 0x10u, _fireflight, 10.1f)
            .ActivateOnEnter<FireflightPath>()
            .ActivateOnEnter<FireflightRectStart>()
            .ActivateOnEnter<EmberPyre>()
            .ActivateOnEnter<SunCirclet>();

        ComponentCondition<FireflightPath>(id + 0x20u, 12.7f, static r => r.NumCasts > 0, "飞毯 1");
        ComponentCondition<FireflightPath>(id + 0x22u, 4.1f, static r => r.NumCasts > 2, "飞毯 3");
        ComponentCondition<SunCirclet>(id + 0x23u, 3.7f, static r => r.NumCasts > 0, "月环")
            .DeactivateOnExit<FireflightRectStart>()
            .DeactivateOnExit<FireflightPath>()
            .DeactivateOnExit<SunCirclet>();

        ComponentCondition<EmberPyre>(id + 0x30u, 1.1f, static e => e.NumCasts > 0, "分摊／分散")
            .DeactivateOnExit<EmberPyre>();
    }

    private void KindledFlame(uint id, float delay)
    {
        Timeout(id, default)
            .ActivateOnEnter<WheelRectStart>()
            .ActivateOnEnter<WheelOfFireflight>();

        CastMulti(id + 0x10u, _kindling, delay, 8f)
            .ActivateOnEnter<Kindling>();

        ComponentCondition<Kindling>(id + 0x20u, 0.1f, static k => k.NumCasts > 0, "分摊／分散")
            .DeactivateOnExit<Kindling>();
        ComponentCondition<WheelOfFireflight>(id + 0x21u, 0.1f, static w => w.NumCasts > 0, "安全角落")
            .DeactivateOnExit<WheelOfFireflight>()
            .DeactivateOnExit<WheelRectStart>();

        Cast(id + 0x30u, AID.FireOfVictory, 5f, 5f, "死刑")
            .ActivateOnEnter<FireOfVictory>()
            .DeactivateOnExit<FireOfVictory>();
    }

    private void FourLongNights(uint id, float delay)
    {
        CastStartMulti(id, _longNights, delay)
            .ActivateOnEnter<FourLongNightsSide>()
            .ActivateOnEnter<FourLongNightsBait>()
            .ActivateOnEnter<LongNightRectStart>();

        ComponentCondition<FourLongNightsBait>(id + 0x10u, 19.1f, static b => b.NumSpreads > 0, "诱导 1")
            .DeactivateOnExit<LongNightRectStart>();
        ComponentCondition<FourLongNightsBait>(id + 0x11u, 3f, static b => b.NumSpreads > 1, "诱导 2");
        ComponentCondition<FourLongNightsBait>(id + 0x12u, 3f, static b => b.NumSpreads > 2, "诱导 3");
        ComponentCondition<FourLongNightsBait>(id + 0x13u, 3f, static b => b.NumSpreads > 3, "诱导 4")
            .DeactivateOnExit<FourLongNightsSide>()
            .DeactivateOnExit<FourLongNightsBait>();
    }

    private void ParisCurse(uint id, float delay)
    {
        Cast(id, AID.ParisCurse, delay, 5f, "全屏伤害")
            .ActivateOnEnter<ParisCurseRaidwide>()
            .ActivateOnEnter<ParisCurseStackSpread>()
            .ActivateOnEnter<ParisCurseSolver>()
            .ActivateOnEnter<FableflightLongRectStart>()
            .ActivateOnEnter<FableflightLong>()
            .ExecOnEnter<ParisCurseStackSpread>(static p => p.EnableHints = false)
            .DeactivateOnExit<ParisCurseRaidwide>();

        Cast(id + 0x10u, AID.CharmingBaubles, 5.2f, 3f)
            .ActivateOnEnter<BurningGleam>()
            .ActivateOnEnter<ChillingGleam>();

        Cast(id + 0x20u, AID.ThievesWeave, 2.1f, 3f);
        ComponentCondition<BurningGleam>(id + 0x30u, 9.8f, static g => g.Casters.Count > 0)
            .ExecOnExit<ParisCurseStackSpread>(static p => p.EnableHints = true);

        Condition(id + 0x100u, 9.2f, () => Module.FindComponent<ChillingGleam>()?.NumCasts > 0 && Module.FindComponent<BurningGleam>()?.NumCasts > 0 && Module.FindComponent<FableflightLong>()?.NumCasts > 0, "飞毯＋十字攻击")
            .DeactivateOnExit<FableflightLongRectStart>()
            .DeactivateOnExit<FableflightLong>()
            .DeactivateOnExit<ChillingGleam>()
            .DeactivateOnExit<BurningGleam>();

        ComponentCondition<ParisCurseStackSpread>(id + 0x110u, 0.4f, static p => p.NumCasts > 0, "分摊＋分散")
            .DeactivateOnExit<ParisCurseStackSpread>()
            .DeactivateOnExit<ParisCurseSolver>();
    }

    private void SpurningFlames(uint id, float delay)
    {
        Cast(id, AID.SpurningFlames, delay, 7f, "全屏伤害")
            .ActivateOnEnter<SpurningFlames>()
            .DeactivateOnExit<SpurningFlames>();

        Cast(id + 0x10u, AID.ImpassionedSparksCast, 5.2f, 5f)
            .ActivateOnEnter<ImpassionedSparks>()
            .ActivateOnEnter<BurningPillar>();

        ComponentCondition<ImpassionedSparks>(id + 0x12u, 4.5f, static s => s.Casters.Count > 0, "火球出现");
        ComponentCondition<BurningPillar>(id + 0x13u, 5.4f, static b => b.Groups > 0, "首轮诱导");
        ComponentCondition<BurningPillar>(id + 0x14u, 15f, static b => b.Groups > 3, "末轮诱导");

        ComponentCondition<ChainsOfPassion>(id + 0x20u, 3.7f, static c => c.TethersAssigned, "锁链出现")
            .ActivateOnEnter<HotFoot>()
            .ActivateOnEnter<ChainsOfPassion>();

        ComponentCondition<HotFoot>(id + 0x30u, 4.3f, static h => h.NumFinishedSpreads > 0, "分散")
            .DeactivateOnExit<HotFoot>()
            .DeactivateOnExit<ImpassionedSparks>();

        Cast(id + 0x100u, AID.ScouringScorn, 1.3f, 6f, "全屏伤害")
            .ActivateOnEnter<ScouringScorn>()
            .DeactivateOnExit<ScouringScorn>()
            .DeactivateOnExit<BurningPillar>()
            .DeactivateOnExit<ChainsOfPassion>();
    }

    private void Doubling(uint id, float delay)
    {
        Cast(id, AID.Doubling, delay, 3f);
        Targetable(id + 0x10u, false, 4.1f, "首领消失")
            .ActivateOnEnter<Doubling>()
            .ActivateOnEnter<FableflightRectStart>()
            .ActivateOnEnter<FableflightTether>()
            .ActivateOnEnter<CarpetRideBaited>();

        ComponentCondition<Doubling>(id + 0x20u, 10.2f, static e => e.NumCasts > 0, "踩塔 1");
        ComponentCondition<CarpetRideBaited>(id + 0x21u, 0.7f, static c => c.NumCasts > 0, "诱导 1");
        ComponentCondition<Doubling>(id + 0x22u, 6.3f, static e => e.NumCasts > 4, "踩塔 2")
            .DeactivateOnExit<Doubling>();
        ComponentCondition<CarpetRideBaited>(id + 0x23u, 0.7f, static c => c.NumCasts > 2, "诱导 2")
            .DeactivateOnExit<CarpetRideBaited>()
            .DeactivateOnExit<FableflightRectStart>()
            .DeactivateOnExit<FableflightTether>();

        Targetable(id + 0x30u, true, 0.4f, "首领再现");
    }

    private void CharmedFlight(uint id, float delay)
    {
        CastStart(id, AID.KindledFlame2Cast, delay)
            .ActivateOnEnter<BurningGleam>()
            .ActivateOnEnter<KindledFlameStack>();

        ComponentCondition<KindledFlameStack>(id + 0x10u, 5.1f, static k => k.NumFinishedStacks > 0, "分摊")
            .DeactivateOnExit<KindledFlameStack>();
        ComponentCondition<BurningGleam>(id + 0x11u, 0.5f, static b => b.NumCasts > 0, "十字攻击")
            .DeactivateOnExit<BurningGleam>();

        CastMulti(id + 0x100u, _charmdFlight, 7.3f, 17.5f)
            .ActivateOnEnter<BurningGleamSlow>()
            .ActivateOnEnter<FourLongNightsSide>()
            .ActivateOnEnter<CharmdFlightRect>();

        ComponentCondition<FourLongNightsSide>(id + 0x110u, 2.1f, static f => f.NumCasts > 0, "侧面攻击 1")
            .DeactivateOnExit<CharmdFlightRect>();
        ComponentCondition<FourLongNightsSide>(id + 0x111u, 3f, static f => f.NumCasts > 1, "侧面攻击 2");
        ComponentCondition<FourLongNightsSide>(id + 0x112u, 3f, static f => f.NumCasts > 2, "侧面攻击 3");
        ComponentCondition<FourLongNightsSide>(id + 0x113u, 3f, static f => f.NumCasts > 3, "侧面攻击 4")
            .DeactivateOnExit<FourLongNightsSide>()
            .DeactivateOnExit<BurningGleamSlow>();

        Cast(id + 0x200u, AID.HeatBurst, 7f, 5f, "全屏伤害")
            .ActivateOnEnter<HeatBurst>()
            .DeactivateOnExit<HeatBurst>();
    }

    private void Enrage(uint id, float delay)
    {
        Cast(id, AID.SpurningFlamesPreEnrage, delay, 7f, "全屏伤害")
            .ActivateOnEnter<SpurningFlamesPreEnrage>()
            .DeactivateOnExit<SpurningFlamesPreEnrage>();

        Cast(id + 0x100u, AID.ScouringScornEnrageCast, 3.2f, 7f, "狂暴");
    }
}
