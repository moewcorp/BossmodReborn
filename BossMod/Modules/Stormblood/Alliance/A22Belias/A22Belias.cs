namespace BossMod.Stormblood.Alliance.A22Belias;

class FireIV(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.FireIV));
class Eruption(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Eruption), 8f);
class TimeBomb2(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.TimeBomb2), new AOEShapeCone(60f, 45f.Degrees()));

class TimeEruption(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect rect = new(20f, 10f);
    private readonly List<AOEInstance> _aoes = new(4);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];

        var deadline = _aoes[0].Activation.AddSeconds(1d);

        var index = 0;
        while (index < count && _aoes[index].Activation < deadline)
            ++index;

        return CollectionsMarshal.AsSpan(_aoes)[..index];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.TimeEruptionAOEFirst or (uint)AID.TimeEruptionAOESecond)
        {
            _aoes.Add(new(rect, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
            if (_aoes.Count == 9)
                _aoes.SortBy(x => x.Activation);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.TimeEruptionAOEFirst or (uint)AID.TimeEruptionAOESecond)
            _aoes.RemoveAt(0);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 550, NameID = 7223)]
public class A22Belias(WorldState ws, Actor primary) : BossModule(ws, primary, new(-200f, -541f), new ArenaBoundsSquare(30))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.Gigas));
    }
}
