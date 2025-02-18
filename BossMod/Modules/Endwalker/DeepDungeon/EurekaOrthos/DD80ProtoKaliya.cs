namespace BossMod.Endwalker.DeepDungeon.EurekaOrthos.DD80ProtoKaliya;

public enum OID : uint
{
    Boss = 0x3D18, // R5.0
    WeaponsDrone = 0x3D19, // R2.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 31421, // Boss->players, no cast, range 6+R 90-degree cone
    AetheromagnetismKB = 31431, // Helper->player, no cast, single-target, knockback 10, away from source
    AetheromagnetismPull = 31430, // Helper->player, no cast, single-target, pull 10, between centers
    AutoCannons = 31432, // WeaponsDrone->self, 4.0s cast, range 41+R width 5 rect
    Barofield = 31427, // Boss->self, 3.0s cast, single-target

    CentralizedNerveGasVisual = 31423, // Boss->self, 4.5s cast, range 25+R 120-degree cone
    CentralizedNerveGas = 32933, // Helper->self, 5.3s cast, range 25+R 120-degree cone
    LeftwardNerveGasVisual = 31424, // Boss->self, 4.5s cast, range 25+R 180-degree cone
    LeftwardNerveGas = 32934, // Helper->self, 5.3s cast, range 25+R 180-degree cone
    RightwardNerveGasVisual = 31425, // Boss->self, 4.5s cast, range 25+R 180-degree cone
    RightwardNerveGas = 32935, // Helper->self, 5.3s cast, range 25+R 180-degree cone
    NerveGasRingVisual = 31426, // Boss->self, 5.0s cast, range 8-30 donut
    NerveGasRing = 32930, // Helper->self, 7.2s cast, range 8-30 donut
    Resonance = 31422, // Boss->player, 5.0s cast, range 12 90-degree cone, tankbuster

    NanosporeJet = 31429 // Boss->self, 5.0s cast, range 100 circle
}

public enum SID : uint
{
    Barofield = 3420, // none->Boss, extra=0x0, damage taken when near boss
    NegativeChargePlayer = 3419, // none->player, extra=0x0
    PositiveChargePlayer = 3418, // none->player, extra=0x0
    NegativeChargeDrone = 3417, // none->WeaponsDrone, extra=0x0
    PositiveChargeDrone = 3416 // none->WeaponsDrone, extra=0x0
}

public enum TetherID : uint
{
    Magnetism = 38
}

class Magnetism(BossModule module) : Components.Knockback(module, ignoreImmunes: true)
{
    private readonly HashSet<(Actor, uint)> statusOnActor = [];
    public readonly HashSet<(Actor, Source)> sourceByActor = [];
    private readonly NerveGasRingAndAutoCannons _aoe1 = module.FindComponent<NerveGasRingAndAutoCannons>()!;
    private readonly Barofield _aoe2 = module.FindComponent<Barofield>()!;

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        if (sourceByActor.Count != 0 && _aoe1.AOEs.Any(z => z.Shape == NerveGasRingAndAutoCannons.donut))
        {
            var x = sourceByActor.FirstOrDefault(x => x.Item1 == actor);
            return [new(x.Item2.Origin, x.Item2.Distance, x.Item2.Activation, Kind: x.Item2.Kind)];
        }
        else
            return [];
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        var count = _aoe1.AOEs.Count;
        for (var i = 0; i < count; ++i)
        {
            var aoe = _aoe1.AOEs[i];
            if (aoe.Check(pos))
                return true;
        }

