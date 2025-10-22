namespace BossMod.Endwalker.DeepDungeon.PilgrimsTraverse.DD99EminentGrief;

public enum OID : uint
{
    EminentGrief = 0x48EA, // R28.5
    EminentGriefHelper = 0x486D, // R1.0
    DevouredEater = 0x48EB, // R15.0
    VodorigaMinion = 0x48EC, // R1.2
    Crystal = 0x1EBE70, // R0.5
    BallOfFire = 0x48ED, // R1.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttackVisual1 = 44820, // EminentGriefHelper->player, 0.5s cast, single-target
    AutoAttackVisual2 = 44802, // EminentGriefHelper->player, 0.5s cast, single-target
    AutoAttackVisual3 = 44095, // DevouredEater->self, no cast, single-target
    AutoAttackVisual4 = 44094, // EminentGrief->self, no cast, single-target
    AutoAttack1 = 44813, // Helper->player, 0.8s cast, single-target
    AutoAttack2 = 44096, // Helper->player, 0.8s cast, single-target
    AutoAttackAdd = 45196, // VodorigaMinion->player, no cast, single-target

    ChainsOfCondemnationVisual1 = 44063, // EminentGrief->location, 5.3+0,7s cast, single-target
    ChainsOfCondemnationVisual2 = 44069, // EminentGrief->location, 8.3+0,7s cast, single-target
    ChainsOfCondemnation1 = 44064, // Helper->location, 6.0s cast, range 30 circle, applies chains of condemnation status
    ChainsOfCondemnation2 = 44070, // Helper->location, 9.0s cast, range 30 circle

    BladeOfFirstLightVisual1 = 44072, // DevouredEater->self, 8.2+0,8s cast, single-target
    BladeOfFirstLightVisual2 = 44071, // DevouredEater->self, 8.2+0,8s cast, single-target
    BladeOfFirstLightVisual3 = 44065, // DevouredEater->self, 5.2+0,8s cast, single-target
    BladeOfFirstLightVisual4 = 44066, // DevouredEater->self, 5.2+0,8s cast, single-target
    BladeOfFirstLight1 = 44073, // Helper->self, 9.0s cast, range 30 width 15 rect
    BladeOfFirstLight2 = 44067, // Helper->self, 6.0s cast, range 30 width 15 rect

    BoundsOfSinVisual = 44081, // DevouredEater->self, 3.3+0,7s cast, single-target
    BoundsOfSinPull = 44082, // Helper->self, 4.0s cast, range 40 circle, pull into middle
    BoundsOfSin = 44083, // Helper->self, 3.0s cast, range 3 circle
    BoundsOfSinEnd = 44084, // Helper->self, no cast, range 8 circle, aoe in middle

    SpinelashVisual1 = 44085, // EminentGrief->self, 2.0s cast, single-target
    SpinelashVisual2 = 44086, // EminentGrief->self, 1.0+0,8s cast, single-target
    Spinelash = 45118, // Helper->self, 1.8s cast, range 60 width 4 rect
    SpinelashEndVisual = 44087, // EminentGrief->self, no cast, single-target

    TerrorEye = 45115, // VodorigaMinion->location, 3.0s cast, range 6 circle
    BloodyClaw = 45114, // VodorigaMinion->player, no cast, single-target

    DrainAetherVisual1 = 44092, // DevouredEater->self, 11.0+1,0s cast, single-target
    DrainAetherVisual2 = 44090, // DevouredEater->self, 6.0+1,0s cast, single-target
    DrainAetherVisual3 = 44270, // Helper->EminentGrief, no cast, single-target
    DrainAetherVisual4 = 44314, // Helper->none, no cast, single-target
    DrainAether1 = 44088, // EminentGrief->self, 7.0s cast, range 50 width 50 rect, need light
    DrainAether2 = 44093, // EminentGriefHelper->self, 12.0s cast, range 50 width 50 rect, need darkness
    DrainAether3 = 44091, // EminentGriefHelper->self, 7.0s cast, range 50 width 50 rect, need darkness
    DrainAether4 = 44089, // EminentGrief->self, 12.0s cast, range 50 width 50 rect, need light

    BallOfFireVisual1 = 44068, // EminentGrief->self, 9.0s cast, single-target
    BallOfFireVisual2 = 44061, // EminentGrief->self, 6.0s cast, single-target
    BallOfFire = 44062, // Helper->location, 2.1s cast, range 6 circle

