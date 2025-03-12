namespace BossMod.Shadowbringers.Foray.CriticalEngagement.CE64FeelingTheBurn;

public enum OID : uint
{
    Boss = 0x31A0, // R3.640, x1
    Escort1 = 0x31A1, // R2.800, x24
    Escort2 = 0x32FD, // R2.800, spawn during fight
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttackBoss = 24281, // Boss->player, no cast, single-target
    AutoAttackEscort = 24319, // Escort2->player, no cast, single-target

    ReadOrdersCoordinatedAssault = 23604, // Boss->self, 19.0s cast, single-target, visual
    DiveFormation = 23606, // Escort1->self, 5.0s cast, range 60 width 6 rect aoe
    AntiPersonnelMissile1 = 23609, // Boss->self, 10.0s cast, single-target, visual (3 impact pairs)
    AntiPersonnelMissile2 = 23618, // Boss->self, 12.0s cast, single-target, visual (4 impact pairs)
    BallisticImpact = 23610, // Helper->self, no cast, range 24 width 24 rect aoe
    ReadOrdersShotsFired = 23611, // Boss->self, 3.0s cast, single-target, visual
    ChainCannonEscort = 23612, // Escort2->self, 1.0s cast, range 60 width 5 rect visual
    ChainCannonEscortAOE = 23613, // Helper->self, no cast, range 60 width 5 rect 'voidzone'
    ChainCannonBoss = 24658, // Boss->self, 3.0s cast, range 60 width 6 rect, visual
    ChainCannonBossAOE = 24659, // Helper->self, no cast, range 60 width 6 rect
    SurfaceMissile = 23614, // Boss->self, 3.0s cast, single-target, visual
    SurfaceMissileAOE = 23615, // Helper->location, 3.0s cast, range 6 circle puddle
    SuppressiveMagitekRays = 23616, // Boss->self, 5.0s cast, single-target, visual
    SuppressiveMagitekRaysAOE = 23617, // Helper->self, 5.5s cast, ???, raidwide
    Analysis = 23607, // Boss->self, 3.0s cast, single-target, visual
    PreciseStrike = 23619 // Escort1->self, 5.0s cast, range 60 width 6 rect aoe (should orient correctly to avoid vuln)
}

public enum SID : uint
{
    Tracking = 2056, // none->Escort2, extra=0x87
    FrontUnseen = 2644, // Boss->player, extra=0x120
    BackUnseen = 1709, // Boss->player, extra=0xE8
}

public enum IconID : uint
{
    BallisticImpact = 261, // Helper
}

class DiveFormation(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.DiveFormation), new AOEShapeRect(60f, 3f));

class AntiPersonnelMissile(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.BallisticImpact))
{
    private readonly List<AOEInstance> _aoes = new(8);
    private static readonly AOEShapeRect _shape = new(12f, 12f, 12f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var max = count > 2 ? 2 : count;
        return CollectionsMarshal.AsSpan(_aoes)[..max];
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        // TODO: activation time (icon pairs are ~3s apart, but explosion pairs are ~2.6s apart; first explosion is ~2.1s after visual cast end)
        if (iconID == (uint)IconID.BallisticImpact)
            _aoes.Add(new(_shape, actor.Position, default, WorldState.FutureTime(11d)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0 && spell.Action == WatchedAction)
            _aoes.RemoveAt(0);
    }
}

class ChainCannonEscort(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<(Actor caster, int numCasts, DateTime activation)> _casters = [];
    private static readonly AOEShapeRect _shape = new(60f, 2.5f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var countC = _casters.Count;
        if (countC == 0)
            return [];
        var count = 0;
        for (var i = 0; i < countC; ++i)
        {
            var c = _casters[i];
            if (!IsTrackingPlayer(c, actor))
                ++count;
        }
        if (count == 0)
            return [];

        var aoes = new AOEInstance[count];
        var index = 0;

        for (var i = 0; i < countC; ++i)
        {
            var c = _casters[i];
            if (!IsTrackingPlayer(c, actor))
                aoes[index++] = new(_shape, c.caster.Position, c.caster.Rotation, c.activation);
        }
        return aoes;
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var count = _casters.Count;
        if (count == 0)
            return;
        for (var i = 0; i < count; ++i)
        {
            var c = _casters[i];
            if (IsTrackingPlayer(c, pc))
                _shape.Outline(Arena, c.caster);
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.Tracking)
            _casters.Add((actor, 0, status.ExpireAt));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.ChainCannonEscortAOE)
        {
            var count = _casters.Count;
            var pos = caster.Position;
            for (var i = count - 1; i <= 0; ++i)
            {
                var c = _casters[i];
                if (c.caster.Position.AlmostEqual(pos, 1f))
                {
                    var numCasts = c.numCasts + 1;
                    if (numCasts >= 6)
                        _casters.RemoveAt(i);
                    else
                        _casters[i] = (c.caster, numCasts, WorldState.FutureTime(1d));
                    return;
                }
            }
        }
    }

    private bool IsTrackingPlayer((Actor caster, int numCasts, DateTime activation) c, Actor actor) => c.numCasts == 0 && c.caster.CastInfo == null && c.caster.TargetID == actor.InstanceID;
}

class ChainCannonBoss(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _instance;
    private static readonly AOEShapeRect _shape = new(60f, 2.5f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(ref _instance);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ChainCannonBoss)
            _instance = new(_shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell, 1f));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.ChainCannonBossAOE)
        {
            if (++NumCasts >= 4)
            {
                _instance = null;
                NumCasts = 0;
            }
            else
            {
                _instance = new(_shape, WPos.ClampToGrid(caster.Position), caster.Rotation, WorldState.FutureTime(1d));
            }
        }
    }
}

class SurfaceMissile(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.SurfaceMissileAOE), 6f);
class SuppressiveMagitekRays(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.SuppressiveMagitekRays));
class Analysis(BossModule module) : Components.CastHint(module, ActionID.MakeSpell(AID.Analysis), "Face open weakpoint to charging adds");
class PreciseStrike(BossModule module) : Components.CastWeakpoint(module, ActionID.MakeSpell(AID.PreciseStrike), new AOEShapeRect(60f, 3f), (uint)SID.FrontUnseen, (uint)SID.BackUnseen, 0, 0);

class CE64FeelingTheBurnStates : StateMachineBuilder
{
    public CE64FeelingTheBurnStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DiveFormation>()
            .ActivateOnEnter<AntiPersonnelMissile>()
            .ActivateOnEnter<ChainCannonEscort>()
            .ActivateOnEnter<ChainCannonBoss>()
            .ActivateOnEnter<SurfaceMissile>()
            .ActivateOnEnter<SuppressiveMagitekRays>()
            .ActivateOnEnter<Analysis>()
            .ActivateOnEnter<PreciseStrike>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.BozjaCE, GroupID = 778, NameID = 18)] // bnpcname=9945
public class CE64FeelingTheBurn(WorldState ws, Actor primary) : BossModule(ws, primary, new(-240f, -230f), new ArenaBoundsSquare(24f))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        base.DrawEnemies(pcSlot, pc);
        Arena.Actors(Enemies((uint)OID.Escort2));
    }
}
