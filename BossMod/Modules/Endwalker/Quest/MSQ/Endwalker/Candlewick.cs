namespace BossMod.Endwalker.Quest.MSQ.Endwalker;

class Candlewick(BossModule module) : Components.ConcentricAOEs(module, _shapes)
{
    private static readonly AOEShape[] _shapes = [new AOEShapeCircle(10), new AOEShapeDonut(10, 30)];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.CandlewickPointBlank)
            AddSequence(spell.LocXZ, Module.CastFinishAt(spell, 2));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (Sequences.Count != 0)
        {
            var order = (AID)spell.Action.ID switch
            {
                AID.CandlewickPointBlank => 0,
                AID.CandlewickDonut => 1,
                _ => -1
            };
            AdvanceSequence(order, spell.LocXZ, WorldState.FutureTime(2));
        }
    }
}
