namespace BossMod.Dawntrail.FATE.Ttokrrone;

class DesertDustdevil(BossModule module) : Components.GenericRotatingAOE(module)
{
    private static readonly AOEShapeCone cone = new(60f, 45f.Degrees());
    private static readonly Angle offset = 180f.Degrees();
    private static readonly Angle a90 = 90f.Degrees();

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.FangwardDustdevilVisualCW:
            case (uint)AID.FangwardDustdevilVisualCCW:
            case (uint)AID.TailwardDustdevilVisualCW:
            case (uint)AID.TailwardDustdevilVisualCCW:
                AddSequence(spell, Sequences.Count == 0 ? 7 : 4);
                break;
        }
    }

    private void AddSequence(ActorCastInfo spell, int repeats)
    {
        var rotation = spell.Rotation;
        var direction = a90;

        if (spell.Action.ID is (uint)AID.FangwardDustdevilVisualCW or (uint)AID.TailwardDustdevilVisualCW)
            direction = -a90;
        else if (spell.Action.ID is (uint)AID.TailwardDustdevilVisualCW or (uint)AID.TailwardDustdevilVisualCCW)
            rotation += offset;
        if (Sequences.Count != 0)
            Sequences.Clear();
        Sequences.Add(new(cone, WPos.ClampToGrid(Module.PrimaryActor.Position), rotation, direction, Module.CastFinishAt(spell, 1), 2.6f, repeats));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.TailwardDustdevilFirst:
            case (uint)AID.FangwardDustdevilFirst:
            case (uint)AID.RightwardSandspoutDDRest:
            case (uint)AID.LeftwardSandspoutDDRest:
                AdvanceSequence(0, WorldState.CurrentTime);
                break;
        }
    }
}

class DustcloakDustdevil(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(13f);
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void Update()
    {
        var component = Module.FindComponent<DesertDustdevil>()!.Sequences.Count == 0;
        if (_aoe == null && !component)
            _aoe = new(circle, Module.PrimaryActor.Position, default, WorldState.FutureTime(8.1d));
        else if (_aoe != null && component)
            _aoe = null;
    }
}
