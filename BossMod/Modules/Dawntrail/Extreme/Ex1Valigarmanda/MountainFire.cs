﻿namespace BossMod.Dawntrail.Extreme.Ex1Valigarmanda;

sealed class MountainFire(BossModule module) : Components.GenericTowers(module, (uint)AID.MountainFireTower)
{
    private BitMask _nonTanks = GetNonTanks(module);
    private BitMask _lastSoakers;

    private static BitMask GetNonTanks(BossModule module)
    {
        var party = module.Raid.WithSlot(true, true, true);
        var len = party.Length;
        BitMask nontanks = default;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var p = ref party[i];
            if (p.Item2.Role != Role.Tank)
            {
                nontanks[p.Item1] = true;
            }
        }
        return nontanks;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
            Towers.Add(new(spell.LocXZ, 3f, forbiddenSoakers: _nonTanks | _lastSoakers));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
            Towers.Clear();
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            ++NumCasts;
            _lastSoakers = default;
            var targets = CollectionsMarshal.AsSpan(spell.Targets);
            var len = targets.Length;
            for (var i = 0; i < len; ++i)
            {
                ref readonly var targ = ref targets[i];
                _lastSoakers.Set(Raid.FindSlot(targ.ID));
            }
        }
    }
}

sealed class MountainFireCone(BossModule module) : Components.GenericAOEs(module)
{
    private readonly MountainFire? _tower = module.FindComponent<MountainFire>();
    private AOEInstance[] _aoe = [];
    private static readonly AOEShapeCone _shape = new(40f, 165f.Degrees());

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        // show aoe only if not (or not allowed to) soak the tower
        if (_aoe.Length != 0)
        {
            var isForbidden = false;
            if (_tower != null)
            {
                var count = _tower.Towers.Count;
                for (var i = 0; i < count; ++i)
                {
                    if (_tower.Towers[i].ForbiddenSoakers[slot])
                    {
                        isForbidden = true;
                        break;
                    }
                }
            }
            ref var aoe = ref _aoe[0];
            if (isForbidden || !actor.Position.InCircle(aoe.Origin, 3f))
            {
                return _aoe;
            }
        }
        return [];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.MountainFireTower)
        {
            _aoe = [new(_shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell, 0.4d))];
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.MountainFireConeAOE)
        {
            _aoe = [];
            ++NumCasts;
        }
    }
}
