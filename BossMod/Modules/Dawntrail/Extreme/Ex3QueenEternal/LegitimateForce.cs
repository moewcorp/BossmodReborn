namespace BossMod.Dawntrail.Extreme.Ex3QueenEternal;

class LegitimateForce(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> AOEs = new(2);
    private static readonly AOEShapeRect rect = new(20f, 40f);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = AOEs.Count;
        if (count == 0)
            return [];
        var aoes = new AOEInstance[count];
        for (var i = 0; i < count; ++i)
        {
            var aoe = AOEs[i];
            if (i == 0)
                aoes[i] = count != 1 ? aoe with { Color = Colors.Danger } : aoe;
            else
                aoes[i] = aoe with { Risky = false };
        }
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.LegitimateForceFirstR:
                AddAOEs(caster, -90f, 90f);
                break;
            case (uint)AID.LegitimateForceFirstL:
                AddAOEs(caster, 90f, -90f);
                break;
        }
        void AddAOEs(Actor caster, float first, float second)
        {
            AOEs.Add(new(rect, caster.Position, spell.Rotation + first.Degrees(), Module.CastFinishAt(spell)));
            AOEs.Add(new(rect, caster.Position, spell.Rotation + second.Degrees(), Module.CastFinishAt(spell, 3.1f)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.LegitimateForceFirstL:
            case (uint)AID.LegitimateForceFirstR:
            case (uint)AID.LegitimateForceSecondL:
            case (uint)AID.LegitimateForceSecondR:
                ++NumCasts;
                if (AOEs.Count != 0)
                    AOEs.RemoveAt(0);
                break;
        }
    }
}
