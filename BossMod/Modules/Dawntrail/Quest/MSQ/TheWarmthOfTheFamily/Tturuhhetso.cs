namespace BossMod.Dawntrail.Quest.MSQ.TheWarmthOfTheFamily.Tturuhhetso;

public enum OID : uint
{

    Boss = 0x4638, // R5.95
    ScaleArmoredLeg = 0x4648, // R5.95
    BallOfFire = 0x4639, // R1.0
    Koana = 0x4630, // R0.5
    WukLamat = 0x4631, // R0.5
    Orbs = 0x463A, // R1.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target

    Ensnare = 40566, // Boss->location, 5.0s cast, range 6 circle
    Regenerate = 40583, // Boss->self, 4.0s cast, single-target

    SearingSwellVisual1 = 40577, // Boss->self, 4.0s cast, single-target
    SearingSwellVisual2 = 40578, // Boss->self, no cast, single-target
    SearingSwell = 40579, // Helper->self, 4.0s cast, range 40 45-degree cone

    FlameBlast = 40580, // BallOfFire->self, 2.0s cast, range 40 width 4 rect

    TriceraSnareVisual = 40573, // Boss->self, 5.0s cast, single-target, spread
    TriceraSnare = 40574, // Boss->player/WukLamat/Koana, no cast, range 6 circle

    PrimordialRoar1 = 41271, // Boss->self, 5.0s cast, range 40 circle
    PrimordialRoar2 = 40584, // Boss->self, 5.0s cast, range 40 circle

    Pull = 40664, // Helper->Boss, 1.2s cast, single-target, pull 30 between centers, after leg got destroyed

    RazingRoarVisual = 40586, // Boss->self, no cast, single-target
    RazingRoarPlayer = 41245, // Helper->player, no cast, single-target, knockback 30, dir forward
    RazingRoarWukLamat = 41246, // Helper->WukLamat, 0.5s cast, single-target, knockback 13, dir forward
    RazingRoarKoana = 41247, // Helper->Koana, 0.5s cast, single-target, down for the count

    ModelStateChange = 40570, // Boss->self, no cast, single-target

    FirestormVisual = 40575, // Boss->self, 5.0s cast, single-target
    Firestorm = 40576, // Helper->self, 5.0s cast, range 10 circle

    CandescentRayTB = 40571, // Boss->self/players, 8.0s cast, range 50 width 8 rect, shared tankbuster, only happens if playing duty with tank
    CandescentRayMarker = 40569, // Helper->player, no cast, single-target
    CandescentRayLineStack = 40572, // Boss->self/players, 8.0s cast, range 50 width 8 rect, shared tankbuster, only happens if not playing duty with tank

    Aethercall = 40581, // Boss->self, 5.0s cast, single-target
    CollectOrb = 40582, // Orbs->player/Koana/WukLamat, no cast, single-target
    BossOrb = 41117 // Orbs->Boss, no cast, single-target
}

public enum IconID : uint
{
    Spreadmarker = 140 // WukLamat/Koana/player->self
}

class CandescentRayLineStack(BossModule module) : Components.LineStack(module, null, ActionID.MakeSpell(AID.CandescentRayLineStack), minStackSize: 3, maxStackSize: 3);
class CandescentRayTB(BossModule module) : Components.CastSharedTankbuster(module, ActionID.MakeSpell(AID.CandescentRayTB), new AOEShapeRect(50, 4))
{
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Target == null)
            return;
        hints.Add("Stack with Wuk Lamat!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Target == null)
            return;
        var koana = Module.Enemies(OID.Koana).FirstOrDefault();
        var wuk = Module.Enemies(OID.WukLamat).FirstOrDefault();
        var primary = Module.PrimaryActor;
        if (koana != null)
            hints.AddForbiddenZone(ShapeDistance.Cone(primary.Position, 100, primary.AngleTo(koana), Angle.Asin(4 / (koana.Position - primary.Position).Length())), Activation);
        if (wuk != null)
            hints.AddForbiddenZone(ShapeDistance.InvertedRect(primary.Position, primary.AngleTo(wuk), 50, 0, 4), Activation);
    }
}

