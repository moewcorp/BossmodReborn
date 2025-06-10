namespace BossMod.Dawntrail.Foray.ForkedTowerBlood.FTB1DemonTablet;

sealed class PortentousCometeor(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PortentousCometeor, 43f);

sealed class PortentousCometeorBait(BossModule module) : Components.GenericBaitAway(module, onlyShowOutlines: true)
{
    private static readonly AOEShapeCircle circle = new(43f);

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.CraterLater)
            CurrentBaits.Add(new(actor, actor, circle, WorldState.FutureTime(12d)));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.PortentousCometeor)
            CurrentBaits.Clear();
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (ActiveBaitsOn(actor).Count != 0)
        {
            hints.AddForbiddenZone(ShapeDistance.InvertedRect(Arena.Center, new WDir(default, 1f), 32f, 32f, 15f), CurrentBaits[0].Activation);
        }
    }
}

sealed class PortentousCometKnockback(BossModule module) : Components.GenericKnockback(module, ignoreImmunes: true)
{
    private static readonly AOEShapeCircle circle = new(4f);
    private readonly List<(Actor target, Angle dir)> targets = new(4);
    private DateTime activation;
    private readonly LandingKnockback _kb = module.FindComponent<LandingKnockback>()!;

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        if (_kb.Casters.Count != 0)
            return [];
        var count = targets.Count;
        if (count == 0)
            return [];
        Span<Knockback> knockbacks = new Knockback[count];
        for (var i = 0; i < count; ++i)
        {
            var kb = targets[i];
            knockbacks[i] = new(kb.target.Position, 13f, activation, circle, kb.dir, Kind: Kind.DirForward);
        }
        return knockbacks;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var id = spell.Action.ID;
        if (spell.Action.ID is (uint)AID.PortentousComet1 or (uint)AID.PortentousComet2)
        {
            targets.Add((WorldState.Actors.Find(spell.TargetID)!, id == (uint)AID.PortentousComet2 ? default : 180f.Degrees()));
            activation = Module.CastFinishAt(spell);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        var id = spell.Action.ID;
        if (spell.Action.ID is (uint)AID.PortentousComet1 or (uint)AID.PortentousComet2)
        {
            targets.Remove((WorldState.Actors.Find(spell.TargetID)!, id == (uint)AID.PortentousComet2 ? default : 180f.Degrees()));
        }
    }
}

sealed class PortentousComet1(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.PortentousComet1, 4f, 12);
sealed class PortentousComet2(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.PortentousComet2, 4f, 12);