        return _aoe2.AOE != null && _aoe2.AOE.Value.Check(pos) || !Module.InBounds(pos);
    }

    private bool IsPull(Actor source, Actor target)
    {
        return statusOnActor.Contains((source, (uint)SID.NegativeChargeDrone)) && statusOnActor.Contains((target, (uint)SID.PositiveChargePlayer)) ||
            statusOnActor.Contains((source, (uint)SID.PositiveChargeDrone)) && statusOnActor.Contains((target, (uint)SID.NegativeChargePlayer));
    }

    private bool IsKnockback(Actor source, Actor target)
    {
        return statusOnActor.Contains((source, (uint)SID.NegativeChargeDrone)) && statusOnActor.Contains((target, (uint)SID.NegativeChargePlayer)) ||
            statusOnActor.Contains((source, (uint)SID.PositiveChargeDrone)) && statusOnActor.Contains((target, (uint)SID.PositiveChargePlayer));
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.Magnetism)
        {
            var target = WorldState.Actors.Find(tether.Target)!;
            var activation = WorldState.FutureTime(10d);
            if (IsPull(source, target))
                sourceByActor.Add((target, new(source.Position, 10f, activation, Kind: Kind.TowardsOrigin)));
            else if (IsKnockback(source, target))
                sourceByActor.Add((target, new(source.Position, 10f, activation)));
        }
    }

    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.Magnetism)
        {
            var target = WorldState.Actors.Find(tether.Target)!;
            sourceByActor.RemoveWhere(x => x.Item1 == target);
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID is (uint)SID.NegativeChargeDrone or (uint)SID.PositiveChargeDrone or (uint)SID.PositiveChargePlayer or (uint)SID.NegativeChargePlayer)
        {
            statusOnActor.RemoveWhere(x => x.Item1 == actor);
            statusOnActor.Add((actor, status.ID));
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (sourceByActor.Count > 0)
        {
            var target = sourceByActor.FirstOrDefault(x => x.Item1 == actor);
            var offset = target.Item2.Kind == Kind.TowardsOrigin ? 180f.Degrees() : default;
            hints.AddForbiddenZone(ShapeDistance.InvertedDonutSector(Arena.Center, 15f, 18f, Angle.FromDirection(target.Item2.Origin - Arena.Center) + offset, 35f.Degrees()));
        }
    }
}

class Barofield(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(5f);
    public AOEInstance? AOE;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(AOE);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (AOE == null && spell.Action.ID is (uint)AID.Barofield or (uint)AID.NanosporeJet)
            AOE = new(circle, spell.LocXZ, default, Module.CastFinishAt(spell, 0.7f));
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.Barofield)
            AOE = null;
    }
}

class NerveGasRingAndAutoCannons(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> AOEs = new(2);
    private static readonly AOEShapeCross cross = new(43f, 2.5f);
    public static readonly AOEShapeDonut donut = new(8f, 30f);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => AOEs;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        void AddAOE(AOEShape shape, bool risky = true) => AOEs.Add(new(shape, spell.LocXZ, default, Module.CastFinishAt(spell), Risky: risky));
        if (spell.Action.ID == (uint)AID.NerveGasRing)
        {
            AddAOE(donut, false);
            AddAOE(cross);
        }
        else if (spell.Action.ID == (uint)AID.AutoCannons)
            AddAOE(cross);
        else if (AOEs.Count == 2 && spell.Action.ID == (uint)AID.AutoCannons)
        {
            AOEs.RemoveAt(1);
            AddAOE(cross);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (AOEs.Count != 0 && spell.Action.ID is (uint)AID.NerveGasRing or (uint)AID.AutoCannons)
            AOEs.RemoveAt(0);
    }
}

class NerveGas(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), new AOEShapeCone(30f, 90f.Degrees()));
class LeftNerveGas(BossModule module) : NerveGas(module, AID.LeftwardNerveGas);
class RightNerveGas(BossModule module) : NerveGas(module, AID.RightwardNerveGas);

class CentralizedNerveGas(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.CentralizedNerveGas), new AOEShapeCone(30f, 60f.Degrees()));

class AutoAttack(BossModule module) : Components.Cleave(module, ActionID.MakeSpell(AID.AutoAttack), new AOEShapeCone(11f, 45f.Degrees()))
{
    private readonly Barofield _aoe = module.FindComponent<Barofield>()!;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_aoe.AOE != null)
            base.AddAIHints(slot, actor, assignment, hints);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_aoe.AOE != null)
            base.DrawArenaForeground(pcSlot, pc);
    }
}

class Resonance(BossModule module) : Components.BaitAwayCast(module, ActionID.MakeSpell(AID.Resonance), new AOEShapeCone(12f, 45f.Degrees()), endsOnCastEvent: true)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        if (CurrentBaits.Count != 0)
            hints.Add("Tankbuster cleave");
    }
}

class DD80ProtoKaliyaStates : StateMachineBuilder
{
    public DD80ProtoKaliyaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Barofield>()
            .ActivateOnEnter<AutoAttack>()
            .ActivateOnEnter<Resonance>()
            .ActivateOnEnter<NerveGasRingAndAutoCannons>()
            .ActivateOnEnter<LeftNerveGas>()
            .ActivateOnEnter<RightNerveGas>()
            .ActivateOnEnter<CentralizedNerveGas>()
            .ActivateOnEnter<Magnetism>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus, legendoficeman", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 904, NameID = 12247)]
public class DD80ProtoKaliya(WorldState ws, Actor primary) : BossModule(ws, primary, new(-600f, -300f), new ArenaBoundsCircle(20f));