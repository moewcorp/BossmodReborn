﻿namespace BossMod.Shadowbringers.Foray.CriticalEngagement.CE44FamiliarFace;

public enum OID : uint
{
    Boss = 0x2DD2, // R9.450, x1
    PhantomHashmal = 0x3321, // R9.450, x1
    ArenaFeatures = 0x1EA1A1, // R2.000, x9, EventObj type
    Tower = 0x1EB17E, // R0.500, EventObj type, spawn during fight
    FallingTower = 0x1EB17D, // R0.500, EventObj type, spawn during fight, rotation at spawn determines fall direction?..
    Hammer = 0x1EB17F, // R0.500, EventObj type, spawn during fight
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 6497, // Boss->player, no cast, single-target
    TectonicEruption = 23826, // Helper->location, 4.0s cast, range 6 circle puddle
    RockCutter = 23827, // Boss->player, 5.0s cast, single-target, tankbuster
    AncientQuake = 23828, // Boss->self, 5.0s cast, single-target, visual
    AncientQuakeAOE = 23829, // Helper->self, no cast, ??? (x2 ~0.8s after visual)
    Sanction = 23817, // Boss->self, no cast, single-target, visual (light raidwide)
    SanctionAOE = 23832, // Helper->self, no cast, ??? (x2 ~0.8s after visual)
    Roxxor = 23823, // Helper->players, 5.0s cast, range 6 circle spread

    ControlTowerAppear = 23830, // Helper->self, 4.0s cast, range 6 circle aoe around appearing towers
    TowerRound = 23831, // Boss->self, 4.0s cast, single-target, visual (spawns 2 towers + light raidwide)
    TowerRoundAOE = 23834, // Helper->self, no cast, ??? (x2 ~0.8s after visual)
    ControlTower = 23816, // Boss->self, 4.0s cast, single-target, visual (spawns 3 towers + light raidwide)
    ControlTowerAOE = 23833, // Helper->self, no cast, ??? (x2 ~0.8s after visual)
    Towerfall = 23818, // Helper->self, 7.0s cast, range 40 width 10 rect aoe

    PhantomOrder = 24702, // Boss->self, 4.0s cast, single-target, visual
    ExtremeEdgeR = 23821, // PhantomHashmal->self, 8.0s cast, range 60 width 36 rect aoe offset to the right
    ExtremeEdgeL = 23822, // PhantomHashmal->self, 8.0s cast, range 60 width 36 rect aoe offset to the left

    IntractableLand = 24576, // Boss->self, 5.0s cast, single-target, visual (double exaflares)
    IntractableLandFirst = 23819, // Helper->self, 5.3s cast, range 8 circle
    IntractableLandRest = 23820, // Helper->location, no cast, range 8 circle

    HammerRound = 23824, // Boss->self, 5.0s cast, single-target, visual
    Hammerfall = 23825 // Helper->self, 8.0s cast, range 37 circle aoe
}

class TectonicEruption(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.TectonicEruption), 6f);
class RockCutter(BossModule module) : Components.SingleTargetDelayableCast(module, ActionID.MakeSpell(AID.RockCutter));
class AncientQuake(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.AncientQuake));
class Roxxor(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.Roxxor), 6f);
class ControlTowerAppear(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.ControlTowerAppear), 6f);

// note: we could predict aoes way in advance, when FallingTower actors are created - they immediately have correct rotation
// if previous cast was TowerRound, delay is ~24.4s; otherwise if previous cast was ControlTower, delay is ~9.6s; otherwise it is ~13s
// however, just watching casts normally gives more than enough time to avoid aoes and does not interfere with mechanics that resolve earlier
class Towerfall(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Towerfall), new AOEShapeRect(40f, 5f));

abstract class ExtremeEdge(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), new AOEShapeRect(60f, 18f));
class ExtremeEdgeL(BossModule module) : ExtremeEdge(module, AID.ExtremeEdgeL);
class ExtremeEdgeR(BossModule module) : ExtremeEdge(module, AID.ExtremeEdgeR);

class IntractableLand(BossModule module) : Components.Exaflare(module, 8f)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.IntractableLandFirst)
        {
            Lines.Add(new() { Next = caster.Position, Advance = 8f * spell.Rotation.ToDirection(), NextExplosion = Module.CastFinishAt(spell), TimeToMove = 0.8f, ExplosionsLeft = 8, MaxShownExplosions = 4 });
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.IntractableLandFirst or (uint)AID.IntractableLandRest)
        {
            var count = Lines.Count;
            var pos = spell.Action.ID == (uint)AID.IntractableLandFirst ? caster.Position : spell.TargetXZ;
            for (var i = 0; i < count; ++i)
            {
                var line = Lines[i];
                if (line.Next.AlmostEqual(pos, 1f))
                {
                    AdvanceLine(line, pos);
                    if (line.ExplosionsLeft == 0)
                        Lines.RemoveAt(i);
                    return;
                }
            }
            ReportError($"Failed to find entry for {caster.InstanceID:X}");
        }
    }
}

class Hammerfall(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(3);
    private static readonly AOEShapeCircle _shape = new(37);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var max = count > 2 ? 2 : count;
        var aoes = new AOEInstance[max];
        for (var i = 0; i < max; ++i)
        {
            aoes[i] = _aoes[i];
        }
        return aoes;
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.Hammer)
            _aoes.Add(new(_shape, actor.Position, default, WorldState.FutureTime(12.6d)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID == (uint)AID.Hammerfall)
            _aoes.RemoveAt(0);
    }
}

class CE44FamiliarFaceStates : StateMachineBuilder
{
    public CE44FamiliarFaceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<TectonicEruption>()
            .ActivateOnEnter<RockCutter>()
            .ActivateOnEnter<AncientQuake>()
            .ActivateOnEnter<Roxxor>()
            .ActivateOnEnter<ControlTowerAppear>()
            .ActivateOnEnter<Towerfall>()
            .ActivateOnEnter<ExtremeEdgeL>()
            .ActivateOnEnter<ExtremeEdgeR>()
            .ActivateOnEnter<IntractableLand>()
            .ActivateOnEnter<Hammerfall>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.BozjaCE, GroupID = 778, NameID = 29)] // bnpcname=9693
public class CE44FamiliarFace(WorldState ws, Actor primary) : BossModule(ws, primary, new(330f, 390f), new ArenaBoundsCircle(30f));
