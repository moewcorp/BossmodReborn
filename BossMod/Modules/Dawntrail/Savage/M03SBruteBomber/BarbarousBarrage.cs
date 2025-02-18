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
            foreach (var p in TowerPositions(CurState == State.NextNS ? State.NextCorners : CurState + 1))
                Arena.AddCircle(p, 4, Colors.Object);
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (CurState == State.None && index is 14 or 15)
            SetState(index == 14 ? State.NextNS : State.NextEW, 4, 10.1f);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.BarbarousBarrageExplosion4:
                SetState(State.NextCorners, 2, 3);
                break;
            case AID.BarbarousBarrageExplosion2:
                SetState(State.NextCenter, 8, 3);
                break;
            case AID.BarbarousBarrageExplosion8:
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
            foreach (var p in TowerPositions(state))
                Towers.Add(new(p, 4, soakers, soakers, default, WorldState.FutureTime(activation)));
        }
    }

    private IEnumerable<WPos> TowerPositions(State state)
    {
        switch (state)
        {
            case State.NextNS:
                yield return Arena.Center + new WDir(0, -11);
                yield return Arena.Center + new WDir(0, +11);
                break;
            case State.NextEW:
                yield return Arena.Center + new WDir(-11, 0);
                yield return Arena.Center + new WDir(+11, 0);
                break;
            case State.NextCorners:
                yield return Arena.Center + new WDir(-11, -11);
                yield return Arena.Center + new WDir(-11, +11);
                yield return Arena.Center + new WDir(+11, -11);
                yield return Arena.Center + new WDir(+11, +11);
                break;
            case State.NextCenter:
                yield return Arena.Center;
                break;
        }
    }
}

class BarbarousBarrageKnockback(BossModule module) : Components.Knockback(module)
{
    private readonly BarbarousBarrageTowers? _towers = module.FindComponent<BarbarousBarrageTowers>();

    private static readonly AOEShapeCircle _shape = new(4);

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        if (_towers != null)
        {
            foreach (var t in _towers.Towers)
            {
                var dist = t.MinSoakers switch
                {
                    4 => 23,
                    2 => 19,
                    8 => 15,
                    _ => 0
                };
                yield return new(t.Position, dist, t.Activation, _shape);
            }
        }
    }
}

class BarbarousBarrageMurderousMist(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.BarbarousBarrageMurderousMist), new AOEShapeCone(40, 135.Degrees()));

class BarbarousBarrageLariatCombo(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> AOEs = [];
    public static readonly Angle a90 = 90.Degrees();
    private static readonly AOEShapeRect _shape = new(70, 17);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => AOEs.Skip(NumCasts).Take(1);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var (off1, off2) = (AID)spell.Action.ID switch
        {
            AID.BarbarousBarrageLariatComboFirstRR => (-a90, -a90),
            AID.BarbarousBarrageLariatComboFirstRL => (-a90, a90),
            AID.BarbarousBarrageLariatComboFirstLR => (a90, -a90),
            AID.BarbarousBarrageLariatComboFirstLL => (a90, a90),
            _ => default
        };
        if (off1 != default)
        {
            var from = caster.Position;
            var to = spell.LocXZ;
            var offset = 0.6667f * (to - from);
            var dir1 = Angle.FromDirection(offset);
            var dir2 = dir1 + 180.Degrees();
            AOEs.Add(new(_shape, from - offset + 12 * (dir1 + off1).ToDirection(), dir1, Module.CastFinishAt(spell, 1.2f)));
            AOEs.Add(new(_shape, to + offset + 12 * (dir2 + off2).ToDirection(), dir2, Module.CastFinishAt(spell, 5.6f)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.BarbarousBarrageLariatComboFirstRAOE or AID.BarbarousBarrageLariatComboFirstLAOE or AID.BarbarousBarrageLariatComboSecondRAOE or AID.BarbarousBarrageLariatComboSecondLAOE)
        {
            if (NumCasts < AOEs.Count && !spell.LocXZ.AlmostEqual(AOEs[NumCasts].Origin, 2))
                ReportError($"Unexpected AOE: {spell.Location} vs {AOEs[NumCasts].Origin}");
            ++NumCasts;
        }
    }
}
