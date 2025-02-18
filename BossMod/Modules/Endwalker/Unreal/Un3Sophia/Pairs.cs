﻿namespace BossMod.Endwalker.Unreal.Un3Sophia;

// TODO: there doesn't seem to be any event if mechanic is resolved correctly?..
class Pairs(BossModule module) : BossComponent(module)
{
    private BitMask _players1;
    private BitMask _players2;
    private DateTime _activation;

    private const float _radius = 4; // TODO: verify

    public bool Active => (_players1 | _players2).Any();

    public override void Update()
    {
        if (WorldState.CurrentTime > _activation && Active)
        {
            _players1.Reset();
            _players2.Reset();
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var atRisk = _players1[slot] ? AtRisk(actor, _players1, _players2) : _players2[slot] && AtRisk(actor, _players2, _players1);
        if (atRisk)
            hints.Add("Stack with opposite color!");
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var p in Raid.WithSlot(false, true, true).IncludedInMask(_players1).Exclude(pc))
            Arena.AddCircle(p.Item2.Position, _radius, _players1[pcSlot] ? Colors.Danger : Colors.Safe);
        foreach (var p in Raid.WithSlot(false, true, true).IncludedInMask(_players2).Exclude(pc))
            Arena.AddCircle(p.Item2.Position, _radius, _players2[pcSlot] ? Colors.Danger : Colors.Safe);
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        switch ((IconID)iconID)
        {
            case IconID.Pairs1:
                _players1.Set(Raid.FindSlot(actor.InstanceID));
                _activation = WorldState.FutureTime(5); // TODO: verify
                break;
            case IconID.Pairs2:
                _players2.Set(Raid.FindSlot(actor.InstanceID));
                _activation = WorldState.FutureTime(5); // TODO: verify
                break;
        }
    }

    private bool AtRisk(Actor actor, BitMask same, BitMask opposite)
    {
        return Raid.WithSlot(false, true, true).IncludedInMask(opposite).InRadius(actor.Position, _radius).Any() || !Raid.WithSlot(false, true, true).IncludedInMask(same).InRadiusExcluding(actor, _radius).Any();
    }
}