    AbyssalBlazeVisualWE1 = 44074, // EminentGrief->self, 3.0s cast, single-target
    AbyssalBlazeVisualNS1 = 44075, // EminentGrief->self, 3.0s cast, single-target
    AbyssalBlazeVisualNS2 = 44077, // EminentGrief->self, no cast, single-target
    AbyssalBlazeVisualWE2 = 44076, // EminentGrief->self, no cast, single-target
    SpawnCrystal = 44078, // Helper->location, no cast, single-target
    AbyssalBlazeFirst = 44079, // Helper->location, 7.0s cast, range 5 circle
    AbyssalBlazeRest = 44080 // Helper->location, no cast, range 5 circle
}

public enum SID : uint
{
    ChainsOfCondemnation = 4562, // Helper->player, extra=0x0
    LightVengeance = 4560, // none->player, extra=0x0
    DarkVengeance = 4559 // none->player, extra=0x0
}

public enum IconID : uint
{
    Spinelash = 234 // player->self
}

[SkipLocalsInit]
sealed class LightAndDark(BossModule module) : Components.GenericAOEs(module)
{
    private readonly DateTime[] expirations = new DateTime[4];
    private BitMask lightBuff;
    private BitMask darkBuff;
    private BitMask combined;
    private readonly List<AOEInstance> _aoes = new(2);
    private readonly List<ShapeDistance> _sdfs = new(2);
    private readonly bool[] wantLight = new bool[2];
    private readonly DateTime[] activations = new DateTime[2];
    private bool aetherdrainActive;
    private readonly DD99EminentGrief bossmod = (DD99EminentGrief)module;
    private int numPartyMembersHalf;
    private float hpDifference;
    private uint griefHP, eaterHP;

