namespace BossMod.Dawntrail.Extreme.Ex1Valigarmanda;

class MountainFire(BossModule module) : Components.GenericTowers(module, ActionID.MakeSpell(AID.MountainFireTower))
{
    private BitMask _nonTanks = module.Raid.WithSlot(true, true, true).WhereActor(p => p.Role != Role.Tank).Mask();
    private BitMask _lastSoakers;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            Towers.Add(new(caster.Position, 3f, forbiddenSoakers: _nonTanks | _lastSoakers));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            Towers.Clear();
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            ++NumCasts;
            _lastSoakers.Reset();
            var count = spell.Targets.Count;
            for (var i = 0; i < count; ++i)
                _lastSoakers.Set(Raid.FindSlot(spell.Targets[i].ID));
        }
    }
}

class MountainFireCone(BossModule module) : Components.GenericAOEs(module)
{
    private readonly MountainFire? _tower = module.FindComponent<MountainFire>();
    private AOEInstance? _aoe;

    private static readonly AOEShapeCone _shape = new(40f, 165f.Degrees());

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        // show aoe only if not (or not allowed to) soak the tower
        if (_aoe is AOEInstance aoe)
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

            if (isForbidden || !actor.Position.InCircle(aoe.Origin, 3f))
            {
                return new AOEInstance[1] { aoe };
            }
        }
        return [];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.MountainFireTower)
            _aoe = new(_shape, caster.Position, spell.Rotation, Module.CastFinishAt(spell, 0.4f));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.MountainFireConeAOE)
        {
            _aoe = null;
            ++NumCasts;
        }
    }
}
