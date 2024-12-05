namespace BossMod.Dawntrail.Extreme.Ex3QueenEternal;

class LegitimateForce(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> AOEs = [];
    private static readonly HashSet<AID> castEnds = [AID.LegitimateForceFirstL, AID.LegitimateForceFirstR, AID.LegitimateForceSecondL, AID.LegitimateForceSecondR];
    private static readonly AOEShapeRect rect = new(20, 40);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = AOEs.Count;
        if (count == 0)
            yield break;
        for (var i = 0; i < count; ++i)
        {
            var aoe = AOEs[i];
            if (i == 0)
                yield return count != 1 ? aoe with { Color = Colors.Danger } : aoe;
            else if (i == 1)
                yield return aoe with { Risky = false };
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.LegitimateForceFirstR:
                AddAOEs(caster, spell, -90, 90);
                break;
            case AID.LegitimateForceFirstL:
                AddAOEs(caster, spell, 90, -90);
                break;
        }
    }

    private void AddAOEs(Actor caster, ActorCastInfo spell, float first, float second)
    {
        AOEs.Add(new(rect, caster.Position, spell.Rotation + first.Degrees(), Module.CastFinishAt(spell)));
        AOEs.Add(new(rect, caster.Position, spell.Rotation + second.Degrees(), Module.CastFinishAt(spell, 3.1f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (castEnds.Contains((AID)spell.Action.ID))
        {
            ++NumCasts;
            if (AOEs.Count != 0)
                AOEs.RemoveAt(0);
        }
    }
}