    // extracted from collision data - dark material ID: 00027004, light material ID: 20007004
    private readonly WPos[] light1 = [new(-602.77661f, -288.82251f), new(-602.86578f, -288.14209f), new(-603.28021f, -288.181f), new(-603.06482f, -287.6239f),
        new(-602.72101f, -287.4534f), new(-602.63885f, -286.80841f), new(-603.35138f, -286.24081f), new(-603.75201f, -286.42239f), new(-604.20679f, -285.50009f),
        new(-604.63623f, -285.79639f), new(-604.91998f, -285.5f), new(-605.23059f, -285.63519f), new(-605.27417f, -285.89001f), new(-605.99017f, -286.302f),
        new(-605.64441f, -286.8378f), new(-605.70526f, -287.20941f), new(-606.05365f, -287.26208f), new(-606.5f, -288.02069f), new(-605.77319f, -288.67291f),
        new(-605.073f, -288.492f), new(-604.59839f, -289.13849f), new(-604.21881f, -289.07501f), new(-603.24841f, -289.5f)];
    private readonly WPos[] light2 = [new(-586.74078f, -310.64859f), new(-586.92242f, -310.24799f), new(-586f, -309.79321f), new(-586.29651f, -309.36389f),
        new(-586f, -309.0799f), new(-586.13513f, -308.76941f), new(-586.39001f, -308.72589f), new(-586.802f, -308.0098f), new(-587.33783f, -308.35559f),
        new(-587.70953f, -308.29471f), new(-587.76233f, -307.94629f), new(-588.52069f, -307.5f), new(-589.17285f, -308.22672f), new(-588.992f, -308.927f),
        new(-589.63861f, -309.40161f), new(-589.57489f, -309.78131f), new(-590f, -310.75159f), new(-589.32239f, -311.22339f), new(-588.64203f, -311.13419f),
        new(-588.68103f, -310.71979f), new(-588.12378f, -310.9353f), new(-587.95331f, -311.27911f), new(-587.30841f, -311.36111f)];
    private readonly WPos[] light3 = [new(-580.2074f, -294.15311f), new(-580.27429f, -293.3024f), new(-580.58514f, -293.35129f), new(-580.42352f, -292.65479f),
        new(-580.16559f, -292.44171f), new(-580.10419f, -291.63541f), new(-580.63861f, -290.92599f), new(-580.93903f, -291.15302f), new(-581.28021f, -290f),
        new(-581.60217f, -290.37051f), new(-581.81512f, -290f), new(-582.04791f, -290.16879f), new(-582.08063f, -290.48749f), new(-582.61761f, -291.0025f),
        new(-582.35834f, -291.67221f), new(-582.40393f, -292.13681f), new(-582.66528f, -292.20261f), new(-583f, -293.15079f), new(-582.45502f, -293.96619f),
        new(-581.92981f, -293.73999f), new(-581.57397f, -294.54919f), new(-581.28912f, -294.4686f), new(-580.56128f, -295f)];
    private readonly WPos[] light4 = [new(-595.6203f, -304.4549f), new(-595.75598f, -303.92981f), new(-595.27112f, -303.57379f), new(-595.31879f, -303.289f),
        new(-595f, -302.56131f), new(-595.50818f, -302.2074f), new(-596.01862f, -302.27429f), new(-595.9892f, -302.58511f), new(-596.4071f, -302.42349f),
        new(-596.53497f, -302.16571f), new(-597.01874f, -302.10419f), new(-597.4444f, -302.63849f), new(-597.30823f, -302.939f), new(-598f, -303.28009f),
        new(-597.77771f, -303.6022f), new(-598f, -303.81311f), new(-597.89874f, -304.04791f), new(-597.70752f, -304.0806f), new(-597.39862f, -304.6178f),
        new(-596.9967f, -304.35831f), new(-596.7179f, -304.4039f), new(-596.67847f, -304.66541f), new(-596.1095f, -305.00021f)];
    private readonly WPos[] light5 = [new(-609.2074f, -304.49191f), new(-609.27429f, -303.98141f), new(-609.58514f, -304.01071f), new(-609.42352f, -303.5929f),
        new(-609.16559f, -303.465f), new(-609.10419f, -302.98129f), new(-609.63861f, -302.5556f), new(-609.93903f, -302.6918f), new(-610.28009f, -302f),
        new(-610.60217f, -302.22229f), new(-610.81354f, -302f), new(-611.04797f, -302.10129f), new(-611.08063f, -302.29251f), new(-611.61761f, -302.6015f),
        new(-611.35834f, -303.0033f), new(-611.40393f, -303.2821f), new(-611.66528f, -303.32159f), new(-612.00006f, -303.8905f), new(-611.45514f, -304.3797f),
        new(-610.92981f, -304.24399f), new(-610.57379f, -304.72891f), new(-610.28912f, -304.68121f), new(-609.56158f, -305f)];
    private readonly WPos[] dark1 = [new(-610.82715f, -291.77328f), new(-611.008f, -291.07309f), new(-610.36139f, -290.59839f), new(-610.42511f, -290.21869f),
        new(-610f, -289.24841f), new(-610.67761f, -288.77661f), new(-611.35797f, -288.86581f), new(-611.31897f, -289.28021f), new(-611.87622f, -289.0647f),
        new(-612.04669f, -288.72089f), new(-612.69159f, -288.63889f), new(-613.25922f, -289.35141f), new(-613.07758f, -289.75201f), new(-614f, -290.20679f),
        new(-613.70361f, -290.6362f), new(-614f, -290.9201f), new(-613.86487f, -291.23059f), new(-613.60999f, -291.27411f), new(-613.19806f, -291.9902f),
        new(-612.66217f, -291.64441f), new(-612.29047f, -291.70529f), new(-612.23779f, -292.05371f), new(-611.47931f, -292.5f)];
    private readonly WPos[] dark2 = [new(-594.76941f, -314.36499f), new(-594.72583f, -314.10999f), new(-594.00983f, -313.698f), new(-594.35559f, -313.1622f),
        new(-594.29474f, -312.79059f), new(-593.94678f, -312.73819f), new(-593.5f, -311.97931f), new(-594.22681f, -311.32709f), new(-594.927f, -311.508f),
        new(-595.40161f, -310.86151f), new(-595.78125f, -310.9252f), new(-596.75159f, -310.5f), new(-597.22339f, -311.1777f), new(-597.13434f, -311.85809f),
        new(-596.71979f, -311.819f), new(-596.9353f, -312.37631f), new(-597.27911f, -312.54681f), new(-597.36115f, -313.1918f), new(-596.64862f, -313.75931f),
        new(-596.24799f, -313.57761f), new(-595.79321f, -314.5f), new(-595.36377f, -314.20361f), new(-595.08002f, -314.5f)];
    private readonly WPos[] dark3 = [new(-618.39783f, -309.62949f), new(-618.18488f, -310f), new(-617.95209f, -309.83121f), new(-617.91937f, -309.5126f),
        new(-617.38239f, -308.9975f), new(-617.64166f, -308.32791f), new(-617.59607f, -307.86319f), new(-617.33472f, -307.79739f), new(-617f, -306.84921f),
        new(-617.54498f, -306.03381f), new(-618.07019f, -306.26001f), new(-618.42657f, -305.452f), new(-618.71088f, -305.5314f), new(-619.43872f, -305f),
        new(-619.7926f, -305.84689f), new(-619.72571f, -306.6976f), new(-619.41486f, -306.64871f), new(-619.57648f, -307.34531f), new(-619.83441f, -307.55829f),
        new(-619.89594f, -308.36459f), new(-619.36139f, -309.07419f), new(-619.06097f, -308.84711f), new(-618.71991f, -310.00009f)];
    private readonly WPos[] dark4 = [new(-588.95221f, -297.8988f), new(-588.91962f, -297.70761f), new(-588.38239f, -297.3985f), new(-588.64166f, -296.9967f),
        new(-588.59607f, -296.7179f), new(-588.33472f, -296.67841f), new(-588.00006f, -296.1095f), new(-588.54523f, -295.6203f), new(-589.07019f, -295.7561f),
        new(-589.42621f, -295.27109f), new(-589.71088f, -295.31879f), new(-590.43872f, -295f), new(-590.7926f, -295.50809f), new(-590.72571f, -296.01859f),
        new(-590.41486f, -295.98929f), new(-590.57648f, -296.4071f), new(-590.83441f, -296.535f), new(-590.89581f, -297.01871f), new(-590.36139f, -297.4444f),
        new(-590.06097f, -297.3082f), new(-589.71991f, -298f), new(-589.39783f, -297.77771f), new(-589.18488f, -298f)];
    private readonly WPos[] dark5 = [new(-602.5556f, -297.36151f), new(-602.69177f, -297.061f), new(-602f, -296.71991f), new(-602.22229f, -296.39789f),
        new(-602f, -296.18491f), new(-602.10126f, -295.95209f), new(-602.29248f, -295.9194f), new(-602.6015f, -295.38239f), new(-603.0033f, -295.64169f),
        new(-603.2821f, -295.5961f), new(-603.32159f, -295.33481f), new(-603.8905f, -295.00021f), new(-604.3797f, -295.5451f), new(-604.24402f, -296.07019f),
        new(-604.72888f, -296.42621f), new(-604.68121f, -296.711f), new(-605f, -297.43869f), new(-604.49182f, -297.7926f), new(-603.98138f, -297.72571f),
        new(-604.0108f, -297.41489f), new(-603.5929f, -297.57651f), new(-603.46503f, -297.83429f), new(-602.98126f, -297.89581f)];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public void AddAOE()
    {
        var center = Arena.Center;
        var shapeLight = new AOEShapeCustom([new PolygonCustom(light1), new PolygonCustom(light2), new PolygonCustom(light3),
                new PolygonCustom(light4), new PolygonCustom(light5)]);
        _aoes.Add(new(shapeLight, center, color: Colors.Light, risky: false, shapeDistance: shapeLight.InvertedDistance(center, default)));
        _sdfs.Add(shapeLight.Distance(center, default));
        var shapeDark = new AOEShapeCustom([new PolygonCustom(dark1), new PolygonCustom(dark2), new PolygonCustom(dark3),
                new PolygonCustom(dark4), new PolygonCustom(dark5)]);
        _aoes.Add(new(shapeDark, center, color: Colors.FutureVulnerable, risky: false, shapeDistance: shapeDark.InvertedDistance(center, default)));
        _sdfs.Add(shapeDark.Distance(center, default));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.DrainAether1:
                UpdateWantedStatus(0, true);
                break;
            case (uint)AID.DrainAether2:
                UpdateWantedStatus(1, false);
                break;
            case (uint)AID.DrainAether3:
                UpdateWantedStatus(0, false);
                break;
            case (uint)AID.DrainAether4:
                UpdateWantedStatus(1, true);
                break;
        }

