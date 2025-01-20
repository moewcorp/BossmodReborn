﻿using BossMod.QuestBattle.Endwalker.MSQ;

namespace BossMod.Endwalker.Quest.AsTheHeavensBurn.P1TerminusIdolizer;

public enum OID : uint
{
    Boss = 0x35E9, // R4.95
    TerminusDetonator = 0x35E5, // R2.0
    Helper = 0x233C
}

public enum AID : uint
{

    AutoAttack = 26994, // Boss->Estinien, no cast, single-target
    DeadlyTentacles = 26998, // Boss->Estinien, no cast, single-target
    TentacleWhip = 27005, // Boss->self, no cast, single-target
    Shout = 27000, // Boss->self, no cast, single-target
    Whack1 = 27008, // Boss->Estinien, no cast, single-target
    Whack2 = 27009, // Boss->Estinien, no cast, single-target

    DeadlyCharge = 26995, // Boss->location, 5.0s cast, width 10 rect charge
    GriefOfParting = 26996, // Boss->self, 5.0s cast, range 40 circle
    DeadlyTentaclesTB = 26997, // Boss->Estinien, 5.0s cast, single-target
    TentacleWhipLFirst = 27004, // Boss->self, 5.0s cast, range 60 180-degree cone
    TentacleWhipRSecond = 27006, // Helper->self, 7.0s cast, range 60 180-degree cone
    SelfDestruct = 26991, // TerminusDetonator->self, no cast, range 6 circle
    Petrifaction = 26999, // Boss->self, 4.0s cast, range 60 circle
    TentacleWhipRFirst = 27001, // Boss->self, 5.0s cast, range 60 180-degree cone
    TentacleWhipLSecond = 27003, // Helper->self, 7.0s cast, range 60 180-degree cone
    Whack = 27007, // Boss->35F5, 5.0s cast, single-target
}

public enum IconID : uint
{
    Stack = 218 // Estinien
}

public enum TetherID : uint
{
    BombTether = 17 // TerminusDetonator->Estinien
}

class DeadlyCharge(BossModule module) : Components.ChargeAOEs(module, ActionID.MakeSpell(AID.DeadlyCharge), 5);
class GriefOfParting(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.GriefOfParting));
class DeadlyTentaclesTB(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.DeadlyTentaclesTB));

class TentacleWhip(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(2);
    private static readonly HashSet<AID> casts = [AID.TentacleWhipRFirst, AID.TentacleWhipLSecond, AID.TentacleWhipLFirst, AID.TentacleWhipLSecond];
    private static readonly AOEShapeCone cone = new(60, 90.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        List<AOEInstance> aoes = new(count);
        for (var i = 0; i < count; ++i)
        {
            var aoe = _aoes[i];
            if (i == 0)
                aoes.Add(count > 1 ? aoe with { Color = Colors.Danger } : aoe);
            else
                aoes.Add(aoe with { Risky = false });
        }
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (casts.Contains((AID)spell.Action.ID))
        {
            _aoes.Add(new(cone, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
            if (_aoes.Count == 2)
                _aoes.SortBy(aoe => aoe.Activation);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && casts.Contains((AID)spell.Action.ID))
            _aoes.RemoveAt(0);
    }
}

class SelfDestruct(BossModule module) : Components.GenericStackSpread(module)
{
    private uint numCasts;

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (Stacks.Count == 0 && tether.ID == (uint)TetherID.BombTether)
            Stacks.Add(new(WorldState.Actors.Find(tether.Target)!, 3, 2, 2, activation: WorldState.FutureTime(9.1f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.SelfDestruct)
        {
            if (++numCasts == 6)
                Stacks.Clear();
        }
    }
}
class Petrifaction(BossModule module) : Components.CastGaze(module, ActionID.MakeSpell(AID.Petrifaction));
class Whack(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.Whack));

class AutoAlphinaud(BossModule module) : QuestBattle.RotationModule<AlphinaudAI>(module);

class TerminusIdolizerStates : StateMachineBuilder
{
    public TerminusIdolizerStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DeadlyCharge>()
            .ActivateOnEnter<GriefOfParting>()
            .ActivateOnEnter<TentacleWhip>()
            .ActivateOnEnter<DeadlyTentaclesTB>()
            .ActivateOnEnter<SelfDestruct>()
            .ActivateOnEnter<Petrifaction>()
            .ActivateOnEnter<Whack>()
            .ActivateOnEnter<AutoAlphinaud>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 804, NameID = 10932)]
public class TerminusIdolizer(WorldState ws, Actor primary) : BossModule(ws, primary, new(-300.75f, 151.5f), new ArenaBoundsCircle(19.5f));
