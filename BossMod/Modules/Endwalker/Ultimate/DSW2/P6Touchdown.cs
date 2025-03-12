namespace BossMod.Endwalker.Ultimate.DSW2;

class P6Touchdown(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.TouchdownAOE))
{
    private static readonly AOEShapeCircle _shape = new(20f); // TODO: verify falloff

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        // TODO: activation
        var aoes = new AOEInstance[2];
        aoes[0] = new(_shape, Arena.Center);
        aoes[1] = new(_shape, Arena.Center + new WDir(default, 25f));
        return aoes;
    }
}

class P6TouchdownCauterize(BossModule module) : BossComponent(module)
{
    private Actor? _nidhogg;
    private Actor? _hraesvelgr;
    private BitMask _boiling;
    private BitMask _freezing;

    private static readonly AOEShapeRect _shape = new(80f, 11f);

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var nidhoggSide = actor.Position.X < Module.Center.X; // note: assume nidhogg cleaves whole left side, hraes whole right side
        var forbiddenMask = nidhoggSide ? _boiling : _freezing;
        if (forbiddenMask[slot])
            hints.Add("GTFO from wrong side!");

        // note: assume both dragons are always at north side
        var isClosest = Raid.WithoutSlot(false, true, true).Where(p => (p.Position.X < Module.Center.X) == nidhoggSide).MinBy(p => p.PosRot.Z) == actor;
        var shouldBeClosest = actor.Role == Role.Tank;
        if (isClosest != shouldBeClosest)
            hints.Add(shouldBeClosest ? "Move closer to dragons!" : "Move away from dragons!");
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (_boiling[pcSlot] && _nidhogg != null)
            _shape.Draw(Arena, _nidhogg);
        if (_freezing[pcSlot] && _hraesvelgr != null)
            _shape.Draw(Arena, _hraesvelgr);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.Boiling:
                _boiling[Raid.FindSlot(actor.InstanceID)] = true;
                break;
            case (uint)SID.Freezing:
                _freezing[Raid.FindSlot(actor.InstanceID)] = true;
                break;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.CauterizeN:
                _nidhogg = caster;
                break;
            case (uint)AID.CauterizeH:
                _hraesvelgr = caster;
                break;
        }
    }
}
