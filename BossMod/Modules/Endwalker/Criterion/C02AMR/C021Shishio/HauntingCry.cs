namespace BossMod.Endwalker.VariantCriterion.C02AMR.C021Shishio;

class HauntingCrySwipes(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(4);
    private static readonly AOEShapeCone _shape = new(40f, 90f.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.NRightSwipe:
            case (uint)AID.NLeftSwipe:
            case (uint)AID.SRightSwipe:
            case (uint)AID.SLeftSwipe:
                _aoes.Add(new(_shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.NRightSwipe:
            case (uint)AID.NLeftSwipe:
            case (uint)AID.SRightSwipe:
            case (uint)AID.SLeftSwipe:
                ++NumCasts;
                if (_aoes.Count != 0)
                    _aoes.RemoveAt(0);
                break;
        }
    }
}

class HauntingCryReisho(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<Actor> _ghosts = new(4);
    private DateTime _activation;
    private DateTime _ignoreBefore;

    private static readonly AOEShapeCircle _shape = new(6f);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _ghosts.Count;
        if (count == 0)
            return [];
        var aoes = new AOEInstance[count];
        for (var i = 0; i < count; ++i)
            aoes[i] = new(_shape, _ghosts[i].Position, default, _activation);
        return aoes;
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var g in _ghosts)
        {
            Arena.Actor(g, Colors.Object, true);
            var target = WorldState.Actors.Find(g.Tether.Target);
            if (target != null)
                Arena.AddLine(g.Position, target.Position, Colors.Danger);
        }
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (source.OID is (uint)OID.NHauntingThrall or (uint)OID.SHauntingThrall)
        {
            _ghosts.Add(source);
            _activation = WorldState.FutureTime(5.1f);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.NReisho or (uint)AID.SReisho && WorldState.CurrentTime > _ignoreBefore)
        {
            ++NumCasts;
            _activation = WorldState.FutureTime(2.1f);
            _ignoreBefore = WorldState.FutureTime(1);
        }
    }
}

abstract class HauntingCryVermilionAura(BossModule module, AID aid) : Components.CastTowers(module, ActionID.MakeSpell(aid), 4f);
class NHauntingCryVermilionAura(BossModule module) : HauntingCryVermilionAura(module, AID.NVermilionAura);
class SHauntingCryVermilionAura(BossModule module) : HauntingCryVermilionAura(module, AID.SVermilionAura);

abstract class HauntingCryStygianAura(BossModule module, AID aid) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(aid), 15f, true);
class NHauntingCryStygianAura(BossModule module) : HauntingCryStygianAura(module, AID.NStygianAura);
class SHauntingCryStygianAura(BossModule module) : HauntingCryStygianAura(module, AID.SStygianAura);
