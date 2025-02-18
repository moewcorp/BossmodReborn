namespace BossMod.Dawntrail.Raid.M04NWickedThunder;

class WickedHypercannon(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect rect = new(40, 10);
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.WickedHypercannonVisual2 or AID.WickedHypercannonVisual3)
        {
            var activation = Module.CastFinishAt(spell, 0.6f);
            if (Arena.Center == ArenaChanges.eastremovedCenter)
                _aoe = new(rect, new(85, 80), default, activation, Colors.SafeFromAOE, false);
            else if (Arena.Center == ArenaChanges.westRemovedCenter)
                _aoe = new(rect, new(115, 80), default, activation, Colors.SafeFromAOE, false);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.WickedHypercannon)
        {
            if (++NumCasts == 10)
            {
                _aoe = null;
                NumCasts = 0;
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_aoe != null)
        {
            var activation = _aoe?.Activation!;
            hints.PredictedDamage.Add((Raid.WithSlot(false, true, true).Mask(), (DateTime)activation));
        }
    }
}