        void UpdateWantedStatus(int slot, bool wantL)
        {
            wantLight[slot] = wantL;
            aetherdrainActive = true;
            activations[slot] = Module.CastFinishAt(spell, -1d);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.DrainAether1 or (uint)AID.DrainAether2 or (uint)AID.DrainAether3 or (uint)AID.DrainAether4)
        {
            if ((++NumCasts & 1) == 0)
            {
                aetherdrainActive = false;
            }
            wantLight[0] = wantLight[1];
            activations[0] = activations[1];
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.LightVengeance:
                UpdateStatus(ref lightBuff, actor, status.ExpireAt);
                break;
            case (uint)SID.DarkVengeance:
                UpdateStatus(ref darkBuff, actor, status.ExpireAt);
                break;
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.LightVengeance:
                UpdateStatus(ref lightBuff, actor, default);
                break;
            case (uint)SID.DarkVengeance:
                UpdateStatus(ref darkBuff, actor, default);
                break;
        }
    }

    private void UpdateStatus(ref BitMask mask, Actor actor, DateTime value)
    {
        var slot = Raid.FindSlot(actor.InstanceID);
        if (value == default)
        {
            mask.Clear(slot);
            combined.Clear(slot);
        }
        else
        {
            mask.Set(slot);
            combined.Set(slot);
        }
        if (slot >= 0)
        {
            expirations[slot] = value;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        const string light = "Get light buff!";
        const string dark = "Get dark buff!";
        if (Module.StateMachine.ActivePhaseIndex == -1)
        {
            var countL = lightBuff.NumSetBits();
            var countD = darkBuff.NumSetBits();
            if (!combined[slot])
            {
                if (combined.NumSetBits() == 0 || countL == 1 && countD == 1)
                {
                    hints.Add("Get light or dark buff!");
                }
                else if (countL > countD)
                {
                    hints.Add(dark);
                }
                else
                {
                    hints.Add(light);
                }
            }
            else
            {
                if (numPartyMembersHalf == 0)
                {
                    numPartyMembersHalf = (int)MathF.Ceiling(Module.Raid.WithoutSlot(true, true, true).Length * 0.5f);
                }
                if (darkBuff[slot] && countD > numPartyMembersHalf || lightBuff[slot] && countL > numPartyMembersHalf)
                {
                    hints.Add("Switch to other color!");
                }
            }
        }
        else if (aetherdrainActive)
        {
            if (wantLight[0])
            {
                hints.Add(light, !lightBuff[slot]);
            }
            else
            {
                hints.Add(dark, !darkBuff[slot]);
            }
        }
        else if (!aetherdrainActive)
        {
            if (eaterHP <= 1u && !lightBuff[slot])
            {
                hints.Add(light);
            }
            else if (griefHP <= 1u && !darkBuff[slot])
            {
                hints.Add(dark);
            }
            else if (Math.Abs(hpDifference) > 25f || !combined[slot])
            {
                if (hpDifference > 0f)
                {
                    hints.Add(dark);
                }
                else
                {
                    hints.Add(light);
                }
            }
        }
        else
        {
            hints.Add($"Target {(darkBuff[slot] ? bossmod.BossEater?.Name : Module.PrimaryActor.Name)}!", false);
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (hpDifference != default)
        {
            hints.Add($"Eater HP: {(hpDifference > 0f ? "+" : "")}{hpDifference:f1}%");
        }
    }

    public override void Update()
    {
        if (bossmod.BossEater is Actor eater)
        {
            ref var eaterHPref = ref eater.HPMP;
            ref var primaryHPref = ref Module.PrimaryActor.HPMP;
            var eaterHPs = eaterHP = eaterHPref.CurHP;
            var griefHPs = griefHP = primaryHPref.CurHP;
            hpDifference = (int)(eaterHPs - griefHPs) * 100f / primaryHPref.MaxHP;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            var oid = e.Actor.OID;
            ref var hp = ref e.Actor.HPMP;
            e.Priority = hp.CurHP <= 1u ? AIHints.Enemy.PriorityInvincible : darkBuff[slot] && oid == (uint)OID.DevouredEater ? 0
                : lightBuff[slot] && oid == (uint)OID.EminentGrief ? 0 : oid == (uint)OID.VodorigaMinion ? 1 : AIHints.Enemy.PriorityInvincible;
        }

        var aoes = CollectionsMarshal.AsSpan(_aoes);
        ref var aoeLight = ref aoes[0];
        ref var aoeDark = ref aoes[1];
        aoeDark.Risky = false;
        aoeLight.Risky = false;
        if (!aetherdrainActive)
        {
            if (eaterHP <= 1u && !lightBuff[slot])
            {
                aoeLight.Risky = true;
            }
            else if (griefHP <= 1u && !darkBuff[slot])
            {
                aoeDark.Risky = true;
            }
            else if (Math.Abs(hpDifference) > 25f || !combined[slot])
            {
                if (hpDifference > 0f)
                {
                    aoeDark.Risky = true;
                }
                else
                {
                    aoeLight.Risky = true;
                }
            }
        }
        else
        {
            if (wantLight[0])
            {
                UpdateAOE(ref aoeLight, _sdfs[1], lightBuff);
            }
            else
            {
                UpdateAOE(ref aoeDark, _sdfs[0], darkBuff);
            }
        }
        base.AddAIHints(slot, actor, assignment, hints);
        void UpdateAOE(ref AOEInstance aoeWant, ShapeDistance aoeAvoid, BitMask mask)
        {
            if (!mask[slot] || expirations[slot] < aoeWant.Activation)
            {
                aoeWant.Risky = true;
                aoeWant.Activation = activations[0];
            }
            hints.TemporaryObstacles.Add(aoeAvoid);
        }
    }
}

[SkipLocalsInit]
sealed class TerrorEyeBallOfFire(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.TerrorEye, (uint)AID.BallOfFire], 6f);

