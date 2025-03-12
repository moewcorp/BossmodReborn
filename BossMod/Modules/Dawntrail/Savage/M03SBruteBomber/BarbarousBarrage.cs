using BossMod.Shadowbringers.Foray.CriticalEngagement.CE14VigilForLost;

namespace BossMod.Dawntrail.Savage.M03SBruteBomber;

class BarbarousBarrageTowers(BossModule module) : Components.GenericTowers(module)
{
    public enum State { None, NextNS, NextEW, NextCorners, NextCenter, Done }

    public State CurState;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        // draw next towers to aim knockback
        if (CurState != State.None)
        {
            var positions = TowerPositions(CurState == State.NextNS ? State.NextCorners : CurState + 1);
            var count = positions.Count;
            for (var i = 0; i < count; ++i)
                Arena.AddCircle(positions[i], 4f, Colors.Object);
        }
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (CurState == State.None && index is 14 or 15)
            SetState(index == 14 ? State.NextNS : State.NextEW, 4, 10.1f);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.BarbarousBarrageExplosion4:
                SetState(State.NextCorners, 2, 3);
                break;
            case (uint)AID.BarbarousBarrageExplosion2:
                SetState(State.NextCenter, 8, 3);
                break;
            case (uint)AID.BarbarousBarrageExplosion8:
                SetState(State.Done, 1, default);
                break;
        }
    }

    private void SetState(State state, int soakers, float activation)
    {
        if (CurState != state)
        {
            CurState = state;
            Towers.Clear();
            var positions = TowerPositions(state);
            var count = positions.Count;
            for (var i = 0; i < count; ++i)
                Towers.Add(new(positions[i], 4f, soakers, soakers, default, WorldState.FutureTime(activation)));
        }
    }

    private List<WPos> TowerPositions(State state)
    {
        List<WPos> towers = new(4);
        var pos = Arena.Center;
        switch (state)
        {
            case State.NextNS:
                towers.Add(pos + new WDir(0, -11));
                towers.Add(pos + new WDir(0, +11));
                break;
            case State.NextEW:
                towers.Add(pos + new WDir(-11, 0));
                towers.Add(pos + new WDir(+11, 0));
                break;
            case State.NextCorners:
                towers.Add(pos + new WDir(-11, -11));
                towers.Add(pos + new WDir(-11, +11));
                towers.Add(pos + new WDir(+11, -11));
                towers.Add(pos + new WDir(+11, +11));
                break;
            case State.NextCenter:
                towers.Add(pos);
                break;
        }
        return towers;
    }
}

class BarbarousBarrageKnockback(BossModule module) : Components.Knockback(module)
{
    private readonly BarbarousBarrageTowers? _towers = module.FindComponent<BarbarousBarrageTowers>();
    private static readonly AOEShapeCircle _shape = new(4f);

    public override ReadOnlySpan<Source> ActiveSources(int slot, Actor actor)
    {
        if (_towers != null)
        {
            var towers = _towers.Towers;
            var count = towers.Count;
            var sources = new Source[count];
            for (var i = 0; i < count; ++i)
            {
                var t = towers[i];
                var dist = t.MinSoakers switch
                {
                    4 => 23,
                    2 => 19,
                    8 => 15,
                    _ => 0
                };
                sources[i] = new(t.Position, dist, t.Activation, _shape);
            }
            return sources;
        }
        return [];
    }
}

class BarbarousBarrageMurderousMist(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.BarbarousBarrageMurderousMist), new AOEShapeCone(40f, 135f.Degrees()));

class BarbarousBarrageLariatCombo(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    public static readonly Angle a90 = 90.Degrees();
    private static readonly AOEShapeRect _shape = new(70, 17);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Count != 0 ? CollectionsMarshal.AsSpan(_aoes)[..1] : [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var (off1, off2) = spell.Action.ID switch
        {
            (uint)AID.BarbarousBarrageLariatComboFirstRR => (-a90, -a90),
            (uint)AID.BarbarousBarrageLariatComboFirstRL => (-a90, a90),
            (uint)AID.BarbarousBarrageLariatComboFirstLR => (a90, -a90),
            (uint)AID.BarbarousBarrageLariatComboFirstLL => (a90, a90),
            _ => default
        };
        if (off1 != default)
        {
            var from = caster.Position;
            var to = spell.LocXZ;
            var offset = 0.6667f * (to - from);
            var dir1 = Angle.FromDirection(offset);
            var dir2 = dir1 + 180.Degrees();
            _aoes.Add(new(_shape, from - offset + 12 * (dir1 + off1).ToDirection(), dir1, Module.CastFinishAt(spell, 1.2f)));
            _aoes.Add(new(_shape, to + offset + 12 * (dir2 + off2).ToDirection(), dir2, Module.CastFinishAt(spell, 5.6f)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.BarbarousBarrageLariatComboFirstRAOE or (uint)AID.BarbarousBarrageLariatComboFirstLAOE or (uint)AID.BarbarousBarrageLariatComboSecondRAOE or (uint)AID.BarbarousBarrageLariatComboSecondLAOE)
        {
            if (NumCasts < _aoes.Count && !spell.LocXZ.AlmostEqual(_aoes[NumCasts].Origin, 2f))
                ReportError($"Unexpected AOE: {spell.Location} vs {_aoes[NumCasts].Origin}");
            ++NumCasts;
            if (_aoes.Count != 0)
                _aoes.RemoveAt(0);
        }
    }
}
