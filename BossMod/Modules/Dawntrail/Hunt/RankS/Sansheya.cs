﻿namespace BossMod.Dawntrail.Hunt.RankS.Sansheya;

public enum OID : uint
{
    Boss = 0x43DD // R4.0
}

public enum AID : uint
{
    AutoAttack = 870, // Boss/4379->player, no cast, single-target

    CullingBlade = 39295, // Boss->self, 4.0s cast, range 80 circle
    PyreOfRebirth = 39288, // Boss->self, 4.0s cast, range 32 circle, status effect boiling, turns into pyretic
    FiresDomain = 39285, // Boss->players, 5.0s cast, width 6 rect charge
    TwinscorchedVeil1 = 39292, // Boss->self, 5.0s cast, range 40 180-degree cone, visual, right then left then circle
    TwinscorchedVeil2 = 39290, // Boss->self, 5.0s cast, range 40 180-degree cone, visual left then right then circle
    TwinscorchedHalo1 = 39289, // Boss->self, 5.0s cast, range 40 180-degree cone, visual, left then right then donut
    TwinscorchedHalo2 = 39291, // Boss->self, 5.0s cast, range 40 180-degree cone, visual, right then left then donut
    Twinscorch1 = 39601, // Boss->self, 4.0s cast, range 40 180-degree cone, left then right
    Twinscorch2 = 39602, // Boss->self, 4.0s cast, range 40 180-degree cone, right then left
    ScorchingLeft1 = 39528, // Boss->self, 1.0s cast, range 40 180-degree cone
    ScorchingLeft2 = 39557, // Boss->self, no cast, range 40 180-degree cone
    ScorchingRight1 = 39558, // Boss->self, no cast, range 40 180-degree cone
    ScorchingRight2 = 39529, // Boss->self, 1.0s cast, range 40 180-degree cone
    VeilOfHeat1 = 39283, // Boss->self, 5.0s cast, range 15 circle
    VeilOfHeat2 = 39293, // Boss->self, 1.0s cast, range 15 circle
    CaptiveBolt = 39296, // Boss->players, 5.0s cast, range 6 circle, stack
    HaloOfHeat1 = 39284, // Boss->self, 5.0s cast, range 10-40 donut
    HaloOfHeat2 = 39294 // Boss->self, 1.0s cast, range 10-40 donut
}

public enum SID : uint
{
    Boiling = 4140, // Boss->player, extra=0x0
    Pyretic = 960 // none->player, extra=0x0
}

class Boiling(BossModule module) : Components.StayMove(module)
{
    private BitMask _boiling;

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.Boiling && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
        {
            _boiling.Set(Raid.FindSlot(actor.InstanceID));
            PlayerStates[slot] = new(Requirement.Stay, status.ExpireAt);
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
        {
            if (status.ID == (uint)SID.Boiling)
                _boiling.Clear(Raid.FindSlot(actor.InstanceID));
            else if (status.ID == (uint)SID.Pyretic)
                PlayerStates[slot] = default;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (_boiling[slot])
            hints.Add($"Boiling on you in {(actor.FindStatus(SID.Boiling)!.Value.ExpireAt - WorldState.CurrentTime).TotalSeconds:f1}s. (Pyretic!)");
    }
}

class TwinscorchedHaloVeil(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCone cone = new(40f, 90f.Degrees());
    private static readonly AOEShapeDonut donut = new(10f, 40f);
    private static readonly AOEShapeCircle circle = new(15f);
    private readonly List<AOEInstance> _aoes = new(3);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var max = count > 2 ? 2 : count;
        var aoes = new AOEInstance[max];
        for (var i = 0; i < max; ++i)
        {
            var aoe = _aoes[i];
            if (i == 0)
                aoes[i] = count > 1 ? aoe with { Color = Colors.Danger } : aoe;
            else
                aoes[i] = aoe with { Risky = aoe.Shape != cone };
        }
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.TwinscorchedVeil1:
            case (uint)AID.TwinscorchedVeil2:
                AddAOEs(circle);
                break;
            case (uint)AID.TwinscorchedHalo1:
            case (uint)AID.TwinscorchedHalo2:
                AddAOEs(donut);
                break;
            case (uint)AID.Twinscorch1:
            case (uint)AID.Twinscorch2:
                AddAOEs(null);
                break;
        }
        void AddAOEs(AOEShape? secondaryShape)
        {
            var position = Module.PrimaryActor.Position;
            _aoes.Add(new(cone, position, spell.Rotation, Module.CastFinishAt(spell)));
            _aoes.Add(new(cone, position, spell.Rotation + 180.Degrees(), Module.CastFinishAt(spell, 2.3f)));
            if (secondaryShape != null)
                _aoes.Add(new(secondaryShape, position, default, Module.CastFinishAt(spell, 4.5f)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0)
            switch (spell.Action.ID)
            {
                case (uint)AID.HaloOfHeat2:
                case (uint)AID.VeilOfHeat2:
                case (uint)AID.ScorchingLeft1:
                case (uint)AID.ScorchingLeft2:
                case (uint)AID.ScorchingRight1:
                case (uint)AID.ScorchingRight2:
                    _aoes.RemoveAt(0);
                    break;
            }
    }
}

class HaloOfHeat1(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.HaloOfHeat1), new AOEShapeDonut(10f, 40f));
class VeilOfHeat1(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.VeilOfHeat1), 15f);
class FiresDomain(BossModule module) : Components.BaitAwayChargeCast(module, ActionID.MakeSpell(AID.FiresDomain), 3);
class CaptiveBolt(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.CaptiveBolt), 6f, 8);
class PyreOfRebirth(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.PyreOfRebirth));
class CullingBlade(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.CullingBlade));

class SansheyaStates : StateMachineBuilder
{
    public SansheyaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<HaloOfHeat1>()
            .ActivateOnEnter<VeilOfHeat1>()
            .ActivateOnEnter<TwinscorchedHaloVeil>()
            .ActivateOnEnter<FiresDomain>()
            .ActivateOnEnter<PyreOfRebirth>()
            .ActivateOnEnter<CullingBlade>()
            .ActivateOnEnter<CaptiveBolt>()
            .ActivateOnEnter<Boiling>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.S, NameID = 13399)]
public class Sansheya(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
