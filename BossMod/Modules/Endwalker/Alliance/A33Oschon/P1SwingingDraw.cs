namespace BossMod.Endwalker.Alliance.A33Oschon;

class P1SwingingDraw(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.SwingingDrawAOE))
{
    public readonly List<AOEInstance> AOEs = [];
    private static readonly AOEShapeCone _shape = new(60f, 60f.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => AOEs;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var dir = spell.Action.ID switch
        {
            (uint)AID.SwingingDrawCW => -45f.Degrees(),
            (uint)AID.SwingingDrawCCW => 45f.Degrees(),
            _ => default
        };
        if (dir != default)
        {
            dir += Angle.FromDirection(caster.Position - Arena.Center);
            AOEs.Add(new(_shape, WPos.ClampToGrid(Arena.Center + 25 * dir.ToDirection()), dir + 180.Degrees(), Module.CastFinishAt(spell, 6.2f)));
        }
    }
}