[SkipLocalsInit]
sealed class ChainsOfCondemnation(BossModule module) : Components.StayMove(module, 2d)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.ChainsOfCondemnation1 or (uint)AID.ChainsOfCondemnation2)
        {
            var act = Module.CastFinishAt(spell, 0.1d);
            Array.Fill(PlayerStates, new(Requirement.Stay2, act));
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.ChainsOfCondemnation && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
        {
            PlayerStates[slot] = default;

            // ensure players who never got the debuff are properly reset
            var party = Raid.WithSlot(true, false, false);
            var len = party.Length;
            for (var i = 0; i < len; ++i)
            {
                ref var p = ref party[i];
                if (p.Item2.IsDead)
                {
                    PlayerStates[p.Item1] = default;
                }
            }
        }
    }
}

[SkipLocalsInit]
sealed class BladeOfFirstLight(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.BladeOfFirstLight1, (uint)AID.BladeOfFirstLight2], new AOEShapeRect(30f, 7.5f));

[SkipLocalsInit]
sealed class BoundsOfSinSmallAOE : Components.SimpleAOEs
{
    public BoundsOfSinSmallAOE(BossModule module) : base(module, (uint)AID.BoundsOfSin, 3f)
    {
        MaxDangerColor = 3;
    }
}

[SkipLocalsInit]
sealed class BoundsOfSinEnd(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];
    private readonly AOEShapeCircle circle = new(8f);
    private readonly List<Polygon> pillars = new(12);
    private const float radius = 2.57745f; // adjusted for hitbox radius

    private readonly Polygon[] pillarPolygons =
    [
        new(new(-600f, -306.99991f), radius, 64), // ENVC 0x00
        new(new(-596.50012f, -306.06201f), radius, 64, -30f.Degrees()), // ENVC 0x01
        new(new(-593.93793f, -303.49991f), radius, 64, -60f.Degrees()), // ENVC 0x02
        new(new(-592.99988f, -299.99991f), radius, 64, -89.98f.Degrees()), // ENVC 0x03
        new(new(-593.93781f, -296.49991f), radius, 64, -120f.Degrees()), // ENVC 0x04
        new(new(-596.50012f, -293.93771f), radius, 64, -150f.Degrees()), // ENVC 0x05
        new(new(-600f, -293f), radius, 64), // ENVC 0x06
        new(new(-603.5f, -293.93781f), radius, 64, 150f.Degrees()), // ENVC 0x07
        new(new(-606.06219f, -296.5f), radius, 64, 120f.Degrees()), //  ENVC 0x08
        new(new(-607.00012f, -300f), radius, 64, 89.98f.Degrees()), //  ENVC 0x09
        new(new(-606.06219f, -303.5f), radius, 64, 60f.Degrees()), // ENVC 0x0A
        new(new(-603.5f, -306.06219f), radius, 64, 30f.Degrees())// ENVC 0x0B
    ];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.BoundsOfSinPull)
        {
            var loc = Arena.Center.Quantized();
            _aoe = [new(circle, loc, default, WorldState.FutureTime(6d), shapeDistance: circle.Distance(loc, default))]; // true activation is 7.7, but slightly lower values seem to improve the pathfinding here
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.BoundsOfSinEnd)
        {
            _aoe = [];
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index is >= 0x00 and <= 0x0B)
        {
            if (state == 0x00020001u)
            {
                pillars.Add(pillarPolygons[index]);
                Arena.Bounds = new ArenaBoundsCustom([new Rectangle(DD99EminentGrief.ArenaCenter, 20f, 15f)], [.. pillars]);
            }
            else if (index == 0x00 && state == 0x00080004u)
            {
                Arena.Bounds = new ArenaBoundsCustom([new Rectangle(DD99EminentGrief.ArenaCenter, 20f, 15f)], AdjustForHitboxOutwards: true);
                pillars.Clear();
            }
        }
    }
}

