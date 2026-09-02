namespace BossMod.Dawntrail.Foray.ForkedTowerMagic.Extreme.FTME4Index;

[SkipLocalsInit]
sealed class ArenaChange(BossModule module) : BossComponent(module)
{
    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x00)
        {
            switch (state)
            {
                case 0x00020001:
                    Arena.Bounds = FTME4Index.OmniElementsBounds;
                    Arena.Center = FTME4Index.OmniElementsCenter;
                    break;
                case 0x00080004:
                    Arena.Bounds = FTME4Index.InitialBounds;
                    Arena.Center = FTME4Index.InitialCenter;
                    break;
            }
        }
    }
}
[SkipLocalsInit]
sealed class OmniElementPanels(BossModule module) : BossComponent(module)
{
    // spawns elemental panels with rotation 0, 60, 120
    public readonly List<Actor> Actors = [];

    public override void OnActorCreated(Actor actor)
    {
        switch (actor.OID)
        {
            case (uint)OID.OmniElementFire:
            case (uint)OID.OmniElementIce:
            case (uint)OID.OmniElementThunder:
                Actors.Add(actor);
                break;
        }
    }

    public override void OnActorDestroyed(Actor actor)
    {
        switch (actor.OID)
        {
            case (uint)OID.OmniElementFire:
            case (uint)OID.OmniElementIce:
            case (uint)OID.OmniElementThunder:
                Actors.Remove(actor);
                break;
        }
    }

    public ReadOnlySpan<ShapeDistance> GetElementSDs(bool fire = false, bool ice = false, bool lightning = false)
    {
        List<ShapeDistance> sd = [];
        var actors = CollectionsMarshal.AsSpan(Actors);
        var count = actors.Length;
        for (var i = 0; i < count; i++)
        {
            ref var actor = ref actors[i];
            var rotation = actor.Rotation;
            if (fire && actor.OID == (uint)OID.OmniElementFire ||
                ice && actor.OID == (uint)OID.OmniElementIce ||
                lightning && actor.OID == (uint)OID.OmniElementThunder
                )
            {
                sd.Add(new SDCone(FTME4Index.OmniElementsCenter, 30f, rotation, 30f.Degrees()));
                sd.Add(new SDCone(FTME4Index.OmniElementsCenter, 30f, rotation + 180f.Degrees(), 30f.Degrees()));
            }
        }

        return CollectionsMarshal.AsSpan(sd);
    }

#if DEBUG
    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var count = Actors.Count;
        for (var i = 0; i < count; i++)
        {
            var act = Actors[i];
            var position = act.Position;
            var rotation = act.Rotation;
            var dir = rotation.Normalized();
            var pos1 = position + dir.ToDirection() * 17f;
            var pos2 = position - dir.ToDirection() * 17f;
            var txt = act.OID switch
            {
                (uint)OID.OmniElementFire => "Fire",
                (uint)OID.OmniElementIce => "Ice",
                (uint)OID.OmniElementThunder => "Thunder",
                _ => "N/A"
            };
            Arena.TextWorld(pos1, txt, 0xFFFFFFFF, 14);
            Arena.TextWorld(pos2, txt, 0xFFFFFFFF, 14);
        }
    }
#endif
}

