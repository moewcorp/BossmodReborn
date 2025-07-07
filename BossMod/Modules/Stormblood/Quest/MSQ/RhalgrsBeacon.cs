﻿namespace BossMod.Stormblood.Quest.MSQ.RhalgrsBeacon;

public enum OID : uint
{
    Boss = 0x1A88,
    TerminusEst = 0x1BCA,
    MarkXLIIIArtilleryCannon = 0x1B4A, // R2.000, x3
    SkullsSpear = 0x1A8C, // R0.500, x3
    SkullsBlade = 0x1A8B, // R0.500, x3
    MagitekTurretII = 0x1BC7, // R0.600, x0 (spawn during fight)
    ChoppingBlock = 0x1EA4D9, // R0.500, x0 (spawn during fight), voidzone event object
    Helper = 0x233C
}

public enum AID : uint
{
    TheOrder = 8370, // Boss->self, 3.0s cast, single-target
    TerminusEst1 = 8337, // 1BCA->self, no cast, range 40+R width 4 rect
    Gunblade = 8310, // Boss->player, 5.0s cast, single-target, 10y knockback
    DiffractiveLaser = 8340, // 1BC7->self, 2.5s cast, range 18+R 60-degree cone
    ChoppingBlock1 = 8346, // 1A57->location, 3.0s cast, range 5 circle
}

class DiffractiveLaser(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DiffractiveLaser, new AOEShapeCone(18.6f, 30.Degrees()));

class TerminusEst(BossModule module) : Components.GenericAOEs(module, (uint)AID.TheOrder)
{
    private readonly List<Actor> Termini = [];
    private DateTime? CastFinish;
    private static readonly AOEShapeRect rect = new(41f, 2f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = Termini.Count;
        if (count == 0)
            return [];
        var aoes = new AOEInstance[count];
        for (var i = 0; i < count; ++i)
        {
            var t = Termini[i];
            aoes[i] = new(rect, t.Position, t.Rotation, activation: CastFinish ?? WorldState.FutureTime(10d));
        }
        return aoes;
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var terminii = Module.Enemies((uint)OID.TerminusEst);
        var count = terminii.Count;
        if (count == 0)
            return;
        for (var i = 0; i < count; ++i)
        {
            var m = terminii[i];
            if (!m.IsDead)
            {
                Arena.Actor(m, Colors.Object, true);
            }
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.TerminusEst)
            Termini.Add(actor);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
            CastFinish = Module.CastFinishAt(spell);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.TerminusEst1)
            Termini.Remove(caster);
    }
}

class Gunblade(BossModule module) : Components.GenericKnockback(module, (uint)AID.Gunblade, stopAtWall: true)
{
    private Actor? _caster;
    private readonly ChoppingBlock _aoe = module.FindComponent<ChoppingBlock>()!;

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        if (_caster is Actor c)
            return new Knockback[1] { new(c.Position, 10f, Module.CastFinishAt(c.CastInfo)) };
        return [];
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_caster is Actor source)
        {
            var aoes = _aoe.ActiveAOEs(slot, actor);
            var len = aoes.Length;
            if (len == 0)
                return;
            var voidzones = new Func<WPos, float>[len];
            for (var i = 0; i < len; ++i)
            {
                voidzones[i] = new(ShapeDistance.Circle(aoes[i].Origin, 5f));
            }
            var combined = ShapeDistance.Union(voidzones);
            float projectedDist(WPos pos)
            {
                var direction = (pos - source.Position).Normalized();
                var projected = pos + 10f * direction;
                return combined(projected);
            }
            hints.AddForbiddenZone(projectedDist, Module.CastFinishAt(source.CastInfo));
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
            _caster = caster;
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
            _caster = null;
    }
}

class ChoppingBlock(BossModule module) : Components.VoidzoneAtCastTarget(module, 5f, (uint)AID.ChoppingBlock1, GetVoidzones, 0f)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.ChoppingBlock);
        var count = enemies.Count;
        if (count == 0)
            return [];

        var voidzones = new Actor[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var z = enemies[i];
            if (z.EventState != 7)
                voidzones[index++] = z;
        }
        return voidzones[..index];
    }
}

class FordolaRemLupisStates : StateMachineBuilder
{
    public FordolaRemLupisStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ChoppingBlock>()
            .ActivateOnEnter<Gunblade>()
            .ActivateOnEnter<TerminusEst>()
            .ActivateOnEnter<DiffractiveLaser>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 68064, NameID = 5953)]
public class FordolaRemLupis(WorldState ws, Actor primary) : BossModule(ws, primary, new(-195.25f, 147.5f), new ArenaBoundsCircle(20));
