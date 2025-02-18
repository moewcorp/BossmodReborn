namespace BossMod.Dawntrail.Dungeon.D05Origenics.D053Ambrose;

public enum OID : uint
{
    Boss = 0x417D, // R4.998
    Electrolance = 0x4180, // R1.38
    Superfluity = 0x417F, // R1.8
    OrigenicsEyeborg = 0x417E, // R4.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack1 = 870, // Boss->player, no cast, single-target
    AutoAttack2 = 872, // Superfluity/OrigenicsEyeborg->player, no cast, single-target
    Teleport = 36439, // Boss->location, no cast, single-target

    PsychicWave = 36436, // Boss->self, 5.0s cast, range 80 circle

    OverwhelmingChargeVisual = 36435, // Boss->self, no cast, single-target
    OverwhelmingCharge1 = 39233, // Boss->self, 5.0s cast, range 26 180-degree cone
    OverwhelmingCharge2 = 39072, // Helper->self, 9.8s cast, range 26 180-degree cone

    PsychokinesisVisual1 = 36427, // Boss->self, 10.0s cast, single-target
    PsychokinesisVisual2 = 38929, // Boss->self, 8.0s cast, single-target
    Psychokinesis = 36428, // Helper->self, 10.0s cast, range 70 width 13 rect

    ExtrasensoryField = 36432, // Boss->self, 7.0s cast, single-target
    ExtrasensoryExpulsionWestEast = 36434, // Helper->self, 7.0s cast, range 15 width 20 rect
    ExtrasensoryExpulsionNorthSouth = 36433, // Helper->self, 7.0s cast, range 20 width 15 rect

    VoltaicSlash = 36437, // Boss->player, 5.0s cast, single-target
    PsychokineticCharge = 39055, // Boss->self, 7.0s cast, single-target

    Electrolance = 36429, // Boss->location, 6.0s cast, range 22 circle

    RushTelegraph = 38953, // Helper->location, 2.5s cast, width 10 rect charge
    Rush = 38954, // Electrolance->location, no cast, width 10 rect charge
    ElectrolanceAssimilationVisual = 36430, // Boss->self, 0.5s cast, single-target
    ElectrolanceAssimilation = 36431, // Helper->self, 1.0s cast, range 33 width 10 rect

    WhorlOfTheMind = 36438 // Helper->player, 5.0s cast, range 5 circle
}

class PsychicWaveArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCustom rect = new([new Rectangle(D053Ambrose.ArenaCenter, 33f, 24f)], [new Rectangle(D053Ambrose.ArenaCenter, 15f, 19.5f)]);
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.PsychicWave && Arena.Bounds == D053Ambrose.StartingBounds)
            _aoe = new(rect, Arena.Center, default, Module.CastFinishAt(spell, 0.7f));
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (state == 0x00020001 && index == 0x28)
        {
            Arena.Bounds = D053Ambrose.DefaultBounds;
            _aoe = null;
        }
    }
}

class PsychicWave(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.PsychicWave));
class Psychokinesis(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Psychokinesis), new AOEShapeRect(70f, 6.5f));

class ExtrasensoryExpulsion(BossModule module) : Components.Knockback(module, maxCasts: 1)
{
    public readonly List<Source> Sourcez = new(4);
    public static readonly AOEShapeRect RectNS = new(20f, 7.5f);
    public static readonly AOEShapeRect RectEW = new(15f, 10f);

    public override IEnumerable<Source> Sources(int slot, Actor actor) => Sourcez;

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        var aoe = Module.FindComponent<OverwhelmingCharge>();
        if (aoe != null && aoe.AOE != null)
        {
            var activeAOE = aoe.AOE.Value;
            if (activeAOE.Check(pos))
                return true;
        }
        return !Module.InBounds(pos);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        void AddSource(AOEShape shape) => Sourcez.Add(new(spell.LocXZ, 20f, Module.CastFinishAt(spell), shape, spell.Rotation, Kind.DirForward));
        if (spell.Action.ID == (uint)AID.ExtrasensoryExpulsionNorthSouth)
            AddSource(RectNS);
        else if (spell.Action.ID == (uint)AID.ExtrasensoryExpulsionWestEast)
            AddSource(RectEW);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (Sourcez.Count != 0 && spell.Action.ID is (uint)AID.ExtrasensoryExpulsionNorthSouth or (uint)AID.ExtrasensoryExpulsionWestEast)
            Sourcez.Clear();
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = Sourcez.Count;
        if (Sourcez.Count != 0)
        {
            var forbidden = new List<Func<WPos, float>>(2);
            for (var i = 0; i < count; ++i)
            {
                var recti = Sourcez[i];
                if (recti.Shape is AOEShapeRect rect && rect == RectNS)
                    forbidden.Add(ShapeDistance.InvertedRect(recti.Origin, recti.Direction, 19f, default, 9f));
            }
            hints.AddForbiddenZone(ShapeDistance.Intersection(forbidden), Sourcez[0].Activation);
        }
    }
}

class VoltaicSlash(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.VoltaicSlash));