[SkipLocalsInit]
sealed class AllMightyFlames(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.SpreadTankbuster, (uint)AID.AllMightyFlames, 6f, 5.1f)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // AI may spaz while ranged are moving towards outer edge
        // let player or preset handle prepositioning
        base.AddAIHints(slot, actor, assignment, hints);
        if (IsSpreadTarget(actor))
        {
            hints.GoalZones.Add(AIHints.GoalSingleTarget(Module.PrimaryActor.Position, 12f));
        }
    }
}
[SkipLocalsInit]
sealed class AllConsumingFlames(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.Spread, (uint)AID.AllConsumingFlames, 6f, 5.1f)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (IsSpreadTarget(actor))
        {
            hints.GoalZones.Add(AIHints.GoalDonut(Module.PrimaryActor.Position, 12f, 30f));
        }
    }
}
sealed class AllKnowingFlames(BossModule module) : Components.GenericAOEs(module)
{
    private readonly AOEShapeCone _cone = new(30f, 30f.Degrees());
    private BitMask _spreading = new();

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        List<AOEInstance> aoes = [];
        var rotation = _spreading[slot] ? 60f.Degrees() : 0f.Degrees();
        aoes.Add(new(_cone, Module.PrimaryActor.Position, 0f.Degrees() + rotation));
        aoes.Add(new(_cone, Module.PrimaryActor.Position, 120f.Degrees() + rotation));
        aoes.Add(new(_cone, Module.PrimaryActor.Position, -120f.Degrees() + rotation));
        return CollectionsMarshal.AsSpan(aoes);
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID is (uint)IconID.Spread or (uint)IconID.SpreadTankbuster)
        {
            var slot = Raid.FindSlot(actor.InstanceID);
            _spreading.Set(slot);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var aoes = ActiveAOEs(slot, actor);
        var len = aoes.Length;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var aoe = ref aoes[i];
            if (aoe.Risky && aoe.Check(actor.Position))
            {
                hints.Add($"Move {(_spreading[slot] ? "into" : "out of")} spread cone!");
                return;
            }
        }
    }
}
[SkipLocalsInit]
sealed class QuadrilogyOfImplements(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private readonly AOEShapeCircle _bow = new(11f);
    private readonly AOEShapeCircle _harp = new(15f);
    private readonly AOEShapeCone _cone = new(30f, 30f.Degrees());
    private readonly List<Mechanic> _mechs = [];
    private enum Mechanic : uint
    {
        None = 0x0,
        Bell = AID.WindSlash,
        Bow = AID.Aim,
        Harp = AID.RomeosBallad,
        Sword = AID.Iainuki,
    }

    public ReadOnlySpan<AOEInstance> ActiveCasters => CollectionsMarshal.AsSpan(_aoes);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count == 0)
        {
            return [];
        }

        var aoes = CollectionsMarshal.AsSpan(_aoes);
        var count = aoes.Length;
        var start = 0;
        var end = 0;
        var id = aoes[0].ActorID;
        /*
        for (var i = 0; i < count; i++)
        {
            ref var aoe = ref aoes[i];
            if (aoe.ActorID != id)
            {
                break;
            }
            end++;
        }
        */
        ulong second = default;
        for (var i = 0; i < count; i++)
        {
            ref var aoe = ref aoes[i];
            if (aoe.ActorID != id)
            {
                if (second == default)
                {
                    second = aoe.ActorID;
                }
                else if (aoe.ActorID != second)
                {
                    break;
                }

                aoe.Risky = false;
            }
            else
            {
                aoe.Color = Colors.Danger;
            }
            end++;
        }
        /*
        var subset = aoes[start..end];
        var shockwave = Module.FindComponent<Shockwave>();
        //if (shockwave != null)
        if (shockwave?.ActiveKnockbacks(slot, actor).Length > 0)
        {
            for (var i = 0; i < end; i++)
            {
                ref var aoe = ref subset[i];
                aoe.Risky = false;
            }
        }
        */
        return aoes[start..end];
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (actor.OID == (uint)OID.Index)
        {
            var position = actor.Position;
            // gain times - 38.206 | 41.211 | 44.234 | 47.155
            // resolved   - 52.466 | 55.695 | 58.870 | 61.983
            var activation = WorldState.CurrentTime.AddSeconds(14.2d);

            // set actorID to differentiate AOEs
            var statusID = status.ID;
            switch (statusID)
            {
                case (uint)SID.SealOfTheBell:
                    _mechs.Add((Mechanic)AID.WindSlash);
                    _aoes.Add(new(_cone, position, 180f.Degrees(), activation, actorID: statusID));
                    _aoes.Add(new(_cone, position, 60f.Degrees(), activation, actorID: statusID));
                    _aoes.Add(new(_cone, position, -60f.Degrees(), activation, actorID: statusID));
                    break;
                case (uint)SID.SealOfTheBlade:
                    _mechs.Add((Mechanic)AID.Iainuki);
                    _aoes.Add(new(_cone, position, 0f.Degrees(), activation, actorID: statusID));
                    _aoes.Add(new(_cone, position, 120f.Degrees(), activation, actorID: statusID));
                    _aoes.Add(new(_cone, position, -120f.Degrees(), activation, actorID: statusID));
                    break;
                case (uint)SID.SealOfTheBow:
                    _mechs.Add((Mechanic)AID.Aim);
                    _aoes.Add(new(_bow, new(0f, -607.5f), default, activation, actorID: statusID));
                    _aoes.Add(new(_bow, new(-17.754f, -638.25f), default, activation, actorID: statusID));
                    _aoes.Add(new(_bow, new(17.754f, -638.25f), default, activation, actorID: statusID));
                    break;
                case (uint)SID.SealOfTheHarp:
                    _mechs.Add((Mechanic)AID.RomeosBallad);
                    _aoes.Add(new(_harp, position, default, activation, actorID: statusID));
                    break;
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0)
        {
            switch (spell.Action.ID)
            {
                case (uint)AID.RomeosBallad:
                case (uint)AID.Aim:
                case (uint)AID.Iainuki:
                case (uint)AID.WindSlash:
                    ++NumCasts;
                    _mechs.Remove((Mechanic)spell.Action.ID);
                    _aoes.RemoveAt(0);
                    break;
            }
        }
    }

    // ignore AOE until KBs resolve
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var shockwave = Module.FindComponent<Shockwave>();
        if (shockwave?.ActiveKnockbacks(slot, actor).Length == 0)
        {
            base.AddHints(slot, actor, hints);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var shockwave = Module.FindComponent<Shockwave>();
        if (shockwave?.ActiveKnockbacks(slot, actor).Length == 0)
        {
            base.AddAIHints(slot, actor, assignment, hints);
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add(string.Join(" -> ", _mechs));
    }
}
[SkipLocalsInit]
sealed class RomeosBallad(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RomeosBallad1, 15f)
{
    private readonly Predict _predict = module.FindComponent<Predict>()!;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _predict.ActiveAOEs(slot, actor).Length != 0 ? [] : base.ActiveAOEs(slot, actor);
}
[SkipLocalsInit]
sealed class Aim(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Aim1, 11f)
{
    private readonly Predict _predict = module.FindComponent<Predict>()!;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _predict.ActiveAOEs(slot, actor).Length != 0 ? [] : base.ActiveAOEs(slot, actor);
}
[SkipLocalsInit]
sealed class SealedImplements(BossModule module) : Components.GenericAOEs(module)
{
    private readonly Predict _predict = module.FindComponent<Predict>()!;
    private readonly List<AOEInstance> _aoes = [];
    private readonly AOEShapeCircle _aim = new(11f);
    private readonly AOEShapeCircle _romeo = new(15f);
    public int VisualCasts = 0;

