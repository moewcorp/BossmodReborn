namespace BossMod.Dawntrail.Savage.M04SWickedThunder;

class FulminousField(BossModule module) : Components.GenericAOEs(module)
{
    private Angle _dir;
    private DateTime _activation;

    public bool Active => _activation != default;

    private static readonly AOEShapeCone _shape = new(30f, 15f.Degrees());

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (Active)
        {
            var aoes = new AOEInstance[8];
            for (var i = 0; i < 8; ++i)
                aoes[i] = new(_shape, Arena.Center, _dir + i * 45f.Degrees(), _activation);
        }
        return [];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.FulminousField)
        {
            _dir = spell.Rotation;
            _activation = Module.CastFinishAt(spell);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (Active && spell.Action.ID is (uint)AID.FulminousField or (uint)AID.FulminousFieldRest)
        {
            ++NumCasts;
            _dir = caster.Rotation + 22.5f.Degrees();
            _activation = WorldState.FutureTime(3);
        }
    }
}

class ConductionPoint : Components.UniformStackSpread
{
    public ConductionPoint(BossModule module) : base(module, default, 6f)
    {
        AddSpreads(Raid.WithoutSlot(true, true, true), WorldState.FutureTime(12d));
    }
}

class ForkedFissures : Components.GenericWildCharge
{
    public ForkedFissures(BossModule module) : base(module, 5f, ActionID.MakeSpell(AID.ForkedFissures), 40f)
    {
        Array.Fill(PlayerRoles, PlayerRole.Share);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.ConductionPoint:
                Source = caster;
                var slot = Raid.FindSlot(spell.MainTargetID);
                if (slot >= 0)
                    PlayerRoles[slot] = PlayerRole.TargetNotFirst;
                break;
            case (uint)AID.ForkedFissures:
                ++NumCasts;
                break;
        }
    }
}