[SkipLocalsInit]
sealed class SpinelashBait(BossModule module) : Components.GenericBaitAway(module)
{
    private readonly AOEShapeRect rect = new(60f, 2f);

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.Spinelash)
        {
            CurrentBaits.Add(new(Module.PrimaryActor, actor, rect, WorldState.FutureTime(6.3d), customRotation: Angle.AnglesCardinals[1]));
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Spinelash)
        {
            CurrentBaits.Clear();
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints) { } // handled by hint component

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) { }  // not really needed until cast starts
}

[SkipLocalsInit]
sealed class SpinelashBaitHint(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];
    private Actor? target;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.Spinelash)
        {
            target = actor;
            AOEShapeCustom shape = new([new Rectangle(new(-593f, -300f), 1f, 16f), new Rectangle(new(-607f, -300f), 1f, 16f)]);
            var pos = Arena.Center;
            _aoe = [new(shape, pos, default, WorldState.FutureTime(6.3d), Colors.SafeFromAOE, shapeDistance: shape.InvertedDistance(pos, default))];
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Spinelash)
        {
            target = null;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (target == actor)
        {
            ref var aoe = ref _aoe[0];
            hints.Add("Avoid breaking windows!", aoe.Check(actor.Position));
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (target == actor)
        {
            base.AddAIHints(slot, actor, assignment, hints);
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (target == pc)
        {
            base.DrawArenaBackground(pcSlot, pc);
        }
    }
}

[SkipLocalsInit]
sealed class Spinelash(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Spinelash, new AOEShapeRect(60f, 2f));

[SkipLocalsInit]
sealed class BallOfFire(BossModule module) : Components.GenericBaitAway(module, centerAtTarget: true)
{
    private readonly AOEShapeCircle circle = new(6f);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.BallOfFireVisual1:
            case (uint)AID.BallOfFireVisual2:
                var party = Raid.WithoutSlot(false, true, true);
                var len = party.Length;
                var act = Module.CastFinishAt(spell, 0.1d);
                for (var i = 0; i < len; ++i)
                {
                    CurrentBaits.Add(new(Module.PrimaryActor, party[i], circle, act));
                }
                break;
            case (uint)AID.BallOfFire:
                CurrentBaits.Clear();
                break;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints) { } // handled by hint component

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) { }  // not really needed until cast starts
}