    public ReadOnlySpan<AOEInstance> ActiveCasters => CollectionsMarshal.AsSpan(_aoes);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _predict.ActiveAOEs(slot, actor).Length != 0 ? [] : (ReadOnlySpan<AOEInstance>)CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.Aim1:
                _aoes.Add(new(_aim, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
                break;
            case (uint)AID.RomeosBallad1:
                _aoes.Add(new(_romeo, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0)
        {
            switch (spell.Action.ID)
            {
                case (uint)AID.Aim1:
                case (uint)AID.RomeosBallad1:
                    ++NumCasts;
                    _aoes.RemoveAt(0);
                    break;
                case (uint)AID.SealedImplementsHarp:
                case (uint)AID.SealedImplementsBow:
                    ++VisualCasts;
                    break;
            }
        }
    }
#if DEBUG
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add($"SealedNumCast[{NumCasts}] SealedVisual[{VisualCasts}]");
    }
#endif
}
[SkipLocalsInit]
sealed class ElementIII(BossModule module) : Components.GenericAOEs(module)
{
    // 8s between icon appear to resolve
    // 2 sets of icons go out
    // 2nd set happens roughly same time as Sealed Implements, resolve this before Sealed
    // whole arena may potentially be filled depending on player icon and expansion element
    // hide expansion while player has ElementIII? have some leeway on the 2nd set
    // or leave this as a text/AI hint so less visual clutter?
    private readonly OmniElementPanels _panels = module.FindComponent<OmniElementPanels>()!;
    private readonly uint[] _safePanel = new uint[PartyState.MaxAllies];
    private readonly AOEShapeCone _cone = new(30f, 30f.Degrees());
    private DateTime _activation = default;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_safePanel[slot] == default)
        {
            return [];
        }

