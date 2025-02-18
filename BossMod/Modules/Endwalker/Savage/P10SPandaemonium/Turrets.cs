﻿namespace BossMod.Endwalker.Savage.P10SPandaemonium;

class Turrets(BossModule module) : Components.Knockback(module, ActionID.MakeSpell(AID.PealOfCondemnation), true, 1, stopAfterWall: true)
{
    private readonly Actor?[] _turrets = new Actor?[8]; // pairs in order of activation
    private DateTime _activation;
    private BitMask _forbidden;

    private const float _distance = 17;
    private static readonly AOEShapeRect _shape = new(50, 2.5f);

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        foreach (var t in ImminentTurretsWithTargets())
            if (t.source != null && t.target != null)
                yield return new(t.source.Position, _distance, _activation, _shape, Angle.FromDirection(t.target.Position - t.source.Position));
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        var inCount = 0;
        foreach (var t in ImminentTurretsWithTargets())
        {
            if (t.source == null || t.target == null || !_shape.Check(actor.Position, t.source.Position, Angle.FromDirection(t.target.Position - t.source.Position)))
                continue; // not in aoe
            ++inCount;
        }

        if (inCount > 1)
            hints.Add("GTFO from one of the knockbacks!");
        else if (inCount > 0 && _forbidden[slot])
            hints.Add("GTFO from knockback!");
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor) => ImminentTurretsWithTargets().Any(t => t.target == player) ? PlayerPriority.Interesting : PlayerPriority.Irrelevant;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var t in ImminentTurretsWithTargets())
        {
            Arena.Actor(t.source, Colors.Enemy, true);
            if (t.source != null && t.target != null)
                _shape.Outline(Arena, t.source.Position, Angle.FromDirection(t.target.Position - t.source.Position));
        }

        foreach (var t in FutureTurrets())
            Arena.Actor(t, Colors.Object, true);

        base.DrawArenaForeground(pcSlot, pc);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.DarkResistanceDown)
            _forbidden.Set(Raid.FindSlot(actor.InstanceID));
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        var order = (IconID)iconID switch
        {
            IconID.Order1 => 0,
            IconID.Order2 => 1,
            IconID.Order3 => 2,
            IconID.Order4 => 3,
            _ => -1
        };
        if (order < 0)
            return;

        _activation = WorldState.FutureTime(8.1f);
        if (_turrets[order * 2] == null)
            _turrets[order * 2] = actor;
        else if (_turrets[order * 2 + 1] == null)
            _turrets[order * 2 + 1] = actor;
        else
            ReportError($"More than 2 turrets of order {order}");
    }

    private IEnumerable<(Actor? source, Actor? target)> ImminentTurretsWithTargets() => _turrets.Skip(NumCasts).Take(2).Select(t => (t, WorldState.Actors.Find(t?.TargetID ?? 0)));
    private IEnumerable<Actor?> FutureTurrets() => _turrets.Skip(NumCasts + 2).Take(2);
}
