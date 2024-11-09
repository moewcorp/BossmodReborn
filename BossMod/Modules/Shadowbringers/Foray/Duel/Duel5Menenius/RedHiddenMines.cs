﻿namespace BossMod.Shadowbringers.Foray.Duel.Duel5Menenius;

class RedHiddenMines(BossModule module) : Components.GenericAOEs(module)
{
    private List<AOEInstance> _mines = [];
    private static readonly AOEShapeCircle _shapeTrigger = new(3.6f);
    private static readonly AOEShapeCircle _shapeExplosion = new(8);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _mines;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.ActivateRedMine)
            _mines.Add(new(_shapeTrigger, caster.Position, Color: Colors.Trap));
        else if ((AID)spell.Action.ID is AID.DetonateRedMine or AID.Explosion)
            _mines.RemoveAll(t => t.Origin.AlmostEqual(caster.Position, 1));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.IndiscriminateDetonation)
        {
            List<AOEInstance> _detonatingMines = [];
            for (var i = 0; i < _mines.Count; ++i)
                _detonatingMines.Add(new(_shapeExplosion, _mines[i].Origin, Color: Colors.AOE));
            _mines = _detonatingMines;
        }
    }
}