        var safeOID = _safePanel[slot];
        List<AOEInstance> aoes = [];
        var panels = CollectionsMarshal.AsSpan(_panels.Actors);
        var count = panels.Length;
        var center = Module.PrimaryActor.Position;
        for (var i = 0; i < count; i++)
        {
            ref var panel = ref panels[i];
            if (panel.OID != safeOID)
            {
                var rotation = panel.Rotation;
                aoes.Add(new(_cone, center, rotation, _activation));
                aoes.Add(new(_cone, center, rotation + 180f.Degrees(), _activation));
            }
        }

        return CollectionsMarshal.AsSpan(aoes);
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        var safeOID = iconID switch
        {
            (uint)IconID.FireIce => (uint)OID.OmniElementThunder,
            (uint)IconID.IceThunder => (uint)OID.OmniElementFire,
            (uint)IconID.ThunderFire => (uint)OID.OmniElementIce,
            _ => default
        };

        if (safeOID == default)
        {
            return;
        }

        var slot = Raid.FindSlot(targetID);
        if (slot == -1)
        {
            return;
        }

        _activation = WorldState.CurrentTime.AddSeconds(3d);
        _safePanel[slot] = safeOID;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.FireIII or (uint)AID.BlizzardIII or (uint)AID.ThunderIII)
        {
            ++NumCasts;
            var targets = spell.Targets;
            var count = targets.Count;
            for (var i = 0; i < count; i++)
            {
                ref var target = ref targets.Ref(i);
                var slot = Raid.FindSlot(target.ID);
                if (slot != -1)
                {
                    _safePanel[slot] = default;
                }
            }
        }
    }
}
[SkipLocalsInit]
sealed class ElementaryChemistryPlatform(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ElementaryChemistryPlatform, new AOEShapeRect(15f, 7.5f))
{
    // arena change happens slightly after cast, remoe AOE on arena change
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {

    }
    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x00 && state == 0x00080004)
        {
            Casters.Clear();
        }
    }
}
[SkipLocalsInit]
sealed class SummonBombs(BossModule module) : Components.Adds(module, (uint)OID.SummonedBomb)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // focus inside bombs 1st, then outside
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; i++)
        {
            var h = hints.PotentialTargets[i];
            if (h.Actor.OID == (uint)OID.SummonedBomb)
            {
                if (h.Actor.DistanceToPoint(FTME4Index.OmniElementsCenter) <= 13f)
                {
                    h.Priority = 2;
                }
                else
                {
                    h.Priority = 1;
                }
            }
        }
    }
}
[SkipLocalsInit]
sealed class SummonBirds(BossModule module) : Components.Adds(module, (uint)OID.SummonedBird)
{
    private readonly SummonBombs _bombs = module.FindComponent<SummonBombs>()!;
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // only focus if all boms are dead
        // should face outside when killed
        if (_bombs.ActiveActors.Count == 0)
        {
            hints.PrioritizeTargetsByOID((uint)OID.SummonedBird, 1);
        }
    }
}
[SkipLocalsInit]
sealed class BladeBlitz(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Bladeblitz, new AOEShapeRect(15f, 4f), riskyWithSecondsLeft: 3d)
{
    // group by activation time
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = Casters.Count;
        if (count == 0)
        {
            return [];
        }

        var aoes = CollectionsMarshal.AsSpan(Casters);
        ref var first = ref aoes[0];
        var time = WorldState.CurrentTime;
        var actFirst = first.Activation;
        DateTime actSecond = default;
        var end = 0;

