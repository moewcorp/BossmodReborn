namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS2StygimolochWarrior;

class FocusedTremorLarge(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.FocusedTremorAOELarge), new AOEShapeRect(20f, 10f), 2);
class ForcefulStrike(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.ForcefulStrike), new AOEShapeRect(44f, 24f));

// combined with flailing strike, first bait should be into first square
class FocusedTremorSmall : Components.SimpleAOEs
{
    public FocusedTremorSmall(BossModule module) : base(module, ActionID.MakeSpell(AID.FocusedTremorAOESmall), new AOEShapeRect(10f, 5f), 1)
    {
        Color = Colors.SafeFromAOE;
        Risky = false;
    }

    public void Activate()
    {
        Color = Colors.AOE;
        Risky = true;
        MaxCasts = 3;
    }
}

class FlailingStrikeBait(BossModule module) : Components.BaitAwayTethers(module, new AOEShapeCone(40f, 30f.Degrees()), (uint)TetherID.FlailingStrike);

class FlailingStrike(BossModule module) : Components.GenericRotatingAOE(module)
{
    private static readonly AOEShapeCone _shape = new(60f, 30f.Degrees());

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.FlailingStrikeFirst)
        {
            Sequences.Add(new(_shape, spell.LocXZ, spell.Rotation, 60f.Degrees(), Module.CastFinishAt(spell), 1.6f, 6, 3));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.FlailingStrikeRest)
        {
            AdvanceSequence(0, WorldState.CurrentTime);
        }
    }
}
