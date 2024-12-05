namespace BossMod.Dawntrail.Alliance.A14ShadowLord;

class CthonicFury(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;
    public bool Active => _aoe != null || Arena.Bounds != A14ShadowLord.DefaultBounds;
    private static readonly Square[] def = [new Square(A14ShadowLord.ArenaCenter, 30)]; // using a square for the difference instead of a circle since less vertices will result in slightly better performance
    public static readonly AOEShapeCustom AOEBurningBattlements = new(def, [new Square(A14ShadowLord.ArenaCenter, 11.5f, 45.Degrees())]);
    private static readonly AOEShapeCustom aoeCthonicFury = new(def, A14ShadowLord.Combined);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.CthonicFuryStart)
            _aoe = new(aoeCthonicFury, Arena.Center, default, Module.CastFinishAt(spell));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.CthonicFuryStart:
                _aoe = null;
                Arena.Bounds = A14ShadowLord.ComplexBounds;
                break;
            case AID.CthonicFuryEnd:
                Arena.Bounds = A14ShadowLord.DefaultBounds;
                break;
        }
    }
}

class BurningCourt(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.BurningCourt), new AOEShapeCircle(8));
class BurningMoat(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.BurningMoat), new AOEShapeDonut(5, 15));
class BurningKeep(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.BurningKeep), new AOEShapeRect(11.5f, 11.5f, 11.5f));
class BurningBattlements(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.BurningBattlements), CthonicFury.AOEBurningBattlements);

class DarkNebula(BossModule module) : Components.Knockback(module)
{
    public readonly List<Actor> Casters = [];

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        foreach (var caster in Casters.Take(2))
        {
            var dir = caster.CastInfo?.Rotation ?? caster.Rotation;
            var kind = dir.ToDirection().OrthoL().Dot(actor.Position - caster.Position) > 0 ? Kind.DirLeft : Kind.DirRight;
            yield return new(caster.Position, 20, Module.CastFinishAt(caster.CastInfo), null, dir, kind);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.DarkNebulaShort or AID.DarkNebulaLong)
            Casters.Add(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.DarkNebulaShort or AID.DarkNebulaLong)
        {
            ++NumCasts;
            Casters.Remove(caster);
        }
    }
}

class EchoesOfAgony(BossModule module) : Components.StackWithIcon(module, (uint)IconID.EchoesOfAgony, ActionID.MakeSpell(AID.EchoesOfAgonyAOE), 5, 9.2f, PartyState.MaxAllianceSize, PartyState.MaxAllianceSize)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.EchoesOfAgony)
            NumFinishedStacks = 0;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == StackAction)
        {
            if (++NumFinishedStacks >= 5)
            {
                Stacks.Clear();
            }
        }
    }
}