        for (var i = 0; i < count; ++i)
        {
            ref var aoe = ref aoes[i];
            if (Math.Abs((actFirst - aoe.Activation).TotalSeconds) <= 1d)
            {
                aoe.Color = Colors.Danger;
                aoe.Risky &= aoe.Activation.AddSeconds(-RiskyWithSecondsLeft) <= time;
            }
            else
            {
                if (actSecond == default)
                {
                    actSecond = aoe.Activation;
                }
                else if (Math.Abs((actSecond - aoe.Activation).TotalSeconds) > 1d)
                {
                    break;
                }
                aoe.Risky = false;
            }
            end++;
        }
        return aoes[..end];
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP,
    StatesType = typeof(FTME4IndexStates),
    ConfigType = null, // replace null with typeof(FTME1TwoHeadedAevisConfig) if applicable
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID), // replace null with typeof(AID) if applicable
    StatusIDType = typeof(SID), // replace null with typeof(SID) if applicable
    TetherIDType = typeof(TetherID), // replace null with typeof(TetherID) if applicable
    IconIDType = typeof(IconID), // replace null with typeof(IconID) if applicable
    PrimaryActorOID = (uint)OID.Index,
    Contributors = "gynorhino",
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Foray,
    GroupType = BossModuleInfo.GroupType.TheForkedTowerMagic,
    GroupID = 1114u,
    NameID = 14717u,
    SortOrder = 4,
    PlanLevel = 100)]
[SkipLocalsInit]
public sealed class FTME4Index(WorldState ws, Actor primary) : BossModule(ws, primary, InitialCenter, InitialBounds)
{
    // pulled from normal mode; check replay if arena any different in EX
    // points using material id 0x00007004
    private static readonly WPos[] _arenaInitialPos = [
        new(7.50198f, -615.00610f),new(7.49990f, -600.00012f),new(-7.50010f, -600.00012f),new(-7.50079f, -600.00067f),
        new(-7.50276f, -615.00580f),new(-15.00425f, -628.00012f),new(-27.99880f, -635.50494f),new(-20.49879f, -648.49530f),
        new(-7.50275f, -640.99445f),new(7.50200f, -640.99408f),new(20.49863f, -648.49530f),new(27.99863f, -635.50494f),
        new(15.00408f, -628.00012f),new(15.00408f, -628.00012f)];

    private static readonly WPos[] _arenaFullPos = [
        new(27.99862f, -620.49530f),new(20.49862f, -607.50494f),new(7.50198f, -615.00610f),new(7.49990f, -600.00012f),
        new(-7.50010f, -600.00012f),new(-7.50079f, -600.00067f),new(-7.50276f, -615.00580f),new(-20.49881f, -607.50494f),
        new(-27.99881f, -620.49530f),new(-15.00425f, -628.00012f),new(-27.99880f, -635.50494f),new(-20.49879f, -648.49530f),
        new(-7.50275f, -640.99445f),new(-7.50076f, -656.00049f),new(0.73911f, -656.00031f),new(7.49962f, -656.00043f),
        new(7.49992f, -656.00012f),new(7.50200f, -640.99408f),new(20.49863f, -648.49530f),new(27.99863f, -635.50494f),
        new(15.00408f, -628.00012f),new(15.00408f, -628.00012f)];

    //private static readonly WPos[] _innerHexPos = [new(-2.88752f, -623.00104f), new(0.62856f, -623.00043f), new(2.88607f, -623.00067f), new(5.77356f, -628.00012f), new(2.88633f, -633.00024f), new(-2.88692f, -633.00024f), new(-5.77374f, -628.00012f)];
    private static readonly WPos[] _innerHexPos = [new(-3f, -623f), new(3f, -623f), new(6f, -628f), new(3f, -633f), new(-3f, -633f), new(-6f, -628f)];

    private static readonly PolygonCustom[] _arenaInitial = [new(_arenaInitialPos)];
    private static readonly PolygonCustom[] _arenaFull = [new(_arenaFullPos)];
    private static readonly PolygonCustom[] _innerHex = [new(_innerHexPos)];

    public static WPos InitialCenter = new(0f, -624.25f);
    public static readonly ArenaBoundsCustom InitialBounds = new(_arenaInitial, _innerHex, Offset: -1f);

    public static WPos OmniElementsCenter = new(0f, -628f);
    public static readonly ArenaBoundsCustom OmniElementsBounds = new(_arenaFull, _innerHex, Offset: -1f);

    protected override bool CheckPull() => base.CheckPull() && Raid.Player()!.Position.InCircle(Arena.Center, 28f);
}
