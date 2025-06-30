namespace BossMod.Dawntrail.FATE.Ttokrrone;

sealed class DesertDustdevil(BossModule module) : Components.GenericRotatingAOE(module)
{
    private static readonly AOEShapeCone cone = new(60f, 45f.Degrees());
    private readonly DustcloakDustdevil _aoe = module.FindComponent<DustcloakDustdevil>()!;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.FangwardDustdevilVisualCW:
            case (uint)AID.FangwardDustdevilVisualCCW:
            case (uint)AID.TailwardDustdevilVisualCW:
            case (uint)AID.TailwardDustdevilVisualCCW:
                AddSequence(spell, Sequences.Count == 0 ? 6 : 3);
                _aoe.SetAOE(spell.LocXZ, Module.CastFinishAt(spell, 0.9d));
                break;
        }
    }

    private void AddSequence(ActorCastInfo spell, int repeats)
    {
        var rotation = spell.Rotation;
        var direction = 90f.Degrees();
        var id = spell.Action.ID;
        if (id is (uint)AID.FangwardDustdevilVisualCW or (uint)AID.TailwardDustdevilVisualCW)
        {
            direction = -direction;
        }
        if (id is (uint)AID.TailwardDustdevilVisualCW or (uint)AID.TailwardDustdevilVisualCCW)
        {
            rotation += 180f.Degrees();
        }
        Sequences.Clear();
        Sequences.Add(new(cone, WPos.ClampToGrid(Module.PrimaryActor.Position), rotation, direction, Module.CastFinishAt(spell, 0.9d), 2.6f, repeats));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (Sequences.Count != 0)
        {
            switch (spell.Action.ID)
            {
                case (uint)AID.TailwardDustdevilFirst: // the first two hits of the rotation have the same angle
                case (uint)AID.FangwardDustdevilFirst:
                    UpdateAOE();
                    Sequences.Ref(0).NextActivation = WorldState.FutureTime(2.6d);
                    break;
                case (uint)AID.RightwardSandspoutDDRest:
                case (uint)AID.LeftwardSandspoutDDRest:
                    AdvanceSequence(0, WorldState.CurrentTime);
                    UpdateAOE();
                    break;
            }
            void UpdateAOE()
            {
                if (Sequences.Count != 0)
                {
                    _aoe.SetAOE(spell.LocXZ, WorldState.FutureTime(2.6d));
                }
                else
                {
                    _aoe.ClearAOE();
                }
            }
        }
    }
}

sealed class DustcloakDustdevil(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(13f);
    private AOEInstance? _aoe;
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(ref _aoe);

    public void SetAOE(WPos position, DateTime activation) => _aoe = new(circle, position, default, activation);
    public void ClearAOE() => _aoe = null;
}