[SkipLocalsInit]
sealed class AbyssalBlaze(BossModule module) : Components.Exaflare(module, 5f)
{
    private readonly List<(WDir, WPos)> crystals = new(8);
    private WDir next;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.AbyssalBlazeFirst)
        {
            var count = crystals.Count;
            var pos = caster.Position;
            var crys = CollectionsMarshal.AsSpan(crystals);

            for (var i = 0; i < count; ++i)
            {
                ref var c = ref crys[i];
                var loc = c.Item2;
                if (loc.AlmostEqual(pos, 3f))
                {
                    var dir = c.Item1;
                    var offset = loc - Arena.Center;
                    var intersect = (int)Intersect.RayAABB(offset, dir, 18.1f, 14.1f);
                    var maxexplosions = intersect / 4 + 1;
                    var act = Module.CastFinishAt(spell);
                    Lines.Add(new(loc, 4f * dir, act, 1d, maxexplosions, maxexplosions, rotation: dir.ToAngle()));
                    intersect = (int)Intersect.RayAABB(offset, -dir, 18.1f, 14.1f);
                    maxexplosions = intersect / 4 + 1;
                    Lines.Add(new(loc, -4f * dir, act, 1d, maxexplosions, maxexplosions, rotation: (-dir).ToAngle()));
                    if (Lines.Count == 16)
                    {
                        crystals.Clear();
                    }
                    return;
                }
            }
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.Crystal)
        {
            crystals.Add((next, actor.Position));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.AbyssalBlazeVisualWE1:
            case (uint)AID.AbyssalBlazeVisualWE2:
                next = new(1f, default);
                break;
            case (uint)AID.AbyssalBlazeVisualNS1:
            case (uint)AID.AbyssalBlazeVisualNS2:
                next = new(default, 1f);
                break;
            case (uint)AID.AbyssalBlazeFirst:
                var pos = caster.Position;

                var count = Lines.Count - 1;
                for (var i = count; i >= 0; --i)
                {
                    var line = Lines[i];
                    if (line.Next.AlmostEqual(pos, 0.1f) && (WorldState.CurrentTime - line.NextExplosion).TotalSeconds < 0.5d)
                    {
                        AdvanceLine(line, pos);
                        if (line.ExplosionsLeft == 0)
                        {
                            Lines.RemoveAt(i);
                        }
                    }
                }
                break;
            case (uint)AID.AbyssalBlazeRest:
                var pos2 = spell.TargetXZ;

                var count2 = Lines.Count;
                for (var i = 0; i < count2; ++i)
                {
                    var line = Lines[i];
                    if (line.Next.AlmostEqual(pos2, 0.1f) && caster.Rotation.AlmostEqual(line.Rotation, Angle.DegToRad))
                    {
                        AdvanceLine(line, pos2);
                        if (line.ExplosionsLeft == 0)
                        {
                            Lines.RemoveAt(i);
                        }
                        return;
                    }
                }
                break;
        }
    }
}

