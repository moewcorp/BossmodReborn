namespace BossMod.Dawntrail.Trial.T03QueenEternal;

class LegitimateForce(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(2);
    private static readonly AOEShapeRect rect = new(20f, 40f);
    private static readonly WDir offset1 = new(0f, 20f), offset2 = new(0f, 8f);
    private readonly Besiegement _aoe = module.FindComponent<Besiegement>()!;
    private static readonly Func<WPos, float> stayInBounds = p =>
        Math.Max(ShapeDistance.InvertedRect(T03QueenEternal.LeftSplitCenter + offset2, T03QueenEternal.LeftSplitCenter - offset2, 4f)(p),
            ShapeDistance.InvertedRect(T03QueenEternal.RightSplitCenter + offset2, T03QueenEternal.RightSplitCenter - offset2, 4f)(p));

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0 || _aoe.AOEs.Count != 0)
            return [];
        var compare = count > 1 && _aoes[0].Rotation != _aoes[1].Rotation;
        var max = count > 2 ? 2 : count;
        var aoes = new AOEInstance[count];
        var index = 0;
        for (var i = 0; i < max; ++i)
        {
            var aoe = _aoes[i];
            if (i == 0)
                aoes[index++] = compare ? aoe with { Color = Colors.Danger } : aoe;
            else if (i == 1 && compare)
                aoes[index++] = aoe with { Risky = false };
        }
        return aoes[..index];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.LegitimateForceLL:
                AddAOEs(caster, -90f, -90f);
                break;
            case (uint)AID.LegitimateForceLR:
                AddAOEs(caster, -90f, 90f);
                break;
            case (uint)AID.LegitimateForceRR:
                AddAOEs(caster, 90f, 90f);
                break;
            case (uint)AID.LegitimateForceRL:
                AddAOEs(caster, 90f, -90f);
                break;
        }

        void AddAOEs(Actor caster, float first, float second)
        {
            _aoes.Add(new(rect, caster.Position, spell.Rotation + first.Degrees(), Module.CastFinishAt(spell)));
            _aoes.Add(new(rect, caster.Position, spell.Rotation + second.Degrees(), Module.CastFinishAt(spell, 3.1f)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0)
            switch (spell.Action.ID)
            {
                case (uint)AID.LegitimateForceLL:
                case (uint)AID.LegitimateForceLR:
                case (uint)AID.LegitimateForceRR:
                case (uint)AID.LegitimateForceRL:
                case (uint)AID.LegitimateForceR:
                case (uint)AID.LegitimateForceL:
                    _aoes.RemoveAt(0);
                    break;
            }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        var besiege = _aoe.AOEs;
        var count = _aoes.Count;
        var besiegeCount = besiege.Count;
        var gravityBounds = Arena.Bounds == T03QueenEternal.SplitGravityBounds;
        if (count != 0 && Arena.Center != T03QueenEternal.SplitArena.Center || besiegeCount == 0 && count == 2 && gravityBounds)
            hints.AddForbiddenZone(ShapeDistance.InvertedRect(Arena.Center + offset1, Arena.Center - offset1, 3), _aoes[0].Activation);
        else if (count != 2 && besiegeCount == 0 && gravityBounds)
            hints.AddForbiddenZone(stayInBounds, count != 0 ? _aoes[0].Activation : besiegeCount != 0 ? besiege[0].Activation : default);
    }
}