class SearingSwell(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.SearingSwell), new AOEShapeCone(40, 22.5f.Degrees()));
class Ensnare(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Ensnare), 6);
class TriceraSnare(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.Spreadmarker, ActionID.MakeSpell(AID.TriceraSnare), 6, 4.7f)
{
    public override void OnEventDirectorUpdate(uint updateID, uint param1, uint param2, uint param3, uint param4)
    {
        // on phase change all pending spreads get cancelled
        Spreads.Clear();
    }
}

class PrimordialRoar1(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.PrimordialRoar1));
class PrimordialRoar2(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.PrimordialRoar2));

class OrbCollecting(BossModule module) : BossComponent(module)
{
    private readonly IReadOnlyList<Actor> _orbs = module.Enemies(OID.Orbs);

    private IEnumerable<Actor> ActiveOrbs => _orbs.Where(x => x.Tether.ID != 0);

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (ActiveOrbs.Any())
            hints.Add("Soak the orbs!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var orbs = new List<Func<WPos, float>>();
        if (ActiveOrbs.Any())
            foreach (var o in ActiveOrbs)
                orbs.Add(ShapeDistance.InvertedCircle(o.Position + 0.5f * o.Rotation.ToDirection(), 0.75f));
        if (orbs.Count > 0)
            hints.AddForbiddenZone(p => orbs.Max(f => f(p)));
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var orb in ActiveOrbs)
            Arena.AddCircle(orb.Position, 1.4f, Colors.Safe);
    }
}

class FlameBlast(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect rect = new(20, 2, 20);
    private readonly List<AOEInstance> _aoes = [];
    private static readonly Angle a90 = 90.Degrees();

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            yield break;
        var compare = count > 5 && _aoes[0].Rotation.AlmostEqual(_aoes[5].Rotation, Angle.DegToRad);

        for (var i = 0; i < count; ++i)
        {
            var aoe = _aoes[i];
            if (i < 5)
                yield return count > 5 ? aoe with { Color = Colors.Danger } : aoe;
            else if (i is > 4 and < 10)
                yield return compare ? aoe with { Risky = false } : aoe;
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.BallOfFire)
            _aoes.Add(new(rect, actor.Position, actor.Rotation + a90, WorldState.FutureTime(6.7f)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && (AID)spell.Action.ID == AID.FlameBlast)
            _aoes.RemoveAt(0);
    }
}

class Firestorm(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeCircle circle = new(10);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count > 0)
            for (var i = 0; i < count; ++i)
            {
                var aoe = _aoes[i];
                if ((aoe.Activation - _aoes[0].Activation).TotalSeconds <= 1)
                    yield return aoe;
            }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Firestorm)
            _aoes.Add(new(circle, caster.Position, spell.Rotation, Module.CastFinishAt(spell)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && (AID)spell.Action.ID == AID.Firestorm)
            _aoes.RemoveAt(0);
    }
}

class TturuhhetsoStates : StateMachineBuilder
{
    public TturuhhetsoStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<CandescentRayLineStack>()
            .ActivateOnEnter<CandescentRayTB>()
            .ActivateOnEnter<FlameBlast>()
            .ActivateOnEnter<Ensnare>()
            .ActivateOnEnter<TriceraSnare>()
            .ActivateOnEnter<PrimordialRoar1>()
            .ActivateOnEnter<PrimordialRoar2>()
            .ActivateOnEnter<OrbCollecting>()
            .ActivateOnEnter<Firestorm>()
            .ActivateOnEnter<SearingSwell>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.Quest, GroupID = 70785, NameID = 13593)]
public class Tturuhhetso(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsComplex arena = new([new Polygon(new(395.735f, -45.365f), 19.5f, 20)]);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(WorldState.Actors.Where(x => !x.IsAlly));
    }
}