[SkipLocalsInit]
sealed class DD99EminentGriefStates : StateMachineBuilder
{
    public DD99EminentGriefStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SpinelashBaitHint>()
            .ActivateOnEnter<ChainsOfCondemnation>()
            .ActivateOnEnter<BladeOfFirstLight>()
            .ActivateOnEnter<BoundsOfSinSmallAOE>()
            .ActivateOnEnter<BoundsOfSinEnd>()
            .ActivateOnEnter<Spinelash>()
            .ActivateOnEnter<SpinelashBait>()
            .ActivateOnEnter<TerrorEyeBallOfFire>()
            .ActivateOnEnter<BallOfFire>()
            .ActivateOnEnter<AbyssalBlaze>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.AISupport,
StatesType = typeof(DD99EminentGriefStates),
ConfigType = null,
ObjectIDType = typeof(OID),
ActionIDType = typeof(AID),
StatusIDType = typeof(SID),
TetherIDType = null,
IconIDType = typeof(IconID),
PrimaryActorOID = (uint)OID.EminentGrief,
Contributors = "The Combat Reborn Team (Malediktus)",
Expansion = BossModuleInfo.Expansion.Dawntrail,
Category = BossModuleInfo.Category.DeepDungeon,
GroupType = BossModuleInfo.GroupType.CFC,
GroupID = 1041u,
NameID = 14037u,
SortOrder = 1,
PlanLevel = 0)]
[SkipLocalsInit]
public sealed class DD99EminentGrief : BossModule // module also works in Final Verse normal, everything but the zone ID seem to be identical
{
    public DD99EminentGrief(WorldState ws, Actor primary) : base(ws, primary, ArenaCenter, new ArenaBoundsCustom([new Rectangle(ArenaCenter, 20f, 15f)], AdjustForHitboxOutwards: true))
    {
        ActivateComponent<LightAndDark>();
        FindComponent<LightAndDark>()!.AddAOE();
        vodorigas = Enemies((uint)OID.VodorigaMinion);
    }

    public static readonly WPos ArenaCenter = new(-600f, -300f);
    public Actor? BossEater;
    private readonly List<Actor> vodorigas;

    protected override void UpdateModule()
    {
        BossEater ??= GetActor((uint)OID.DevouredEater);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(vodorigas);
    }
}
