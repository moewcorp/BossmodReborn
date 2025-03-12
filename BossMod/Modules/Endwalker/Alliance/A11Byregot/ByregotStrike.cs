namespace BossMod.Endwalker.Alliance.A11Byregot;

class ByregotStrikeJump(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.ByregotStrikeJump), 8f);
class ByregotStrikeJumpCone(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.ByregotStrikeJumpCone), 8f);
class ByregotStrikeKnockback(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.ByregotStrikeKnockback), 18f);

class ByregotStrikeCone(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeCone _shape = new(90, 22.5f.Degrees());

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ByregotStrikeKnockback && Module.PrimaryActor.FindStatus((uint)SID.Glow) != null)
            for (var i = 0; i < 4; ++i)
                _aoes.Add(new(_shape, spell.LocXZ, spell.Rotation + i * 90f.Degrees(), Module.CastFinishAt(spell)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.ByregotStrikeCone)
        {
            _aoes.Clear();
            ++NumCasts;
        }
    }
}
