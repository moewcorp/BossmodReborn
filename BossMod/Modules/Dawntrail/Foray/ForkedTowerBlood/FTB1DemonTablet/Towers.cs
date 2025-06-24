namespace BossMod.Dawntrail.Foray.ForkedTowerBlood.FTB1DemonTablet;

sealed class DemonographTowers(BossModule module) : Components.CastTowersOpenWorld(module, (uint)AID.Explosion1, 4f, 4, 8, true);

// this is tricky to show in a useful way on radar since there is a grounded and floating tower in each position
sealed class GravityTowers(BossModule module) : Components.GenericTowersOpenWorld(module)
{
    private readonly HashSet<Actor> levitatingPlayers = new(24);
    private readonly HashSet<Actor> groundedPlayers = Soakers(module);
    private readonly List<Tower> towersGround = new(3);
    private readonly List<Tower> towersFlying = new(3);
    public bool Active => towersGround.Count != 0;

    public override ReadOnlySpan<Tower> ActiveTowers(int slot, Actor actor)
    {
        if (groundedPlayers.Contains(actor))
            return CollectionsMarshal.AsSpan(towersGround);
        else
            return CollectionsMarshal.AsSpan(towersFlying);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var id = spell.Action.ID;
        if (spell.Action.ID is (uint)AID.Explosion2 or (uint)AID.Explosion3)
        {
            var list = id == (uint)AID.Explosion2 ? towersFlying : towersGround;
            list.Add(new(spell.LocXZ, 4f, 4, 8, id == (uint)AID.Explosion2 ? levitatingPlayers : groundedPlayers, activation: Module.CastFinishAt(spell)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.Explosion2 or (uint)AID.Explosion3)
        {
            ++NumCasts;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.EraseGravity)
        {
            var targets = spell.Targets;
            var count = targets.Count;
            for (var i = 0; i < count; ++i)
            {
                var target = WorldState.Actors.Find(targets[i].ID)!;
                levitatingPlayers.Add(target);
                groundedPlayers.Remove(target);
            }
        }
    }
}

class EraseGravity(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(4);
    private static readonly AOEShapeCircle circle = new(4);
    private readonly GravityTowers _towers = module.FindComponent<GravityTowers>()!;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.EraseGravity)
            _aoes.Add(new(circle, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell), Colors.SafeFromAOE));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.EraseGravity)
        {
            ++NumCasts;
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (_aoes.Count != 0)
        {
            hints.Add("Stand under a statue if assigned for floating towers!");
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = _aoes.Count;
        if (count != 0)
        {
            var towers = _towers.ActiveTowers(slot, actor);
            var len = towers.Length;
            for (var i = 0; i < len; ++i)
            {
                var t = towers[i];
                if (t.NumInside(Module) < 8)
                {
                    return; // there are unfilled ground towers
                }
            }
            var forbidden = new Func<WPos, float>[count];
            for (var i = 0; i < count; ++i)
            {
                forbidden[i] = ShapeDistance.InvertedCircle(_aoes[i].Origin, 4f);
            }
            hints.AddForbiddenZone(ShapeDistance.Intersection(forbidden), _aoes[0].Activation);
        }
    }
}