class OverwhelmingCharge(BossModule module) : Components.GenericAOEs(module)
{
    private readonly ExtrasensoryExpulsion _kb = module.FindComponent<ExtrasensoryExpulsion>()!;
    private const string Hint = "Wait inside safespot for knockback!";
    private static readonly AOEShapeCone cone = new(26f, 90f.Degrees());
    private static readonly AOEShapeRect rectAdj = new(19f, 7f); // the knockback rectangles are placed poorly with significant error for from visuals plus half height of the arena is smaller than 20 knockback distance

    public AOEInstance? AOE;
    private static readonly Angle a180 = 180f.Degrees();

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var componentActive = _kb.Sourcez.Count != 0 || actor.PendingKnockbacks.Count != 0;
        var aoes = new List<AOEInstance>();
        if (AOE is AOEInstance aoe)
        {
            if (componentActive)
            {
                Components.Knockback.Source? safezone = null;

                var count = _kb.Sourcez.Count;
                for (var i = 0; i < count; ++i)
                {
                    var source = _kb.Sourcez[i];
                    if (aoe.Rotation.AlmostEqual(source.Direction + a180, Angle.DegToRad))
                    {
                        safezone = source;
                        break;
                    }
                }
                if (safezone is Components.Knockback.Source sz)
                    aoes.Add(new(rectAdj, sz.Origin, sz.Direction, sz.Activation, Colors.SafeFromAOE, false));
            }
            else
                aoes.Add(aoe);
        }
        else if (componentActive)
        {
            var count = _kb.Sourcez.Count;
            for (var i = 0; i < count; ++i)
            {
                var recti = _kb.Sourcez[i];
                if (recti.Shape is AOEShapeRect rect && rect == ExtrasensoryExpulsion.RectNS)
                    aoes.Add(new(rectAdj, recti.Origin, recti.Direction, recti.Activation, Colors.SafeFromAOE, false));
            }
        }
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.OverwhelmingCharge1 or (uint)AID.OverwhelmingCharge2)
            AOE = new(cone, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.OverwhelmingCharge1 or (uint)AID.OverwhelmingCharge2)
            AOE = default;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var component = _kb.Sourcez.Count != 0;
        if (component && AOE is AOEInstance aoe)
        {
            hints.AddForbiddenZone(aoe.Shape, aoe.Origin, aoe.Rotation + a180, aoe.Activation);
        }
        else
            base.AddAIHints(slot, actor, assignment, hints);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);

        List<AOEInstance> activeSafespots = new(2);
        foreach (var aoe in ActiveAOEs(slot, actor))
        {
            if (aoe.Shape == rectAdj)
            {
                activeSafespots.Add(aoe);
            }
        }
        var count = activeSafespots.Count;
        if (count != 0)
        {
            var actorInSafespot = false;
            for (var i = 0; i < count; ++i)
            {
                if (activeSafespots[i].Check(actor.Position))
                {
                    actorInSafespot = true;
                    break;
                }
            }
            hints.Add(Hint, !actorInSafespot);
        }
    }
}

class Electrolance(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Electrolance), 22f);
class WhorlOfTheMind(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.WhorlOfTheMind), 5f);

class Rush(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect rect = new(33f, 5f);
    private readonly List<AOEInstance> _aoes = new(7);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var aoes = new AOEInstance[count];
        for (var i = 0; i < count; ++i)
        {
            var aoe = _aoes[i];
            aoes[i] = i == 0 ? count > 1 ? aoe with { Color = Colors.Danger } : aoe : aoe;
        }
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.RushTelegraph)
        {
            var activation = Module.CastFinishAt(spell, 6.8f);
            var dir = spell.LocXZ - caster.Position;

            if (_aoes.Count < 7)
                _aoes.Add(new(new AOEShapeRect(dir.Length(), 5f), caster.Position, Angle.FromDirection(dir), activation));
            else
                _aoes.Add(new(rect, new(190f, 19.5f), -180f.Degrees(), activation));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID is (uint)AID.Rush or (uint)AID.ElectrolanceAssimilation)
            _aoes.RemoveAt(0);
    }
}

class D053AmbroseStates : StateMachineBuilder
{
    public D053AmbroseStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<PsychicWaveArenaChange>()
            .ActivateOnEnter<PsychicWave>()
            .ActivateOnEnter<Psychokinesis>()
            .ActivateOnEnter<ExtrasensoryExpulsion>()
            .ActivateOnEnter<OverwhelmingCharge>()
            .ActivateOnEnter<VoltaicSlash>()
            .ActivateOnEnter<Electrolance>()
            .ActivateOnEnter<Rush>()
            .ActivateOnEnter<WhorlOfTheMind>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 825, NameID = 12695, SortOrder = 4)]
public class D053Ambrose(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, StartingBounds)
{
    public static readonly WPos ArenaCenter = new(190f, default);
    public static readonly ArenaBoundsRect StartingBounds = new(32.5f, 24f);
    public static readonly ArenaBoundsRect DefaultBounds = new(15f, 19.5f);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies([(uint)OID.Superfluity, (uint)OID.OrigenicsEyeborg]));
    }
}
