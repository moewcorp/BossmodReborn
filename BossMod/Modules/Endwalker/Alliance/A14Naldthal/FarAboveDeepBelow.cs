namespace BossMod.Endwalker.Alliance.A14Naldthal;

// TODO: create and use generic 'line stack' component
class FarFlungFire(BossModule module) : Components.GenericWildCharge(module, 3f, fixedLength: 40f)
{
    private bool _real;
    private ulong _targetID;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.FarAboveDeepBelowNald or (uint)AID.HearthAboveFlightBelowNald or (uint)AID.HearthAboveFlightBelowThalNald)
        {
            _real = true;
            InitIfReal();
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.FarFlungFireVisual:
                Source = caster;
                _targetID = spell.MainTargetID;
                InitIfReal();
                break;
            case (uint)AID.FarFlungFireAOE:
                ++NumCasts;
                _real = false;
                Source = null;
                break;
        }
    }

    private void InitIfReal()
    {
        if (_real && _targetID != 0)
            foreach (var (i, p) in Raid.WithSlot(true, false, true))
                PlayerRoles[i] = p.InstanceID == _targetID ? PlayerRole.Target : PlayerRole.Share;
    }
}

class DeepestPit(BossModule module) : Components.GenericAOEs(module)
{
    private bool _real;
    private readonly List<Actor> _targets = [];
    private readonly List<AOEInstance> _aoes = [];

    public bool Active => _aoes.Count != 0;

    private static readonly AOEShapeCircle _shape = new(6f);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor) => _real && _targets.Contains(player) ? PlayerPriority.Danger : PlayerPriority.Irrelevant;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_real)
            foreach (var t in _targets)
                Arena.AddCircle(t.Position, _shape.Radius);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.FarAboveDeepBelowThal:
            case (uint)AID.HearthAboveFlightBelowThal:
            case (uint)AID.HearthAboveFlightBelowNaldThal:
                _real = true;
                break;
            case (uint)AID.DeepestPitFirst:
            case (uint)AID.DeepestPitRest:
                _aoes.Add(new(_shape, spell.LocXZ, default, Module.CastFinishAt(spell), ActorID: caster.InstanceID));
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.DeepestPitFirst or (uint)AID.DeepestPitRest)
        {
            var count = _aoes.Count;
            for (var i = 0; i < count; ++i)
            {
                if (_aoes[i].ActorID == caster.InstanceID)
                {
                    _aoes.RemoveAt(i);
                    break;
                }
            }
            if (count == 0)
                _targets.Clear();
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.DeepestPitTarget)
        {
            _targets.Add(actor);
        }
    }
}
